using UnityEngine;
using System.Collections;
using System;
using Zeptomoby.OrbitTools;

public class CameraMove3D : MonoBehaviour
{
    public float MaxLatitude = 50;
    public double MinAltitude = Globals.Xkmper*0.5;
    public double MaxAltitude = Globals.Xkmper*2;
    protected Geo org_geo, dst_geo;
    protected Vector3 old_mouse_pos, new_mouse_pos;
    protected Transform earth_pos;    
    protected DateTime org_t;
    protected float navigate_start, navigate_len;
    protected GameController game_ctrl;
    // Use this for initialization
    void Awake()
    {
        earth_pos = GameObject.FindWithTag("EarthSat3D").transform;
        game_ctrl = GameObject.Find("GameController").GetComponent<GameController>();
        navigate_len = 0;
    }

    void start_navigate(Geo dst_geo_)
    {
        if (navigate_len != 0)
            Debug.LogWarning("Start navigate is called during navigation");
        Debug.Log("Navigate to Lat:" + dst_geo_.LatitudeDeg +", Long:" + dst_geo_.LongitudeDeg);
        org_t = game_ctrl.getTime();
        Vector3 camera_vec = transform.position - earth_pos.position;
        org_geo = CoordChange3D.World2Geo(camera_vec, org_t);
        dst_geo = new Geo(dst_geo_);
        if (dst_geo.Altitude < MinAltitude)
            dst_geo.Altitude = MinAltitude;
        if (dst_geo.Altitude > MaxAltitude)
            dst_geo.Altitude = MaxAltitude;
        dst_geo.LatitudeRad = Mathf.Clamp((float)dst_geo.LatitudeRad, -MaxLatitude * Mathf.PI / 180, MaxLatitude * Mathf.PI / 180);
        navigate_len = Mathf.Acos(Mathf.Sin((float)org_geo.LatitudeRad) * Mathf.Sin((float) dst_geo.LatitudeRad) +
            Mathf.Cos((float)org_geo.LatitudeRad) * Mathf.Cos((float) dst_geo.LatitudeRad) *
            Mathf.Cos((float) (dst_geo.LongitudeRad - org_geo.LongitudeDeg)));
        navigate_start = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        Geo geo;        
        Vector3 camera_vec;

        if (navigate_len <=Mathf.Epsilon)
        {
            if (Input.GetMouseButtonDown(0))
            {
                org_t = game_ctrl.getTime();
                old_mouse_pos = Input.mousePosition;
                camera_vec = transform.position - earth_pos.position;
                org_geo = CoordChange3D.World2Geo(camera_vec, org_t);
            }
            if (Input.GetMouseButton(0))
            {
                new_mouse_pos = Input.mousePosition;
                geo = new Geo(org_geo);
                geo.LongitudeRad = org_geo.LongitudeRad - Math.PI * (Input.mousePosition.x - old_mouse_pos.x) / Screen.width;
                geo.LatitudeRad = org_geo.LatitudeRad - Math.PI * (Input.mousePosition.y - old_mouse_pos.y) / Screen.width;
                geo.LatitudeRad = Mathf.Clamp((float)geo.LatitudeRad, -MaxLatitude * Mathf.PI / 180, MaxLatitude * Mathf.PI / 180);
                //Debug.Log("camera latitude" + geo.LatitudeDeg + ",longitude" + geo.LongitudeDeg);
                camera_vec = CoordChange3D.Geo2World(geo, org_t);
                transform.position = earth_pos.position + camera_vec;
                transform.LookAt(earth_pos, earth_pos.forward);
            }
            if (Input.GetAxis("Mouse ScrollWheel") != 0)
            {
                org_t = game_ctrl.getTime();
                camera_vec = transform.position - earth_pos.position;
                geo = CoordChange3D.World2Geo(camera_vec, org_t);
                geo.Altitude -= CoordChange3D.scale * Input.GetAxis("Mouse ScrollWheel");
                if (geo.Altitude < MinAltitude)
                    geo.Altitude = MinAltitude;
                if (geo.Altitude > MaxAltitude)
                    geo.Altitude = MaxAltitude;
                camera_vec = CoordChange3D.Geo2World(geo, org_t);
                transform.position = earth_pos.position + camera_vec;
                transform.LookAt(earth_pos, earth_pos.forward);                
            }
            
            /*only for test
            if (Input.GetMouseButtonDown(1))
            {
                start_navigate(new Geo(0.5 ,2, Globals.Xkmper));
            }*/
        }
        else
        {
            Vector3 from = CoordChange3D.Geo2World(org_geo, org_t);
            Vector3 to = CoordChange3D.Geo2World(dst_geo, org_t);

            if (Time.time - navigate_start >= navigate_len)
            {
                navigate_len = 0;
                camera_vec = to;
            } else
                camera_vec = Vector3.Slerp(from, to, (Time.time - navigate_start) / navigate_len);
            transform.position = earth_pos.position + camera_vec;
            transform.LookAt(earth_pos, earth_pos.forward);
        }
    }
}
