﻿using UnityEngine;
using System.Collections;
using Zeptomoby.OrbitTools;
using System;
using System.Collections.Generic;

public class SatInfo
{
    public const UInt64 USA = 1;
    public const UInt64 RUS = 2;
    public const UInt64 EUROPE = 4;
    public const UInt64 CHINA = 8;
    public const UInt64 NOCOUNTRY =0x8000000000000000;
    public const UInt64 COMMUNICATION = 1;
    public const UInt64 WEATHER = 2;
    public const UInt64 NAVIGATION = 4;
    public const UInt64 NOTYPE1 = 0x8000000000000000;
    public Tle tle;
    public Color color;
    public UInt64 country, type1;
    public String type2;

    public static UInt64 CountryNum(String str)
    {
        switch (str)
        {
            case "CHINA": 
                return CHINA;
            case "USA":
                return USA;
            case "RUS":
                return RUS;
            case "EUROPE":
                return EUROPE;
        }
        return NOCOUNTRY;
    }
    public static UInt64 Type1Num(String str)
    {
        switch (str)
        {
            case "COMMUNICATION":
                return COMMUNICATION;
            case "WEATHER":
                return WEATHER;
            case "RUS":
                return RUS;
            case "NAVIGATION":
                return NAVIGATION;
        }
        return NOTYPE1;
    }

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
    public const int SHOW_INFO = 32;
    protected Dictionary<String, SatInfo> sat_tles;
    protected GameController game_ctrl;
	// Use this for initialization
    void Awake()
    {
        game_ctrl = GameObject.Find("GameController").GetComponent<GameController>();
        sat_tles = new Dictionary<String, SatInfo>();
    }
	
	// Update is called once per frame
	public void AddSatellite(String key, SatInfo satinfo) 
    {
        if (sat_tles.ContainsKey(key))
            throw new ArgumentException("Add tle fail! Satellite " + key + " already exist");        
        sat_tles.Add(key, satinfo);
        Debug.Log("SatInfo Container Add satellite: " + key);
	}

    public void ChangeSatellite(String key, SatInfo satinfo)
    {
        if (sat_tles.ContainsKey(key) == false)
            throw new ArgumentException("Change tle fail! Satellite " + key + " not exist");
        sat_tles[key] = satinfo;
    }

    public void AddOrChangeSatellite(String key, SatInfo satinfo)
    {        
        sat_tles[key] = satinfo;
    }

    public SatInfo getSatInfo(String key)
    {
        SatInfo satinfo;

        if (sat_tles.TryGetValue(key, out satinfo))
            return satinfo;
        else
            throw new ArgumentException("getSatInfo fail! Satellite " + key + " not exist");
    }

    public string [] SatFilter(string [] sat_keys, UInt64 country_mask, UInt64 type1_mask, string name_mask)
    {
        List <string> ret = new List<string> ();

        for (int i = 0; i < sat_keys.Length; i++)
        {
            SatInfo satinfo = getSatInfo(sat_keys[i]);
            if ((satinfo.country & country_mask) != 0 && (satinfo.type1 & type1_mask) != 0 && sat_keys[i].ToUpper().Contains(name_mask.ToUpper()))
                ret.Add(sat_keys[i]);
        }
        ret.Sort();
        return ret.ToArray();
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
            throw new ArgumentException("getEci fail! Satellite " + key + " not exist");
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
            throw new ArgumentException("getOrbitEci fail! Satellite " + key + " not exist");
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
            return satinfo.tle.Name;
        else
            throw new ArgumentException("getName fail! Satellite " + key + " not exist");
    }

    public Geo[] getCoverRangeGeo(string key, float angle, int resolution = 512)
    {
        Geo[] ret = new Geo[resolution + 1];
        
        Geo sat_position = getGeo(key);
        float bi = (float)((sat_position.Altitude + Globals.Xkmper) / Globals.Xkmper);
        float a;
        if (Mathf.Sin(angle) * bi >= 1)
            a = Mathf.PI / 2 - Mathf.Atan(1f / bi);
        else
            a = (float)Math.Asin(Mathf.Sin(angle) * bi) - angle;

        Vector3 star_fall = new Vector3(Mathf.Cos((float)sat_position.LatitudeRad) * Mathf.Cos((float)sat_position.LongitudeRad),
                Mathf.Cos((float)sat_position.LatitudeRad) * Mathf.Sin((float)sat_position.LongitudeRad),
                Mathf.Sin((float)sat_position.LatitudeRad));
        float b0_lat = (float)sat_position.LatitudeRad + a;
        Vector3 position = new Vector3(Mathf.Cos(b0_lat) * Mathf.Cos((float)sat_position.LongitudeRad),
                    Mathf.Cos(b0_lat) * Mathf.Sin((float)sat_position.LongitudeRad), Mathf.Sin(b0_lat));

        Quaternion rotate = new Quaternion(star_fall.x * Mathf.Sin(Mathf.PI / resolution), star_fall.y * Mathf.Sin(Mathf.PI / resolution),
            star_fall.z * Mathf.Sin(Mathf.PI / resolution), Mathf.Cos(Mathf.PI / resolution));

        for (int i = 0; i < resolution; i++)
        {
            if (Mathf.Abs(position.z) < 1)
                ret[i] = new Geo(Math.Asin(position.z), Math.Atan2(position.y, position.x), 20);
            else
            {
                if (position.z > 0)
                    ret[i] = new Geo(Mathf.PI / 2 - 0.0001, Math.Atan2(position.y, position.x), 20);
                else
                    ret[i] = new Geo(-Mathf.PI / 2 + 0.0001, Math.Atan2(position.y, position.x), 20);
            }

            position = rotate * position;
        }
        ret[resolution] = ret[0];
        return ret;        
    }

    public Eci[] getCoverRangeEci(string key, float angle, int resolution = 512)
    {        
        DateTime t = game_ctrl.getTime();
        Geo[] orbit_geo = getCoverRangeGeo(key, angle, resolution);
        Eci[] orbit_eci = new Eci[orbit_geo.Length];
        for (int i = 0; i < orbit_geo.Length; i++)
            orbit_eci[i] = new Eci(orbit_geo[i], new Julian(t));
        return orbit_eci;
    }
}
