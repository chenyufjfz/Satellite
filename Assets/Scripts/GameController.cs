using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using Zeptomoby.OrbitTools;

public class GameController : MonoBehaviour {

    public SatelliteShow2D sat2d_prefab;
    public SatelliteShow3D sat3d_prefab;
    public int show_sat_max_num = 10;
    protected GameObject earth3d;
    protected GameObject earth2d;
    protected GameObject camera3d;
    protected GameObject camera2d;
    protected SatInfoContainer sat_info_container;
    protected SatDB sat_db;
    public List<SatelliteShow> satshow_set; //is identified by satshow_keys
    protected List <string> satshow_keys, satunshow_keys;
    protected string[] satshow_filter_keys, satunshow_filter_keys;
    public Toggle toggle_all_country, toggle_usa, toggle_rus, toggle_eu, toggle_other_country;
    public Toggle toggle_all_type, toggle_weather, toggle_comm, toggle_navigate, toggle_other_type;
    public Text text_name;
    public GameObject config_panel, number_exceel_msg;
    public DragBoxAbstract satshow_filter, satunshow_filter;
    protected UInt64 country_mask, type_mask;
    protected string name_mask;
    public bool ui_active
    {
        get { return config_panel.activeSelf; }
    }
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
        sat_db = GetComponent<SatDB>();
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
        satshow_filter.notify_game_ctrl = ShowBoxNotify;
        satunshow_filter.notify_game_ctrl = UnshowBoxNotify;
        name_mask = "";
    }

	// Use this for initialization
	void Start () {        
#if flase
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
        satshow.LookAngle1 = Mathf.PI / 3.7f;
        satshow_set.Add(satshow);

        if (_mode == Mode.Mode2D)
            satshow = Instantiate(sat2d_prefab) as SatelliteShow2D;
        else
            satshow = Instantiate(sat3d_prefab) as SatelliteShow3D;
        satshow.Scale = 1f;
        satshow.Key = "AAUSAT3";
        satshow.ShowState = SatInfoContainer.SHOW_SATELLITE | SatInfoContainer.SHOW_ORBIT | SatInfoContainer.SHOW_NAME | SatInfoContainer.SHOW_RANGE;
        satshow.MainColor = Color.blue;
        satshow.LookAngle0 = Mathf.PI / 3.7f;
        satshow.LookAngle1 = Mathf.PI / 3;
        satshow_set.Add(satshow);
#endif
        SatInfo [] satinfos = sat_db.GetAllSatInfo();
        satshow_keys = new List <string>();
        satunshow_keys = new List<string>();

        for (int i = 0; i < satinfos.Length; i++)
        {
            satunshow_keys.Add(satinfos[i].tle.Name);         
            sat_info_container.AddSatellite(satinfos[i].tle.Name, satinfos[i]);
        }
        onChangeMask();
        if (GameObject.Find("Logo")!=null) 
        {
            config_panel.SetActive(false);
            StartCoroutine(HideLogo());
        }
        number_exceel_msg.GetComponent<Text>().text += show_sat_max_num;
        number_exceel_msg.SetActive(false);
        
	}
	
    IEnumerator HideLogo()
    {
        yield return new WaitForSeconds(1.6f);
        GameObject.Find("Logo").SetActive(false);
        config_panel.SetActive(true);
    }

    IEnumerator ShowNumberExceedMsg()
    {
        config_panel.SetActive(false);
        number_exceel_msg.SetActive(true);        
        yield return new WaitForSeconds(1.5f);
        number_exceel_msg.SetActive(false);
        config_panel.SetActive(true);
    }

    IEnumerator DelayNameMaskUpdate()
    {
        yield return new WaitForSeconds(0.05f);
        if (text_name != null)
            name_mask = text_name.text;
        else
            name_mask = "";
        UpdateFilterShow();
    }

	// Update is called once per frame
	void Update () 
    {
        if (Input.GetMouseButtonDown(0))
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
                        satshow_set[i].ShowState |= SatInfoContainer.SHOW_INFO;
                        hit_sat = i;
                    }
                if (hit_sat!=-1)
                    for (int i=0; i<satshow_set.Count; i++)
                        if (i!=hit_sat)
                            satshow_set[i].ShowState &= ~SatInfoContainer.SHOW_INFO;
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

    protected void UpdateFilterShow()
    {
        satshow_filter_keys = sat_info_container.SatFilter(satshow_keys.ToArray(), country_mask, type_mask, name_mask);
        satunshow_filter_keys = sat_info_container.SatFilter(satunshow_keys.ToArray(), country_mask, type_mask, name_mask);
        satunshow_filter.SetText(satunshow_filter_keys);
        satshow_filter.SetText(satshow_filter_keys);
    }

    protected void UpdateSatellite()
    {
        int i;
        for (i = 0; i < satshow_set.Count; i++)
            Destroy(satshow_set[i].gameObject);
        satshow_set.Clear();
        for (i = 0; i < satshow_keys.Count; i++)
        {
            SatelliteShow satshow;
            if (_mode == Mode.Mode2D)
                satshow = Instantiate(sat2d_prefab) as SatelliteShow2D;
            else
                satshow = Instantiate(sat3d_prefab) as SatelliteShow3D;

            satshow.Scale = 1f;
            satshow.Key = satshow_keys[i];
            satshow.ShowState = SatInfoContainer.SHOW_SATELLITE | SatInfoContainer.SHOW_ORBIT | SatInfoContainer.SHOW_NAME;
            satshow.MainColor = sat_info_container.getSatInfo(satshow_keys[i]).color;
            satshow.LookAngle0 = Mathf.PI / 4;
            satshow_set.Add(satshow);
        }
    }

    public void onChangeNamemask()
    {
        StartCoroutine(DelayNameMaskUpdate());
    }
    public void onChangeMask()
    {
        if (toggle_all_country!=null && toggle_all_country.isOn)
            country_mask = 0xffffffffffffffff;
        else
            country_mask = 0;
        if (toggle_usa!=null && toggle_usa.isOn)
            country_mask |= SatInfo.USA;
        if (toggle_rus!=null && toggle_rus.isOn)
            country_mask |= SatInfo.RUS;
        if (toggle_eu!=null && toggle_eu.isOn)
            country_mask |= SatInfo.EUROPE;
        if (toggle_other_country!=null && toggle_other_country.isOn)
            country_mask |= ~(SatInfo.USA | SatInfo.RUS | SatInfo.EUROPE);
        if (toggle_all_type!=null && toggle_all_type.isOn)
            type_mask = 0xffffffffffffffff;
        else
            type_mask = 0;
        if (toggle_weather!=null && toggle_weather.isOn)
            type_mask |= SatInfo.WEATHER;
        if (toggle_comm!=null && toggle_comm.isOn)
            type_mask |= SatInfo.COMMUNICATION;
        if (toggle_navigate!=null && toggle_navigate.isOn)
            type_mask |= SatInfo.NAVIGATION;
        if (toggle_other_type!=null && toggle_other_type.isOn)
            type_mask |= ~(SatInfo.WEATHER | SatInfo.COMMUNICATION | SatInfo.NAVIGATION);
                
        UpdateFilterShow();
        Debug.Log("Country mask=" + country_mask + ", Type mask=" + type_mask +",Name mask=" + name_mask);
    }

    public void EndUI()
    {
        config_panel.SetActive(false);
        UpdateSatellite();
    }

    public void StartUI()
    {
        config_panel.SetActive(true);
    }

    public void ShowBoxNotify(int [] line_click)
    {
        for (int i=0; i<line_click.Length; i++)
        {
            satunshow_keys.Add(satshow_filter_keys[line_click[i]]);
            satshow_keys.Remove(satshow_filter_keys[line_click[i]]);
        }
        UpdateFilterShow();
    }

    public void UnshowBoxNotify(int [] line_click)
    {
        for (int i = 0; i < line_click.Length; i++)
        {
            if (satshow_keys.Count >= show_sat_max_num)
                break;
            satshow_keys.Add(satunshow_filter_keys[line_click[i]]);
            satunshow_keys.Remove(satunshow_filter_keys[line_click[i]]);            
        }        
        UpdateFilterShow();
        if (satshow_keys.Count >= show_sat_max_num)
            StartCoroutine(ShowNumberExceedMsg());
    }

    public void onQuit()
    {
        Application.Quit();
    }
#if false
    public void onChangeSatelliteNum()
    {
        for (int i = 0; i < satshow_set.Count; i++)
            Destroy(satshow_set[i].gameObject);
        satshow_set.Clear();
        int sat_num = Convert.ToInt32(satnum_infield.text);
        satnum_infield.text = sat_num.ToString();
        for (int i = 0; i < sat_num; i++)
        {
            SatelliteShow satshow;
            if (_mode == Mode.Mode2D)
                satshow = Instantiate(sat2d_prefab) as SatelliteShow2D;
            else
                satshow = Instantiate(sat3d_prefab) as SatelliteShow3D;

            satshow.Scale = 1f;
            satshow.Key = sat_name_list[i];
            satshow.ShowState = SatInfoContainer.SHOW_SATELLITE | SatInfoContainer.SHOW_ORBIT | SatInfoContainer.SHOW_NAME | SatInfoContainer.SHOW_RANGE;
            satshow.MainColor = sat_info_container.getSatInfo(sat_name_list[i]).color;
            satshow.LookAngle0 = Mathf.PI / 4;
            satshow_set.Add(satshow);
        }
    }
#endif
}
