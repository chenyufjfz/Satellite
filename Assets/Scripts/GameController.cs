using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;


public class GameController : MonoBehaviour {

    public SatelliteShow2D sat2d_prefab;
    public SatelliteShow3D sat3d_prefab;

    protected GameObject earth3d;
    protected GameObject earth2d;
    protected GameObject camera3d;
    protected GameObject camera2d;
    protected SatInfoContainer sat_info_container;
    protected List <SatelliteShow>  satshow_set;

    public enum Mode
    {
        Mode3D,
        Mode2D
    };

    [SerializeField, SetProperty("mode")]
    private Mode _mode;
    public Mode mode
    {
        get { return _mode; }
        set {            
            _mode = value;
            Debug.Log("set mode call" + _mode);
            if (_mode == Mode.Mode3D)
            {                
                earth2d.SetActive(false);
                camera2d.SetActive(false);
                earth3d.SetActive(true);
                camera3d.SetActive(true);

                for (int i = 0; i < satshow_set.Count; i++)
                {
                    SatelliteShow satshow = satshow_set[i];
                    string key = satshow.Key;
                    float scale = satshow.Scale;
                    int showstate = satshow.ShowState;
                    Color maincolor = satshow.MainColor;
                    float lookangle0 = satshow.LookAngle0;
                    float lookangle1 = satshow.LookAngle1;
                    Destroy(satshow.gameObject);
                    satshow = Instantiate(sat3d_prefab) as SatelliteShow3D;
                    satshow.Key = key;
                    satshow.Scale = scale;
                    satshow.ShowState = showstate;
                    satshow.MainColor = maincolor;
                    satshow.LookAngle0 = lookangle0;
                    satshow.LookAngle1 = lookangle1;
                    satshow_set[i] = satshow;
                }
            }
            else
            {                
                earth3d.SetActive(false);
                camera3d.SetActive(false);
                earth2d.SetActive(true);
                camera2d.SetActive(true);

                for (int i = 0; i < satshow_set.Count; i++)
                {
                    SatelliteShow satshow = satshow_set[i];
                    string key = satshow.Key;
                    float scale = satshow.Scale;
                    int showstate = satshow.ShowState;
                    Color maincolor = satshow.MainColor;
                    float lookangle0 = satshow.LookAngle0;
                    float lookangle1 = satshow.LookAngle1;
                    Destroy(satshow.gameObject);
                    satshow = Instantiate(sat2d_prefab) as SatelliteShow2D;
                    satshow.Key = key;
                    satshow.Scale = scale;
                    satshow.ShowState = showstate;
                    satshow.MainColor = maincolor;
                    satshow.LookAngle0 = lookangle0;
                    satshow.LookAngle1 = lookangle1;
                    satshow_set[i] = satshow;
                }
            }
        }
    }

    public DateTime getTime()
    {
        return DateTime.UtcNow;
    }

    void Awake()
    {
        sat_info_container = GetComponent<SatInfoContainer>();
        earth3d = GameObject.FindWithTag("EarthSat3D");
        earth2d = GameObject.FindWithTag("EarthSat2D");
        camera3d = GameObject.FindWithTag("Camera3D");
        camera2d = GameObject.FindWithTag("Camera2D");
        if (_mode == Mode.Mode3D)
        {
            earth2d.SetActive(false);
            camera2d.SetActive(false);
            earth3d.SetActive(true);
            camera3d.SetActive(true);
        }
        else
        {
            earth2d.SetActive(true);
            camera2d.SetActive(true);
            earth3d.SetActive(false);
            camera3d.SetActive(false);
        }
        satshow_set = new List<SatelliteShow>();
    }

	// Use this for initialization
	void Start () {        
        sat_info_container.AddSatellite("2015-020A", new Zeptomoby.OrbitTools.Tle("2015-020A",
            "1 40552U 15020A   15094.66673760  .00000010  00000-0  00000+0 0  9994",
            "2 40552  82.4857  60.1813 0007346 326.0885  33.9725 12.40883922   506"), "卫星2015-020A");
        sat_info_container.AddSatellite("2015-020B", new Zeptomoby.OrbitTools.Tle("2015-020B",
            "1 40553U 15020B   15094.74646574  .00000010  00000-0  00000+0 0  9997",
            "2 40553  82.4888  60.1386 0007293 314.1846  45.8632 12.41155411   518"));
        sat_info_container.AddSatellite("AAUSAT3", new Zeptomoby.OrbitTools.Tle("AAUSAT3",
            "1 39087U 13009B   15094.76608532  .00000893  00000-0  32869-3 0  9997",
            "2 39087  98.6084 293.4137 0012655 160.8253 199.3417 14.35022481110126"));

        SatelliteShow satshow;
        if (_mode == Mode.Mode2D)
            satshow = Instantiate(sat2d_prefab) as SatelliteShow2D;            
        else        
            satshow = Instantiate(sat3d_prefab) as SatelliteShow3D;
        
        satshow.Scale = 1f;
        satshow.Key = "2015-020A";
        satshow.ShowState = SatInfoContainer.SHOW_SATELLITE | SatInfoContainer.SHOW_ORBIT | SatInfoContainer.SHOW_STARFALL | SatInfoContainer.SHOW_NAME | SatInfoContainer.SHOW_RANGE;
        satshow.MainColor = Color.red;
        satshow.LookAngle0 = Mathf.PI / 4;
        satshow.LookAngle1 = Mathf.PI / 3;
        satshow_set.Add(satshow);

        if (_mode == Mode.Mode2D)
            satshow = Instantiate(sat2d_prefab) as SatelliteShow2D;
        else
            satshow = Instantiate(sat3d_prefab) as SatelliteShow3D;
        satshow.Scale = 1f;
        satshow.Key = "AAUSAT3";
        satshow.ShowState = SatInfoContainer.SHOW_SATELLITE | SatInfoContainer.SHOW_ORBIT | SatInfoContainer.SHOW_NAME | SatInfoContainer.SHOW_RANGE;
        satshow.MainColor = Color.blue;
        satshow.LookAngle0 = Mathf.PI / 4;
        satshow.LookAngle1 = Mathf.PI / 3;
        satshow_set.Add(satshow);
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray;
            if (_mode == Mode.Mode2D)
                ray = camera2d.camera.ScreenPointToRay(Input.mousePosition);
            else
                ray = camera3d.camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            int hit_sat = -1;
            if (Physics.Raycast(ray, out hit))
            {
                for (int i = 0; i < satshow_set.Count; i++)
                    if (hit.collider.gameObject == satshow_set[i].gameObject)
                    {
                        satshow_set[i].ShowState |= SatInfoContainer.SHOW_STARFALL;
                        hit_sat = i;
                    }
                if (hit_sat!=-1)
                    for (int i=0; i<satshow_set.Count; i++)
                        if (i!=hit_sat)
                            satshow_set[i].ShowState &= ~SatInfoContainer.SHOW_STARFALL;
            }
        }
	}

    public void onChangeMode()
    {
        if (_mode == Mode.Mode3D)
            mode = Mode.Mode2D;
        else
            mode = Mode.Mode3D;
    }

}
