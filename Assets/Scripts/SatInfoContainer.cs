using UnityEngine;
using System.Collections;
using Zeptomoby.OrbitTools;
using System;
using System.Collections.Generic;

public class SatInfo
{
    public Tle tle;
    public String name;
}


public class SatelliteShow : MonoBehaviour
{
    public virtual string Key { get; set; }
    public virtual float Scale { get; set; }
    public virtual int ShowState { get; set; }
    public virtual Color MainColor { get; set; }
    public virtual float LookAngle0 { get; set; }
    public virtual float LookAngle1 { get; set; }
}

public class SatInfoContainer : MonoBehaviour {
    public const int SHOW_SATELLITE = 1;
    public const int SHOW_ORBIT = 2;
    public const int SHOW_STARFALL = 4;
    public const int SHOW_NAME = 8;
    public const int SHOW_RANGE = 16;
    protected Dictionary<String, SatInfo> sat_tles;
    protected GameController game_ctrl;
	// Use this for initialization
    void Awake()
    {
        game_ctrl = GameObject.Find("GameController").GetComponent<GameController>();
        sat_tles = new Dictionary<String, SatInfo>();
    }
	
	// Update is called once per frame
	public void AddSatellite(String key, Tle tle, String name=null) 
    {
        if (sat_tles.ContainsKey(key))
            throw new ArgumentException("Add tle fail! Satellite " + key + " already exist");
        SatInfo satinfo = new SatInfo();
        satinfo.tle = tle;
        if (name != null)
            satinfo.name = name;
        else
            satinfo.name = key;
        sat_tles.Add(key, satinfo);
        Debug.Log("SatInfo Container Add satellite: " + key);
	}

    public void ChangeSatelliteTle(String key, Tle tle)
    {
        if (sat_tles.ContainsKey(key) == false)
            throw new ArgumentException("Change tle fail! Satellite " + key + " not exist");
        sat_tles[key].tle = tle;
    }

    public void AddOrChangeSatellite(String key, Tle tle, String name=null)
    {
        SatInfo satinfo = new SatInfo();
        satinfo.tle = tle;
        if (name != null)
            satinfo.name = name;
        else
            satinfo.name = key;
        sat_tles[key] = satinfo;
    }

    public Eci getEci(String key)
    {
        SatInfo satinfo;

        if (sat_tles.TryGetValue(key, out satinfo))
        {
            Tle tle = satinfo.tle;
            Satellite sat;
            sat = new Satellite(tle);
            DateTime t = game_ctrl.getTime();
            EciTime eci_coord = sat.PositionEci(t);
            return eci_coord;
        }
        else
        {
            throw new ArgumentException("getEci fail! Satellite " + key + " not exist");
        }
    }

    public Eci[] getOrbitEci(string key, int orbitLineResolution=180)
    {
        SatInfo satinfo;
        Eci[] orbit_eci = new Eci[orbitLineResolution+1];

        if (sat_tles.TryGetValue(key, out satinfo))
        {
            Tle tle = satinfo.tle;
            Satellite sat;
            sat = new Satellite(tle);
            TimeSpan step = new TimeSpan(sat.Orbit.Period.Ticks / orbitLineResolution);
            DateTime t = game_ctrl.getTime();

            for (int i = 0; i < orbitLineResolution; i++, t += step)
            {
                EciTime ecitime = sat.PositionEci(t);
                orbit_eci[i] = ecitime;                            
            }
            orbit_eci[orbitLineResolution] = orbit_eci[0];
            return orbit_eci;
        }
        else
        {
            throw new ArgumentException("getOrbitEci fail! Satellite " + key + " not exist");
        }
    }

    public Geo getGeo(String key)
    {
        SatInfo satinfo;

        if (sat_tles.TryGetValue(key, out satinfo))
        {
            Tle tle = satinfo.tle;
            Satellite sat;
            sat = new Satellite(tle);
            DateTime t = game_ctrl.getTime();
            EciTime eci_coord = sat.PositionEci(t);
            Geo geo = new Geo(eci_coord, new Julian(t));
            return geo;
        }
        else
        {
            throw new ArgumentException("getEci fail! Satellite " + key + " not exist");
        }
        
    }

    public Geo[] getOrbitGeo(string key, int orbitLineResolution)
    {
        SatInfo satinfo;
        Geo[] orbit_geo = new Geo[orbitLineResolution + 1];

        if (sat_tles.TryGetValue(key, out satinfo))
        {
            Tle tle = satinfo.tle;
            Satellite sat;
            sat = new Satellite(tle);
            TimeSpan step = new TimeSpan(sat.Orbit.Period.Ticks / orbitLineResolution);
            DateTime t = game_ctrl.getTime();
            DateTime org_t = t;

            for (int i = 0; i < orbitLineResolution; i++, t += step)
            {
                EciTime ecitime = sat.PositionEci(t);
                orbit_geo[i] = new Geo(ecitime, new Julian(org_t));
            }
            orbit_geo[orbitLineResolution] = orbit_geo[0];
            return orbit_geo;
        }
        else
        {
            throw new ArgumentException("getOrbitEci fail! Satellite " + key + " not exist");
        }
    }

    public Orbit getOrbit(string key)
    {
        SatInfo satinfo;
        if (sat_tles.TryGetValue(key, out satinfo)) 
        {
            Tle tle = satinfo.tle;
            return new Orbit(tle);
        }        
        else
            throw new ArgumentException("getName fail! Satellite " + key + " not exist");
    }

    public String getName(string key)
    {
        SatInfo satinfo;
        if (sat_tles.TryGetValue(key, out satinfo))
            return satinfo.name;
        else
            throw new ArgumentException("getName fail! Satellite " + key + " not exist");
    }
}
