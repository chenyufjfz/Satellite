using UnityEngine;
using System.Collections;
using Zeptomoby.OrbitTools;
using System.Collections.Generic;
using System;
using System.IO;

public class SunSatLight2D : MonoBehaviour {

	// Use this for initialization
    public byte bkalpha = 200;
    protected Texture2D tex;
    protected Color32[] color;
    protected List <LightInfo>  light_set;
    protected bool need_update;
    protected bool update_ready;
    // Use this for initialization
    void Awake()
    {
        tex = new Texture2D(CoordChange2D.TexWidth, CoordChange2D.TexHeight, TextureFormat.ARGB32, false);
        color = new Color32[tex.height * tex.width];
        renderer.material.mainTexture = tex;
        light_set = new List<LightInfo> ();
        need_update = false;
        update_ready = false;
    }

    void OnEnable()
    {
        StartCoroutine(FillColor());
    }

    void OnDisable()
    {
        light_set.Clear();
    }

    public int AddLightSource()
    {
        LightInfo l = new LightInfo();
        l.border = new List<Geo[]>();
        light_set.Add(l);
        return light_set.Count - 1;
    }

    public void DisableLightSource(int light_idx)
    {
        light_set[light_idx].border.Clear();
        need_update = true;
    }

    public void SetLight(int light_idx, LightInfo l)
    {
        light_set[light_idx] = l;
        need_update = true;
    }
    
    public void SetLight(int light_idx, Geo starfall_geo, float [] alpha, Color32 color, int resolution=256)
    {
        LightInfo l = light_set[light_idx];
        l.border.Clear();
        l.color = color;
        Vector3 star_fall = new Vector3(Mathf.Cos((float)starfall_geo.LatitudeRad) * Mathf.Cos((float)starfall_geo.LongitudeRad),
                Mathf.Cos((float)starfall_geo.LatitudeRad) * Mathf.Sin((float)starfall_geo.LongitudeRad),
                Mathf.Sin((float)starfall_geo.LatitudeRad));

        for (int b = 0; b < alpha.Length; b++)
        {
            Geo[] ret = new Geo[resolution + 1];
            float b0_lat = (float)starfall_geo.LatitudeRad + alpha[b];
            Vector3 position = new Vector3(Mathf.Cos(b0_lat) * Mathf.Cos((float)starfall_geo.LongitudeRad),
                    Mathf.Cos(b0_lat) * Mathf.Sin((float)starfall_geo.LongitudeRad), Mathf.Sin(b0_lat));

            Quaternion rotate = new Quaternion(star_fall.x * Mathf.Sin(Mathf.PI / resolution), star_fall.y * Mathf.Sin(Mathf.PI / resolution),
                star_fall.z * Mathf.Sin(Mathf.PI / resolution), Mathf.Cos(Mathf.PI / resolution));

            for (int i = 0; i < resolution; i++)
            {
                if (Mathf.Abs(position.z)<1)
                    ret[i] = new Geo(Math.Asin(position.z), Math.Atan2(position.y, position.x), 1); 
                else
                {
                    if (position.z > 0)
                        ret[i] = new Geo(Mathf.PI / 2-0.0001, Math.Atan2(position.y, position.x), 1);
                    else
                        ret[i] = new Geo(-Mathf.PI / 2+0.0001, Math.Atan2(position.y, position.x), 1);
                }
                    
                position = rotate * position;
            }
            ret[resolution] = ret[0];
            l.border.Add(ret);
        }
        need_update = true;        
    }
    void ComputeBorder(Geo [] geo, ref Vector2[] border)
    {
        for (int i = 0; i < border.Length; i++)
        {
            border[i].x = -128;
            border[i].y = -128;
        }
        for (int i = 0; i < geo.Length - 1; i++)
        {
            float x, y;
            Vector2 from, to, from_next, to_next;
            int j;

            from = CoordChange2D.Geo2Texuv(geo[i]);
            to = CoordChange2D.Geo2Texuv(geo[i + 1]);
            if (to.x == (int)to.x)
            {
                geo[i + 1].LongitudeRad = (to.x > tex.width / 2) ? geo[i + 1].LongitudeRad - 0.0001f : geo[i + 1].LongitudeRad + 0.0001f;
                to = CoordChange2D.Geo2Texuv(geo[i + 1]);
            }
            from_next = from;
            to_next = to;
            if (Mathf.Abs(from.x - to.x) > tex.width / 2)
            {
                float x0, x1;
                x0 = (from.x < to.x) ? from.x + tex.width : from.x;
                x1 = (to.x < from.x) ? to.x + tex.width : to.x;
                y = Mathf.Lerp(from.y, to.y, Mathf.InverseLerp(x0, x1, tex.width));
                if (from.x < to_next.x)
                {
                    from_next.x = tex.width - 0.9999f;
                    to.x = -0.0001f;
                }
                else
                {
                    from_next.x = -0.0001f;
                    to.x = tex.width - 0.9999f;
                }
                from_next.y = y;
                to.y = y;
                j = 2;
            }
            else
                j = 1;
            for (; j > 0; j--)
            {
                if (Mathf.Abs(from.x - to.x) > tex.width / 3)
                    throw new ArgumentOutOfRangeException("ComputeBorder wrong x1:" + from + "x2:" + to);
                if (from.x < to.x)
                {
                    x = Mathf.Ceil(from.x);
                    while (x < to.x)
                    {
                        y = Mathf.Lerp(from.y, to.y, Mathf.InverseLerp(from.x, to.x, x));
                        if (y < 0 || y > tex.height - 1 || from.x > x)
                            throw new ArgumentOutOfRangeException("ComputeBorder wrong from:" + from + "to:" + to);
                        if (border[(int)x].x >= 0)
                            throw new ArgumentOutOfRangeException("3 points is found in one longitude:" + (int)x +
                                ",y0=" + y + ",y1=" + border[(int)x].x + ",y2=" + border[(int)x].y);
                        border[(int)x].x = y;                        
                        if (border[(int)x].x > border[(int)x].y)
                        {
                            border[(int)x].x = border[(int)x].y;
                            border[(int)x].y = y;
                        }
                        x = x + 1.0f;
                    }
                }
                else
                {
                    x = Mathf.Floor(from.x);
                    while (x > to.x)
                    {
                        y = Mathf.Lerp(from.y, to.y, Mathf.InverseLerp(from.x, to.x, x));
                        if (y < 0 || y > tex.height - 1 || from.x < x)
                            throw new ArgumentOutOfRangeException("ComputeBorder wrong from:" + from + "to:" + to);
                        if (border[(int)x].x >= 0)
                            throw new ArgumentOutOfRangeException("3 points is found in one longitude:" + (int)x +
                                ",y0=" + y + ",y1=" + border[(int)x].x + ",y2=" + border[(int)x].y);
                        border[(int)x].x = y;                        
                        if (border[(int)x].x > border[(int)x].y)
                        {
                            border[(int)x].x = border[(int)x].y;
                            border[(int)x].y = y;
                        }
                        x = x - 1.0f;
                    }
                }
                from = from_next;
                to = to_next;
            }
        }
    }

    void AddColor(ref Color32 c1, Color32 c2)
    {
        if ((int)c1.a + (int)c2.a > 255)
            c1.a = 255;
        else
            c1.a += c2.a;
        if ((int)c1.b + (int)c2.b > 255)
            c1.b = 255;
        else
            c1.b += c2.b;
        if ((int)c1.g + (int)c2.g > 255)
            c1.g = 255;
        else
            c1.g += c2.g;
        if ((int)c1.r + (int)c2.r > 255)
            c1.r = 255;
        else
            c1.r += c2.r;
    }

    int check_dir(List<Vector2[]> border, int x)
    {
        if (border[0][x].y < 0)
            return 0;

        float y0 = border[0][x].y, dy0 = 1, y0_n;
        float y1 = border[0][x].y, dy1 = -1, y1_n;
        int x0 = x, x1 = x;

        int go = 0;
        float b0 = border[0][x0].x, b1 = border[0][x1].x;
        while (true)
        {
            y0_n = y0 + dy0;
            if ((y0_n - b0) * (y0 - b0) <= 0 ||
                (y0_n - border[1][x0].x) * (y0 - border[1][x0].x) <= 0 ||
                (y0_n - border[1][x0].y) * (y0 - border[1][x0].y) <= 0)
                return 1;
            if (y0_n < 0)
            {                
                x0 = (x0 < tex.width / 2) ? x0 + tex.width / 2 : x0 - tex.width / 2;                
                y0 = -y0;
                dy0 = -dy0;
                b0 = border[0][x0].y;
            }
            else
                if (y0_n > tex.height - 1)
                {                   
                    x0 = (x0 < tex.width / 2) ? x0 + tex.width / 2 : x0 - tex.width / 2;
                    y0 = 2 * (tex.height - 1) - y0;
                    dy0 = -dy0;
                    b0 = border[0][x0].y;
                }
                else
                    y0 = y0_n;

            y1_n = y1 + dy1;
            if ((y1_n - b1) * (y1 - b1) <= 0 ||
                (y1_n - border[1][x1].x) * (y1 - border[1][x1].x) <= 0 ||
                (y1_n - border[1][x1].y) * (y1 - border[1][x1].y) <= 0)
                return -1;
            if (y1_n < 0)
            {               
                x1 = (x1 < tex.width / 2) ? x1 + tex.width / 2 : x1 - tex.width / 2;
                y1 = -y1;
                dy1 = -dy1;
                b1 = border[0][x1].y;
            }
            else
                if (y1_n > tex.height - 1)
                {                    
                    x1 = (x1 < tex.width / 2) ? x1 + tex.width / 2 : x1 - tex.width / 2;
                    y1 = 2 * (tex.height - 1) - y1;
                    dy1 = -dy1;
                    b1 = border[0][x1].y;
                }
                else
                    y1 = y1_n;
            if (go++ > tex.height)
                throw new ArgumentOutOfRangeException("border have only one point at longitue" + x);
        }
    }

    void self_test(List<Vector2[]> border, LightInfo light)
    {
        for (int j = 0; j < border.Count; j++)
        {
            
            for (int x = 0; x < tex.width / 2; x++)
            {
                int c = 0;
                bool pass =true;
                if (border[j][x].x > tex.height - 1 || border[j][x].y > tex.height - 1 ||
                    border[j][x + tex.width / 2].x > tex.height - 1 || border[j][x + tex.width / 2].y > tex.height - 1)
                    pass = false;
                if (float.IsNaN(border[j][x].x) || float.IsNaN(border[j][x].y) || float.IsNaN( border[j][x + tex.width / 2].x)
                    || float.IsNaN( border[j][x + tex.width / 2].y))
                    pass = false;
                if (border[j][x].x >= 0)
                    c++;
                if (border[j][x].y >= 0)
                    c++;
                if (border[j][x + tex.width / 2].x >= 0)
                    c++;
                if (border[j][x + tex.width / 2].y >= 0)
                    c++;
                if (c != 2 && c != 0)
                    pass = false;
                if (!pass)
                {
                    FileStream fs = new FileStream("bug.txt", FileMode.Create);
                    StreamWriter sw = new StreamWriter(fs);
                    sw.Write("border " + j + " wrong at " + x + "c=" + c + "\n");
                    for (int i = 0; i < tex.width; i++)
                        sw.Write(i + ":" + border[j][i].x + "," + border[j][i].y+"\n");
                    sw.Write("raw data");
                    for (int i = 0; i < light.border[j].Length; i++)
                        sw.Write(i + ":" + light.border[j][i]+", uv=" + CoordChange2D.Geo2Texuv(light.border[j][i]) +"\n");
                    sw.Close();
                    fs.Close();
                    Debug.DebugBreak();
                    throw new ArgumentOutOfRangeException("border wrong");
                }
            }

        }
    }

    IEnumerator FillColor()
    {
        for (; ; )
        {
            update_ready = false;
            Color32 black = new Color32(0, 0, 0, 0);
            for (int i = 0; i < tex.height * tex.width; i++)
                color[i] = new Color32(0, 0, 0, bkalpha);
            while (!need_update)
                yield return new WaitForEndOfFrame();
            need_update = false;
            for (int i = 0; i < light_set.Count; i++)
            {
                if (light_set[i].border==null || light_set[i].border.Count == 0)
                    continue;
                //Debug.Log("draw light:" + i);
                int x, y, dy, step, y_n;
                Color32[] c = new Color32[light_set[i].border.Count * 2];
                List<Vector2[]> border = new List<Vector2[]>();
                int[] cs = new int[light_set[i].border.Count];
                for (int j = 0; j < light_set[i].border.Count; j++)
                {
                    Vector2[] b = new Vector2[tex.width];
                    ComputeBorder(light_set[i].border[j], ref b);
                    border.Add(b);
                    c[2 * j] = Color32.Lerp(black, light_set[i].color, Mathf.InverseLerp(0, light_set[i].border.Count * 2 - 1, 2 * j));
                    c[2 * j + 1] = Color32.Lerp(black, light_set[i].color, Mathf.InverseLerp(0, light_set[i].border.Count * 2 - 1, 2 * j + 1));                    
                }

                for (x = 0; x < tex.width / 2; x++)
                {

                    int x0 = (border[0][x].y < 0) ? x + tex.width / 2 : x;
                    for (int j = 0; j < light_set[i].border.Count; j++)
                        cs[j] = 0;

                    self_test(border, light_set[i]);
                    dy = check_dir(border, x0);
                    
                    if (dy == 0)
                        continue;
                    if (dy > 0)
                    {
                        y = (int)Mathf.Floor(border[0][x0].y);
                        if (border[0][x0].y - y == 0f)
                            cs[0] = 1;
                    }
                    else
                        y = (int)Mathf.Ceil(border[0][x0].y);
                    step = 0;
                    AddColor(ref color[y * tex.width + x0], c[step]);                    

                    int go = 0;
                    do
                    {
                        y_n = y + dy;                        
                        bool hit = false;
                        for (int j = 0; j < cs.Length; j++)
                        {
                            if (Mathf.Sign(y_n - border[j][x0].x) != Mathf.Sign(y - border[j][x0].x))
                            {
                                hit = true;
                                cs[j]++;
                            }
                            if (Mathf.Sign(y_n - border[j][x0].y) != Mathf.Sign(y - border[j][x0].y))
                            {
                                hit = true;
                                cs[j]++;
                            }
                        }
                        if (y_n < 0 || y_n > tex.height - 1)
                        {
                            x0 = (x0 < tex.width / 2) ? x0 + tex.width / 2 : x0 - tex.width / 2;
                            dy = -dy;
                        }
                        y = y_n;
                        for (step = 0; step < cs.Length && cs[step] == 1; step++) ;
                        if (step == cs.Length || cs[step] == 0)
                            step = hit ? step * 2 - 2 : step * 2 - 1;
                        else
                            step = hit ? step * 2 : step * 2 - 1;

                        if (step<0 || step>c.Length || go++ >tex.height*2)
                            throw new ArgumentOutOfRangeException("i="+i+",step="+step+",cs0="+cs[0]+",cs1="+cs[1]+"x:"+x+"y:"+y+
                                ",b0=" + border[0][x].x + ",b1=" + border[0][x].y+",b2="+border[0][x+tex.width/2].x+",b3="+border[0][x+tex.width/2].y);

                        if (y >= 0 && y <= tex.height - 1)
                            AddColor(ref color[y * tex.width + x0], c[step]);
                    } while (cs[0] !=2);

                }
                yield return new WaitForEndOfFrame();
            }
            for (int i = 0; i < tex.height * tex.width; i++)
            {
                int red = (int)color[i].r * (int)color[i].a / 128;
                int green = (int)color[i].g * (int)color[i].a / 128;
                int blue = (int)color[i].b * (int)color[i].a / 128;
                color[i].r = (byte)(red > 255 ? 255 : red);
                color[i].g = (byte)(green > 255 ? 255 : green);
                color[i].b = (byte)(blue > 255 ? 255 : blue);
                int da = ((int)color[i].r + (int)color[i].g + (int)color[i].b) / 10;
                color[i].a = (byte)(255 - color[i].a + da);
            }

            update_ready = true;
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (update_ready)
        {
            tex.SetPixels32(color);
            tex.filterMode = FilterMode.Trilinear;
            tex.Apply();
            update_ready = false;
        }
    }
}

public class LightInfo
{
    public List <Geo[]> border;
    public Color32 color;
}