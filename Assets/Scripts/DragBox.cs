using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragBoxAbstract : MonoBehaviour
{
    public delegate void NotifyGameCtrl(int[] line_click);
    public NotifyGameCtrl notify_game_ctrl;
    public virtual void SetText(string [] lines)
    {

    }
}

public class DragBox : DragBoxAbstract, IPointerDownHandler
{    
    protected Text text;
    protected int len_num;
    public int MaxLine = 50;
	// Use this for initialization
	void Awake () {
        text = GetComponent<Text>();
        len_num = 0;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public override void SetText(string [] lines)
    {
        string t = "";
        int len = (lines.Length < MaxLine) ? lines.Length : MaxLine;
        for (int i = 0; i < len; i++)
            if (i + 1 == len)
                t += lines[i];
            else
                t += lines[i] + "\n";
        if (lines.Length > MaxLine)
            t += "  ...";
        len_num = len;
        text.text = t;
    }

    public void OnPointerDown(PointerEventData data)
    {
        RectTransform pos = this.transform as RectTransform;
        Vector2 pointerOffset;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(pos, data.position, data.pressEventCamera, out pointerOffset);

        int[] line_click = new int[1];
        float scale = RectTransformUtility.CalculateRelativeRectTransformBounds(pos).extents.y * 2 / ((len_num==0)? 1: len_num);
        line_click[0] = (int)Mathf.Floor((-pointerOffset.y) / scale);        
        if (line_click[0]<len_num)
            notify_game_ctrl(line_click);
    }
}
