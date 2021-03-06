﻿using UnityEngine;
using System.Collections;
using System;
using Vectrosity;
using Zeptomoby.OrbitTools;
using System.Collections.Generic;

public class SatelliteShow2D : SatelliteShow
{
    public int orbitLineResolution = 256;
    protected SunSatLight2D sun_sat_light_set;
    protected Transform earth2d;
    protected GameObject camera2d;
    protected SatInfoContainer sat_info;
    public List<VectorLine> orbit_set, range_set;
    protected float init_scale;
    protected string key;
    protected static int update_num;

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
        earth2d = GameObject.FindWithTag("EarthSat2D").transform;
        camera2d = GameObject.FindWithTag("Camera2D");
        sun_sat_light_set = GameObject.Find("SunSatLight2D").GetComponent<SunSatLight2D>();
        transform.parent = earth2d;
        sat_info = GameObject.Find("GameController").GetComponent<SatInfoContainer>();
        if (earth2d == null || camera2d == null || sat_info == null)
            Debug.LogError("can't find earth or camera or SatInfo");
        else
            Debug.Log("create satellite success");
        orbit_set = new List<VectorLine>();
        range_set = new List<VectorLine>();
        init_scale = transform.localScale.x;
    }

    IEnumerator UpdateSatlitePos()
    {    
#if false
        int light_idx = sun_sat_light_set.AddLightSource();
#endif
        for (; ; )
        {
            try
            {
                Geo sat_position = sat_info.getGeo(key);
                foreach (VectorLine orbit in orbit_set)
                {
                    VectorLine o = orbit;
                    VectorLine.Destroy(ref o);
                }
                orbit_set.Clear();
                foreach (VectorLine orbit in range_set)
                {
                    VectorLine o = orbit;
                    VectorLine.Destroy(ref o);
                }
                range_set.Clear();
                if ((show_state & SatInfoContainer.SHOW_SATELLITE) != 0)
                {
                    this.renderer.enabled = true;
                    SpriteRenderer r = renderer as SpriteRenderer;
                    r.color = main_color;
                    transform.localPosition = CoordChange2D.Geo2World(sat_position);
                    //Debug.Log("update Satellite 2D pos" + sat_position);
                }
                else
                    this.renderer.enabled = false;
                VectorLine.SetCamera(camera2d.camera);
#if flase
                if ((show_state & SatInfoContainer.SHOW_RANGE) != 0)
                {

                    float a0, a1, bi;
                    bi = (float) ((sat_position.Altitude + Globals.Xkmper) / Globals.Xkmper);
                    if (Mathf.Sin(look_angle0) * bi  >= 1)
                        a0 = Mathf.PI / 2 - Mathf.Atan(1f/bi) - 0.1f;
                    else
                        a0 = Mathf.Asin(Mathf.Sin(look_angle0) * bi) - look_angle0;
                    if (Mathf.Sin(look_angle1) * bi >= 1)
                        a1 = Mathf.PI / 2 - Mathf.Atan(1f / bi);
                    else
                        a1 = (float)Math.Asin(Mathf.Sin(look_angle1) * bi) - look_angle1;
                    
                    Color32 c = main_color;
                    c.a = 0;
                    
                    sun_sat_light_set.SetLight(light_idx, sat_position, new float [] {a1, a0}, c);
                }
                else
                {
                    sun_sat_light_set.DisableLightSource(light_idx);
                }
#else
                if ((show_state & SatInfoContainer.SHOW_RANGE) != 0)
                {
                    Geo[] range_geo = sat_info.getCoverRangeGeo(key, look_angle0);
                    List<Vector3> range = new List<Vector3>(range_geo.Length);
                    for (int i = 0; i < range_geo.Length; i++)
                    {
                        Vector3 coord_world = CoordChange2D.Geo2World(range_geo[i]);
                        if (range.Count >= 1)
                            if (Vector3.Distance(range[range.Count - 1], coord_world) > CoordChange2D.WorldWide/3)
                            {
                                Vector3 interpolate0, interpolate1;
                                
                                if (coord_world.x > range[range.Count - 1].x)
                                {
                                    interpolate0 = Vector3.Lerp(range[range.Count - 1], coord_world - new Vector3(CoordChange2D.WorldWide, 0, 0),
                                        Mathf.InverseLerp(range[range.Count - 1].x, coord_world.x - CoordChange2D.WorldWide, -CoordChange2D.WorldWide / 2));
                                    interpolate1 = Vector3.Lerp(range[range.Count - 1] + new Vector3(CoordChange2D.WorldWide, 0, 0), coord_world,
                                        Mathf.InverseLerp(range[range.Count - 1].x + CoordChange2D.WorldWide, coord_world.x, CoordChange2D.WorldWide / 2));
                                }
                                else
                                {
                                    interpolate0 = Vector3.Lerp(range[range.Count - 1], coord_world + new Vector3(CoordChange2D.WorldWide, 0, 0),
                                        Mathf.InverseLerp(range[range.Count - 1].x, coord_world.x + CoordChange2D.WorldWide, CoordChange2D.WorldWide / 2));
                                    interpolate1 = Vector3.Lerp(range[range.Count - 1] - new Vector3(CoordChange2D.WorldWide, 0, 0), coord_world,
                                        Mathf.InverseLerp(range[range.Count - 1].x - CoordChange2D.WorldWide, coord_world.x, -CoordChange2D.WorldWide / 2));
                                }
                                range.Add(interpolate0);
                                
                                if (range.Count == 1)
                                {
                                    throw new Exception("i=" + i + ",long[i]=" + range_geo[i].LongitudeRad + "long[i-1]=" + range_geo[i-1].LongitudeRad);
                                }
                                VectorLine line = new VectorLine("RangeLine2D", range.ToArray(), main_color, null, 1.0f, LineType.Continuous, Joins.Fill);
                                range_set.Add(line);
                                line.drawTransform = earth2d;
                                line.Draw3D();
                                range.Clear();
                                range.Add(interpolate1);
                            }
                        range.Add(coord_world);
                    }
                    if (range.Count >= 2)
                    {
                        VectorLine line = new VectorLine("RangeLine2D", range.ToArray(), main_color, null, 1.0f, LineType.Continuous, Joins.Fill);
                        range_set.Add(line);
                        line.drawTransform = earth2d;
                        line.Draw3D();
                    }
                }
#endif
                if ((show_state & SatInfoContainer.SHOW_ORBIT) != 0)
                {
                    Geo[] orbit_geo = sat_info.getOrbitGeo(key, orbitLineResolution);
                    List<Vector3> orbit = new List<Vector3>(orbitLineResolution);
                                        
                    for (int i = 0; i < orbit_geo.Length; i++)
                    {
                        Vector3 coord_world = CoordChange2D.Geo2World(orbit_geo[i]);
                        if (orbit.Count >= 1)
                            if (Vector3.Distance(orbit[orbit.Count - 1], coord_world) > CoordChange2D.WorldWide / 3)
                            {
                                Vector3 interpolate0, interpolate1;
                                interpolate1 = new Vector3(0, 0, 0);
                                
                                if (coord_world.x > orbit[orbit.Count - 1].x)
                                {
                                    interpolate0 = Vector3.Lerp(orbit[orbit.Count - 1], coord_world - new Vector3(CoordChange2D.WorldWide, 0, 0),
                                        Mathf.InverseLerp(orbit[orbit.Count - 1].x, coord_world.x - CoordChange2D.WorldWide, -CoordChange2D.WorldWide / 2));
                                    interpolate1 = Vector3.Lerp(orbit[orbit.Count - 1] + new Vector3(CoordChange2D.WorldWide, 0, 0), coord_world,
                                        Mathf.InverseLerp(orbit[orbit.Count - 1].x + CoordChange2D.WorldWide, coord_world.x, CoordChange2D.WorldWide / 2));
                                }
                                else
                                {
                                    interpolate0 = Vector3.Lerp(orbit[orbit.Count - 1], coord_world + new Vector3(CoordChange2D.WorldWide, 0, 0),
                                        Mathf.InverseLerp(orbit[orbit.Count - 1].x, coord_world.x + CoordChange2D.WorldWide, CoordChange2D.WorldWide / 2));
                                    interpolate1 = Vector3.Lerp(orbit[orbit.Count - 1] - new Vector3(CoordChange2D.WorldWide, 0, 0), coord_world,
                                        Mathf.InverseLerp(orbit[orbit.Count - 1].x - CoordChange2D.WorldWide, coord_world.x, -CoordChange2D.WorldWide / 2));
                                }
                                orbit.Add(interpolate0);
                                                                
                                VectorLine line = new VectorLine("OrbitLine2D", orbit.ToArray(), main_color, null, 1.0f, LineType.Continuous, Joins.Fill);
                                orbit_set.Add(line);
                                line.drawTransform = earth2d;
                                line.Draw3D();
                                orbit.Clear();
                                
                                orbit.Add(interpolate1);
                            }
                        orbit.Add(coord_world);
                    }
                    if (orbit.Count >= 2)
                    {
                        VectorLine line = new VectorLine("OrbitLine2D", orbit.ToArray(), main_color, null, 1.0f, LineType.Continuous, Joins.Fill);
                        orbit_set.Add(line);
                        line.drawTransform = earth2d;
                        line.Draw3D();
                    }
                }
            }
            catch (Exception e)
            {
                this.renderer.enabled = false;
                foreach (VectorLine orbit in orbit_set)
                {
                    VectorLine o = orbit;
                    VectorLine.Destroy(ref o);
                }
                orbit_set.Clear();
                foreach (VectorLine orbit in range_set)
                {
                    VectorLine o = orbit;
                    VectorLine.Destroy(ref o);
                }
                range_set.Clear();
                Debug.Log(e);
            }
            yield return new WaitForSeconds(1.5f + UnityEngine.Random.value * 1.5f);
            int no_update = 0;
            while (update_num > 0)
            {
                yield return null;
                if (++no_update > 30)
                    throw new Exception("satellite no update!");
            }

            update_num++;
        }
    }

    // Use this for initialization
    void Start()
    {
        StartCoroutine(UpdateSatlitePos());
    }

    void Update()
    {
    
        if ((show_state & SatInfoContainer.SHOW_SATELLITE) != 0)
        {
            float len = earth2d.position.z - camera2d.transform.position.z;
            float s = Mathf.Sqrt(Mathf.Sqrt(len * len * len)) * scale * init_scale;
            transform.localScale = new Vector3(s, s, 1);
        }

        update_num = 0;
    }

    void OnDisable()
    {
        foreach (VectorLine orbit in orbit_set)
        {
            VectorLine o = orbit;
            VectorLine.Destroy(ref o);
        }
        orbit_set.Clear();
        foreach (VectorLine orbit in range_set)
        {
            VectorLine o = orbit;
            VectorLine.Destroy(ref o);
        }
        range_set.Clear();
    }

    void OnGUI()
    {
        if ((show_state & SatInfoContainer.SHOW_NAME) != 0)
        {
            Vector3 screen_xy = camera2d.camera.WorldToScreenPoint(transform.position);
            GUI.contentColor = main_color;
            int dx, dy = 3;
            dx = (int)Mathf.Lerp(-80, 20, Mathf.InverseLerp(Screen.width / 4, Screen.width * 3 / 4, screen_xy.x));
            if (screen_xy.y > Screen.height / 2)
                dy = -24;
            GUI.Label(new Rect(screen_xy.x + dx, Screen.height - screen_xy.y + dy, sat_info.getName(key).Length * 10, 20), sat_info.getName(key));
        }
    }
}
