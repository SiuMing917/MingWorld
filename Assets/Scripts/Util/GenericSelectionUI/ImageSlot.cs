using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageSlot : MonoBehaviour, ISelectableItem
{
    [SerializeField] Image bgImage;
    [SerializeField] Sprite changeSprite;

    Color originalColor;
    Sprite originalSprite;
    Image originalImage;

    private void Awake()
    {
        bgImage = GetComponent<Image>();
    }

    public void Init()
    {
        originalColor = bgImage.color;
        originalSprite = bgImage.sprite;
        originalImage = bgImage;
    }

    public void Clear()
    {
        bgImage.color = originalColor;
    }

    public void OnSelectionChanged(bool selected)
    {
        //bgImage1.color = (selected)? GlobalSettings.i.HighlightedColor : originalColor;
        bgImage.sprite = (selected) ? changeSprite : originalSprite;
    }

}
