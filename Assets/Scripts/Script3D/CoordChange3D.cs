using UnityEngine;
using System.Collections;
using Zeptomoby.OrbitTools;
using System;

public class CoordChange3D {
    public const double scale = Globals.Xkmper / 10; //10 is earth3d units in view
	// Use this for initialization
    public static Eci World2Eci(Vector3 world)
    {
        return new Eci(new Vector(world.x *scale, -world.y *scale, world.z *scale));
    }

    public static Geo World2Geo(Vector3 world, DateTime t)
    {
        Eci eci = World2Eci(world);
        return new Geo(eci, new Julian(t));
    }

    public static Vector3 Eci2World(Eci eci)
    {
        return new Vector3((float)(eci.Position.X / scale), (float) (-eci.Position.Y / scale), (float)(eci.Position.Z / scale));
    }

    public static Vector3 Eci2World(Vector eci)
    {
        return new Vector3((float)(eci.X / scale), (float)(-eci.Y / scale), (float)(eci.Z / scale));
    }

    public static Vector3 Geo2World(Geo geo, DateTime t)
    {
        Eci eci = new Eci(geo, new Julian(t));
        return Eci2World(eci);
    }
}
