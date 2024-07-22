using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlotUI : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text countText;

    RectTransform rectTransform;

    private void Awake()
    {
    }

    public Text NameText => nameText;

    public Text CountText => countText;

    public float Height => rectTransform.rect.height;

    /// <summary>
    /// 物品槽的值付給 UI
    /// </summary>
    /// <param name="itemSlot"></param>
    public void SetData(ItemSlot itemSlot)
    {
        nameText.text = itemSlot.Item.Name;
        countText.text = $"X {itemSlot.Count}";
        rectTransform = GetComponent<RectTransform>();
    }
    /// <summary>
    /// SET名字和價格
    /// </summary>
    /// <param name="item"></param>
    public void SetNameAndPrice(ItemBase item)
    {
        rectTransform = GetComponent<RectTransform>();
        nameText.text = item.Name;
        countText.text = $"${item.Price}";
    }
}
