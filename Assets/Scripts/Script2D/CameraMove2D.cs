using UnityEngine;
using System.Collections;
using Zeptomoby.OrbitTools;

public class CameraMove2D : MonoBehaviour {
    protected Transform earth_pos;
    protected GameController game_ctrl;
    public double MinZ = 1;
    public double MaxZ = 10;
    public float boundry =5;
    protected Vector3 old_mouse_pos, new_mouse_pos;
    protected Vector3 old_world_pos, new_world_pos;
    protected float navigate_start, navigate_len;
    protected float prev_scroll_state, scroll_state;

	// Use this for initialization
    void Awake()
    {
        earth_pos = GameObject.FindWithTag("EarthSat2D").transform;
        game_ctrl = GameObject.Find("GameController").GetComponent<GameController>();
        navigate_len = 0;
    }
	
    void start_navigate(Geo dst_geo_)
    {
        new_world_pos = CoordChange2D.Geo2World(dst_geo_);
        new_world_pos.z = Mathf.Clamp(new_world_pos.z, (float)-MaxZ, (float)-MinZ);
        new_world_pos.x = Mathf.Clamp(new_world_pos.x, -new_world_pos.z - boundry * 2, boundry * 2 + new_world_pos.z);
        new_world_pos.y = Mathf.Clamp(new_world_pos.y, -new_world_pos.z / 2 - boundry, boundry + new_world_pos.z / 2); 
        old_world_pos = transform.position - earth_pos.position;
        navigate_len = Vector3.Distance(old_world_pos, new_world_pos) / 4;
        navigate_start = Time.time;
    }

	// Update is called once per frame
	void Update () 
    {
        Vector3 camera_vec;

        if (navigate_len == 0)
        {
            if (game_ctrl.ui_active)
                return;

            if (Input.GetMouseButtonDown(0))
            {
                old_mouse_pos = Input.mousePosition;
                old_world_pos = transform.position - earth_pos.position;
            }
            if (Input.GetMouseButton(0))
            {
                new_mouse_pos = Input.mousePosition;
                camera_vec = old_world_pos;
                camera_vec.x += 2 * old_world_pos.z * (Input.mousePosition.x - old_mouse_pos.x) / Screen.width;
                camera_vec.x = Mathf.Clamp(camera_vec.x, -camera_vec.z - boundry * 2, boundry * 2 + camera_vec.z);
                camera_vec.y += old_world_pos.z * (Input.mousePosition.y - old_mouse_pos.y) / Screen.height;
                camera_vec.y = Mathf.Clamp(camera_vec.y, -camera_vec.z / 2 - boundry, boundry + camera_vec.z / 2);
                transform.position = earth_pos.position + camera_vec;
            }
            prev_scroll_state = scroll_state;
            scroll_state = Input.GetAxis("Mouse ScrollWheel");
            if (scroll_state != 0)
            {
                camera_vec = transform.position - earth_pos.position;
                camera_vec.z += Input.GetAxis("Mouse ScrollWheel");
                camera_vec.z = Mathf.Clamp(camera_vec.z, (float)-MaxZ, (float)-MinZ);
                camera_vec.x = Mathf.Clamp(camera_vec.x, -camera_vec.z - boundry * 2, boundry * 2 + camera_vec.z);
                camera_vec.y = Mathf.Clamp(camera_vec.y, -camera_vec.z / 2 - boundry, boundry + camera_vec.z / 2);
                transform.position = earth_pos.position + camera_vec;
            }

            if (Input.GetMouseButtonUp(0) || (prev_scroll_state != 0 && scroll_state == 0))
            {
                for (int i = 0; i < game_ctrl.satshow_set.Count; i++)
                {
                    SatelliteShow2D sat = game_ctrl.satshow_set[i] as SatelliteShow2D;
                    if (sat.orbit_set != null)
                        for (int j = 0; j < sat.orbit_set.Count; j++ )
                            sat.orbit_set[j].Draw3D();

                    if (sat.range_set != null)
                        for (int j = 0; j < sat.range_set.Count; j++)
                            sat.range_set[j].Draw3D();
                    
                }
            }
        }
        else
        {
            if (Time.time - navigate_start >= navigate_len)
            {
                navigate_len = 0;
            }
            camera_vec = Vector3.Lerp(old_world_pos, new_world_pos, (Time.time - navigate_start) / navigate_len);
            transform.position = earth_pos.position + camera_vec;
        }
    }        
	
}
