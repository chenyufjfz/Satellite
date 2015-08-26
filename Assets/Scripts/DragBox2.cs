using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class DragBox2 : DragBoxAbstract, IPointerDownHandler
{
    protected Text text;
    protected string[] l;
    protected bool[] selected;
    public Button Remove, Clear;
    protected int prev_keyclick, first_line_offset, len_num;
    public int MaxLine = 50, MoreLine=3;

    // Use this for initialization
    void Awake()
    {
        text = GetComponent<Text>();
        Remove.onClick.AddListener(OnRemoveButton);
        Clear.onClick.AddListener(OnClearButton);
        Remove.interactable = false;
        Clear.interactable = false;
        prev_keyclick = -1;
        first_line_offset = 0;
        l = new string[0];
    }

    protected void UpdateText()
    {
        bool has_selected=false;
        string t;
        if (first_line_offset!=0) {
            t="...\n";
            len_num = 1;
        }
        else
        {
            t = "\n";
            len_num = 1;
        }
        int end_line = (first_line_offset + MaxLine < l.Length) ? first_line_offset + MaxLine : l.Length;
        for (int i = first_line_offset; i < end_line; i++)
        {
            len_num++;
            if (selected[i]) {
                t += "<color=#ffffffd0><b>" + l[i] + "</b></color>";
                has_selected = true;
            }                
            else
                t += l[i];
            if (i + 1 != end_line)
                t += "\n";
        }
        if (first_line_offset + MaxLine < l.Length)
            t += "\n...";
        else
            t += "\n";
        
        len_num++;
        text.text = t;
        Remove.interactable = has_selected;
        Remove.GetComponentInChildren<Text>().color = has_selected ? Color.white : Color.gray;
        Clear.interactable = has_selected;
        Clear.GetComponentInChildren<Text>().color = has_selected ? Color.white : Color.gray;
    }

    public override void SetText(string[] lines)
    {
        int i;

        first_line_offset = 0;
        l = lines;
        selected = new bool[l.Length];
        for (i = 0; i < l.Length; i++)
            selected[i] = false;
        
        UpdateText();
    }

    public void OnPointerDown(PointerEventData data)
    {
        RectTransform pos = this.transform as RectTransform;
        Vector2 pointerOffset;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(pos, data.position, data.pressEventCamera, out pointerOffset);
        float scale = RectTransformUtility.CalculateRelativeRectTransformBounds(pos).extents.y * 2 / ((len_num == 0) ? 1 : len_num);
        int line_click = (int)Mathf.Floor((-pointerOffset.y) / scale);
        if (line_click == 0)  
            first_line_offset = (first_line_offset > MoreLine) ? first_line_offset - MoreLine : 0;        
        else         
            if (line_click == len_num - 1) {
                if (first_line_offset + MaxLine < l.Length)
                first_line_offset = (first_line_offset + MaxLine + MoreLine < l.Length) ?
                    first_line_offset + MoreLine : l.Length - MaxLine;
            }                
            else 
            {
                int key_click = line_click + first_line_offset-1;
                if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) && prev_keyclick >= 0))
                {
                    int dx = (prev_keyclick < key_click) ? 1 : -1;
                    for (int i = prev_keyclick; (i - key_click) * dx <= 0; i += dx)
                        selected[i] = true;
                }
                else
                    selected[key_click] = !selected[key_click];
                if (selected[key_click])
                    prev_keyclick = key_click;
                else
                    prev_keyclick = -1;
            }        
        
        
        UpdateText();
    }

    public void OnRemoveButton()
    {
        List<int> select_lines = new List<int>();
        for (int i = 0; i < selected.Length; i++)
            if (selected[i])
                select_lines.Add(i);
        notify_game_ctrl(select_lines.ToArray());
    }

    public void OnClearButton()
    {
        for (int i = 0; i < selected.Length; i++)
            selected[i] = false;
        UpdateText();
    }
}
