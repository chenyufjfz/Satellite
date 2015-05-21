using UnityEngine;
using Zeptomoby.OrbitTools;
using System.Collections;
using System;

public class SunMove2D : MonoBehaviour
{
    protected GameController game_ctrl;
    protected SunSatLight2D sun_sat_light_set;
    public int SunRiseDownResolution = 256;

    // Use this for initialization
    void Awake()
    {
        game_ctrl = GameObject.Find("GameController").GetComponent<GameController>();
        sun_sat_light_set = GameObject.Find("SunSatLight2D").GetComponent<SunSatLight2D>();
    }

    void OnEnable()
    {
        StartCoroutine(UpdateSunPosition2d());
    }

/*
z' = cos & z + sin & y
x' = x
y' = cos & y - sin & z

z = r sin w
x = r cos w cos j
y = r cos w sin j
r'sinw' = r sinw cos& + r cosw sinj sin&
r' cosw' cosj' = r cosw cosj
r' cosw' sinj' = r cosw sinj cos& - r sinw sin&

w=0
r'sinw' = r sinj sin&
r'cosw'cosj' = r cosj
r'cosw'sinj' = r sinj cos&
*/
    protected Geo ComputeSunGeo()
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
            alpha += Math.PI; //Chi Jing
        double beta = Math.Asin(Math.Sin(yisilon * Math.PI / 180) * Math.Sin(lambda * Math.PI / 180));  //Chi Wei     
        Eci sun_eci = new Eci(new Vector(600000 * Math.Cos(beta) * Math.Cos(alpha), 600000 * Math.Cos(beta) * Math.Sin(alpha), 600000 * Math.Sin(beta)));
        Geo geo = new Geo(sun_eci, t);
        Debug.Log("Sun Geo Latitude=" + geo.LatitudeDeg + ", Longitude=" + geo.LongitudeDeg);
        return geo;
    }


    IEnumerator UpdateSunPosition2d()
    {
        int light_idx = sun_sat_light_set.AddLightSource();
        for (; ; )
        {
            Geo sun_geo = ComputeSunGeo();
#if false
            float [] alpha = new float [4] {1.57f, 1.55f, 1.53f, 1.51f};
#else
            float[] alpha = new float[2] { 1.0f, 0.3f };
#endif
            sun_sat_light_set.SetLight(light_idx, sun_geo, alpha, new Color32(0, 0, 0, 120), 256);
            yield return new WaitForSeconds(20.15f);
        }
    }
}
