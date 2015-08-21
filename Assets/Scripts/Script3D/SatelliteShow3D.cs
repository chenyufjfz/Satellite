using UnityEngine;
using System.Collections;
using Vectrosity;
using Zeptomoby.OrbitTools;
using System.Collections.Generic;
using System;

public class SatelliteShow3D : SatelliteShow
{
    public int orbitLineResolution = 256;
    public float size = 0.1f;
    public double eccen;
    protected Transform earth3d;
    protected GameObject camera3d;
    protected SatInfoContainer sat_info;
    public VectorLine orbit_line, starfall_line, range_line;
    protected Renderer[] renderers;
    protected float initscale;
    protected static int update_num;
    protected string key;
    public override string Key
    {
        get { return key; }
        set { key = value; }
    }

    protected float scale;
    public override float Scale
    {
        get { return scale; }
        set { scale = value; }
    }

    protected int show_state;
    public override int ShowState
    {
        get { return show_state; }
        set { show_state = value; }
    }

    protected Color main_color;
    public override Color MainColor
    {
        get { return main_color; }
        set { main_color = value; }
    }

    protected float look_angle0;
    public override float LookAngle0
    {
        get { return look_angle0; }
        set { look_angle0 = value; }
    }

    protected float look_angle1;
    public override float LookAngle1
    {
        get { return look_angle1; }
        set { look_angle1 = value; }
    }

    void Awake()
    {
        earth3d = GameObject.FindWithTag("EarthSat3D").transform;
        camera3d = GameObject.FindWithTag("Camera3D");
        transform.parent = earth3d;
        sat_info = GameObject.Find("GameController").GetComponent<SatInfoContainer>();
        if (earth3d == null || camera3d == null || sat_info == null)
            Debug.LogError("SatelliteShow3D can't find earth or camera or SatInfo");
        else
            Debug.Log("SatelliteShow3D create success");
        renderers = GetComponentsInChildren<Renderer>();
        initscale = transform.localScale.x;
    }

    IEnumerator UpdateSatlitePos()
    {
        for (; ; )
        {
            try
            {
                VectorLine.Destroy(ref orbit_line);
                VectorLine.Destroy(ref starfall_line);
                VectorLine.Destroy(ref range_line);
                if ((show_state & SatInfoContainer.SHOW_SATELLITE) != 0)
                {
                    foreach (Renderer r in renderers)
                        r.enabled = true; 
                    transform.localPosition = CoordChange3D.Eci2World(sat_info.getEci(key));                    
                    transform.LookAt(earth3d.transform, Vector3.forward);                    
                }
                else
                {
                    foreach (Renderer r in renderers)
                        r.enabled = false;                                  
                }
                
                VectorLine.SetCamera(camera3d.camera);
                if ((show_state & SatInfoContainer.SHOW_RANGE) != 0)
                {                    
                    Eci [] range_eci = sat_info.getCoverRangeEci(key, look_angle0);
                    List<Vector3> range = new List<Vector3>(range_eci.Length);
                    for (int i = 0; i < range_eci.Length; i++)
                    {
                        Vector3 coord_world = CoordChange3D.Eci2World(range_eci[i]);
                        range.Add(coord_world);
                    }
                    range_line = new VectorLine("RangeLine3d", range.ToArray(), main_color, null, 2.0f, LineType.Continuous, Joins.Fill);
                    range_line.drawTransform = earth3d;
                    range_line.Draw3D();
                }
                
                if ((show_state & SatInfoContainer.SHOW_ORBIT) != 0)
                {
                    Eci[] orbit_eci = sat_info.getOrbitEci(key, orbitLineResolution);
                    List<Vector3> orbit = new List<Vector3>(orbitLineResolution);                    
#if true
                    double max=0, min=Globals.Xkmper *10;
#endif
                    for (int i = 0; i < orbit_eci.Length; i++)
                    {
#if true
                        max = Math.Max(max, orbit_eci[i].Position.Magnitude());
                        min = Math.Min(min, orbit_eci[i].Position.Magnitude());
#endif
                        Vector3 coord_world = CoordChange3D.Eci2World(orbit_eci[i]);                        
                        orbit.Add(coord_world);
                    }
                    orbit_line = new VectorLine("OrbitLine3D", orbit.ToArray(), main_color, null, 1.5f, LineType.Continuous, Joins.Fill);
                    orbit_line.drawTransform = earth3d;
                    orbit_line.Draw3D();
#if true
                    eccen = Math.Sqrt(1 - min * min / (max * max));
#endif
                }                
                
                if ((show_state & SatInfoContainer.SHOW_STARFALL) != 0)
                {
                    Vector3[] startfall = new Vector3[] { transform.position, earth3d.position };
                    starfall_line = new VectorLine("StarFall3D", startfall, main_color, null, 1.0f, LineType.Continuous, Joins.Fill);
                    starfall_line.drawTransform = earth3d;
                    starfall_line.Draw3D();
                } 
            }
            catch (Exception e)
            {
                foreach (Renderer r in renderers)
                    r.enabled = false;                
                VectorLine.Destroy(ref orbit_line);
                VectorLine.Destroy(ref starfall_line);
                VectorLine.Destroy(ref range_line);
                Debug.Log(e);
            }
            yield return new WaitForSeconds(1.5f + UnityEngine.Random.value*1.5f);
            int no_update=0;
            while (update_num > 0) 
            {
                yield return null;
                if (++no_update > 30)
                    throw new Exception("satellite no update!");
            }
               
            update_num++;
        }
    }

    void Start()
    {
        StartCoroutine(UpdateSatlitePos());
    }

    void Update()
    {
#if false
        if ((show_state & SatInfoContainer.SHOW_SATELLITE) != 0)
        {
            Vector3 sat_vec = transform.position - camera3d.transform.position;
            Vector3 camera_vec = earth3d.position - camera3d.transform.position;
            float len = Vector3.Project(sat_vec, camera_vec).magnitude;
            float s = Mathf.Sqrt(Mathf.Sqrt(len * len * len)) * scale * initscale;            
            transform.localScale = new Vector3(s, s, s);            
        }
#endif
        update_num=0;
    }

    void OnDisable()
    {
        VectorLine.Destroy(ref orbit_line);
        VectorLine.Destroy(ref starfall_line);        
        VectorLine.Destroy(ref range_line);
    }

    void OnGUI()
    {
        if ((show_state & SatInfoContainer.SHOW_NAME) != 0) 
        {
            RaycastHit hit;
            bool visible = false;
            if (Physics.Raycast(camera3d.transform.position, transform.position - camera3d.transform.position, out hit))
            {
                if (hit.collider.gameObject == this.gameObject)
                    visible = true;
            }
            if (visible)
            {
                Vector3 screen_xy = camera3d.camera.WorldToScreenPoint(transform.position);                
                GUI.contentColor = main_color;
                int dx, dy = 2;
                dx = (int)Mathf.Lerp(-80, 20, Mathf.InverseLerp(Screen.width / 4, Screen.width * 3 / 4, screen_xy.x));
                if (screen_xy.y > Screen.height / 2)
                    dy = -24;
                GUI.Label(new Rect(screen_xy.x + dx, Screen.height - screen_xy.y + dy, sat_info.getName(key).Length*10, 20), sat_info.getName(key));
            }   
        }

        if ((show_state & SatInfoContainer.SHOW_INFO) != 0)
        {
            Orbit orbit= sat_info.getOrbit(key);
            Geo geo = sat_info.getGeo(key);
            int x0 = 20;
            int y0 = Screen.height * 3 / 4;
            GUI.contentColor = Color.white;
            int high = 18;
#if true
            GUI.Label(new Rect(x0, y0 + high, 120, high), "eccentricity:" + eccen.ToString("f4"));
#else
            GUI.Label(new Rect(x0, y0, 120, 15), "偏心率:" + orbit.Eccentricity);
#endif
            GUI.Label(new Rect(x0, y0 + high * 2, 120, high), "Inclination:" + orbit.Inclination * 180 / Math.PI);
            bool latNorth = (geo.LatitudeRad >= 0.0);            
            if (geo.LongitudeRad > Math.PI)
                geo.LongitudeRad = geo.LongitudeRad - Math.PI*2;
            bool lonEast = (geo.LongitudeRad >= 0.0);
            string str = string.Format("{0:00.0}{1} ", Math.Abs(geo.LatitudeDeg), (latNorth ? 'N' : 'S'));
            str += string.Format("{0:000.0}{1} ",  Math.Abs(geo.LongitudeDeg), (lonEast ? 'E' : 'W'));
            GUI.Label(new Rect(x0, y0 + high * 3, 150, high), "Lon&Lat:" + str);
            GUI.Label(new Rect(x0, y0 + high * 4, 165, high), "Altitude:" + geo.Altitude.ToString("f0") + "km");
            GUI.Label(new Rect(x0, y0 + high * 5, 80 + sat_info.getName(key).Length*8, high), "Name:" + sat_info.getName(key));
            //GUI.Label(new Rect(x0, y0 + 80, 165, 18), "周期:" + orbit.Period);
        }
    }
}
