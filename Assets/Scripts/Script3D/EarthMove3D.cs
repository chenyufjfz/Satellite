using UnityEngine;
using System.Collections;
using System;
using Zeptomoby.OrbitTools;

public class EarthMove3D : MonoBehaviour
{
    protected float init_rotate;
    protected GameController game_ctrl;
    // Use this for initialization
    void Awake()
    {
        game_ctrl = GameObject.Find("GameController").GetComponent<GameController>();
    }

    void OnEnable()
    {
        init_rotate = transform.localRotation.z;
        StartCoroutine(UpdateEarthSelfRotate3d());
    }

    IEnumerator UpdateEarthSelfRotate3d()
    {
        for (; ; )
        {
            DateTime t = game_ctrl.getTime();

            Eci x_unit = new Eci(new Vector(0, 100000, 0));
            Geo geo = new Geo(x_unit, new Julian(t));
            Debug.Log("EarthMove3D Update Eci y_unit Longitude " +geo.LongitudeDeg);
            transform.localEulerAngles = new Vector3(0, 0, (float)geo.LongitudeDeg + init_rotate);
            yield return new WaitForSeconds(5.3f);
        }
    }
}
