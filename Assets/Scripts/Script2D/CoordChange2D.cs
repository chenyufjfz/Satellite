using UnityEngine;
using Zeptomoby.OrbitTools;
using System.Collections;
using System;

public class CoordChange2D {
    public const float WorldHigh = 9; //9 is earth2d units in view
    public const float WorldWide = 18;
    public const double ScaleY = Math.PI / WorldHigh;
    public const double ScaleX = Math.PI * 2 / WorldWide; 
    public const double ScaleAlt = Globals.Xkmper/0.01;
    public const int TexWidth = 512;
    public const int TexHeight = 256;
    public static Vector2 ScaleUV = new Vector2(Mathf.PI * 2 / (TexWidth-1), Mathf.PI / (TexHeight-1));

	// Use this for initialization
    public static Eci World2Eci(Vector3 world, DateTime t)
    {
        Geo geo = World2Geo(world);
        return new Eci(geo, new Julian(t));
    }

    public static Geo World2Geo(Vector3 world)
    {        
        return new Geo(world.y *ScaleY, world.x *ScaleX, -world.z * ScaleAlt);
    }

    public static Vector3 Eci2World(Eci eci, DateTime t)
    {
        Geo geo = new Geo(eci, new Julian(t));
        return Geo2World(geo);
    }

    public static Vector3 Geo2World(Geo geo)
    {        
        if (geo.LongitudeRad>Math.PI)
            return new Vector3((float)((geo.LongitudeRad - Math.PI*2) / ScaleX), (float)(geo.LatitudeRad / ScaleY), 
                (float) (-geo.Altitude /ScaleAlt));
        else        
            return new Vector3((float) (geo.LongitudeRad /ScaleX), (float) (geo.LatitudeRad /ScaleY),
                (float)(-geo.Altitude / ScaleAlt));
    }

    // Use this for initialization
    public static Eci Texuv2Eci(Vector2 uv, DateTime t)
    {
        Geo geo = Texuv2Geo(uv);
        return new Eci(geo, new Julian(t));
    }

    public static Geo Texuv2Geo(Vector2 uv)
    {
        return new Geo((uv.y - TexHeight / 2 + 0.5f) * ScaleUV.y, (uv.x - TexWidth / 2 + 0.5f) * ScaleUV.x, 0);
    }

    public static Vector2 Eci2Texuv(Eci eci, DateTime t)
    {
        Geo geo = new Geo(eci, new Julian(t));
        return Geo2Texuv(geo);
    }

    public static Vector2 Geo2Texuv(Geo geo)
    {
        if (geo.LongitudeRad > Math.PI)
            return new Vector2(TexWidth / 2 + (float)(geo.LongitudeRad - Math.PI*2) / ScaleUV.x -0.5f,
            TexHeight / 2 + (float)(geo.LatitudeRad / ScaleUV.y) -0.5f);
        else
            return new Vector2(TexWidth / 2 + (float)geo.LongitudeRad / ScaleUV.x-0.5f,
            TexHeight / 2 + (float)(geo.LatitudeRad / ScaleUV.y) -0.5f);
    }
}
