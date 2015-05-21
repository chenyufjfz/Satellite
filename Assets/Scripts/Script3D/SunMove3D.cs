using UnityEngine;
using System.Collections;
using Zeptomoby.OrbitTools;
using System;

public class SunMove3D : MonoBehaviour {
    protected GameController game_ctrl;

	// Use this for initialization
    void Awake()
    {
        game_ctrl = GameObject.Find("GameController").GetComponent<GameController>();
    }

    void OnEnable()
    {
        StartCoroutine(UpdateSunPosition3d());
	}

    IEnumerator UpdateSunPosition3d()
    {
        for (; ; )
        {
            Julian t = new Julian(game_ctrl.getTime());
            double n = t.FromJan1_12h_2000();
            double L = 280.460 + 0.9856474 * n;
            double g = 357.528 + 0.9856003 * n;
            double lambda = L + 1.915 * Math.Sin(g * Math.PI / 180) + 0.020 * Math.Sin(g * Math.PI / 90); //Huang Jing
            int round = (int)(lambda / 360);
            lambda = lambda - round * 360;
            
            double yisilon = 23.439281;
            double alpha = Math.Atan(Math.Cos(yisilon * Math.PI / 180) * Math.Tan(lambda * Math.PI / 180));
            if (lambda > 90 && lambda < 270)
                alpha += Math.PI;  //Chi Jing 
            double beta = Math.Asin(Math.Sin(yisilon * Math.PI / 180) * Math.Sin(lambda * Math.PI / 180)); //Chi Wei
            //Debug.Log("Sun Right ascension(Chi Jing)=" + alpha * 180 / Math.PI + ",Declination(Chi Wei)=" +beta * 180 / Math.PI);
            Eci sun_eci = new Eci(new Vector(600000 * Math.Cos(beta) * Math.Cos(alpha), 600000 * Math.Cos(beta) * Math.Sin(alpha), 600000 * Math.Sin(beta)));
            transform.localPosition = CoordChange3D.Eci2World(sun_eci);
            Geo geo = new Geo(sun_eci, t);
            Debug.Log("SunMove3d Update Geo Latitude=" + geo.LatitudeDeg + ", Longitude=" +geo.LongitudeDeg);
            yield return new WaitForSeconds(65f);
        }
    }
}
