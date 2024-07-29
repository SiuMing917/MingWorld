using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalSettings : MonoBehaviour
{
    [SerializeField] Color highlightedColor;
    [SerializeField] Color inhighlightedColor;
    [SerializeField] Color bgHighlightedColor;
    public Color HighlightedColor => highlightedColor;
    public Color InHighlightedColor => inhighlightedColor;

    public Color BgHighlightedColor => bgHighlightedColor;

    public static GlobalSettings i { get; private set; }

    private void Awake()
    {
        i = this;
    }
}
