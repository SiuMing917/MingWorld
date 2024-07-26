using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextSlot : MonoBehaviour, ISelectableItem
{
    [SerializeField] Text text;

    Color originalColor;
    int originalSize;
    FontStyle originalStyle;
    public Color customColor;

    public void Init()
    {
        originalColor = text.color;
        originalStyle = text.fontStyle;
        originalSize = text.fontSize;
    }

    public void Clear()
    {
        text.color = originalColor;
    }

    public void OnSelectionChanged(bool selected)
    {
        text.color = (selected) ? GlobalSettings.i.HighlightedColor : originalColor;
    }

    public void SetText(string s)
    {
        text.text = s;
    }

    public void OnSelectionCustomChanged(bool selected, Color customColor, FontStyle fontStyle, int fontSize)
    {
        text.color = (selected) ? customColor : originalColor;
        text.fontStyle = (selected) ? fontStyle : originalStyle;
        text.fontSize = (selected) ? fontSize : originalSize;
    }
}
