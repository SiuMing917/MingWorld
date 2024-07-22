using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{

    //子物件 物品 UI合集
    [SerializeField] GameObject itemList;
    //為 UI賦值的物品槽
    [SerializeField] ItemSlotUI itemSlotUI;

    [SerializeField] Image itemIcon;
    [SerializeField] Text itemDescription;

    [SerializeField] Image upArrow;
    [SerializeField] Image downArrow;

    List<ItemSlotUI> slotUIList;
    Action<ItemBase> onItemSelected;
    Action onBack;

    int selectedItem = 0;

    /// <summary>
    /// 添加的貨物
    /// </summary>
    [SerializeField] List<ItemBase> availableItem;


    const int itemsInViewport = 10;
    RectTransform itemListRect;

    private void Awake()
    {
        itemListRect = itemList.GetComponent<RectTransform>();
    }


    /// <summary>
    /// 顯示商店貨物清單
    /// </summary>
    /// <param name="availableItem">可購買/選擇的物品列表</param>
    /// <param name="onItemSelected">事件選中物品 參數通過事件啟用傳入</param>
    /// <param name="onBack">事件返回</param>
    public void Show(List<ItemBase> availableItem, Action<ItemBase> onItemSelected,
        Action onBack)
    {
        this.availableItem = availableItem;
        this.onItemSelected = onItemSelected;
        this.onBack = onBack;
        gameObject.SetActive(true);
        UpdateItemList();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }


    /// <summary>
    /// 更新按鍵選擇邏輯
    /// </summary>
    public void HandleUpdate()
    {
        var prveSelected = selectedItem;

        if (Input.GetKeyDown(KeyCode.DownArrow))
            ++selectedItem;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            --selectedItem;

        selectedItem = Mathf.Clamp(selectedItem, 0, availableItem.Count - 1);

        if (selectedItem != prveSelected)
            UpdateItemSelection();

        if (Input.GetKeyDown(KeyCode.Z))
            onItemSelected?.Invoke(availableItem[selectedItem]);
        else if (Input.GetKeyDown(KeyCode.X))
            onBack?.Invoke();
    }

    /// <summary>
    /// 更新商品 列表
    /// </summary>
    void UpdateItemList()
    {
        //清除所有子物件
        foreach (Transform child in itemList.transform)
        {
            Destroy(child.gameObject);
        }

        slotUIList = new List<ItemSlotUI>();
        //獲得玩家物件下的物品屬性
        foreach (var item in availableItem)
        {
            //在（子類，父類） 父類下生成子類
            var slotUIObj = Instantiate(itemSlotUI, itemList.transform);
            //為Ui賦值 設置名字和價格
            slotUIObj.SetNameAndPrice(item);
            //將生成的每一項加入到集合中  用於更新選中效果
            slotUIList.Add(slotUIObj);
        }
        //打開時更新選中的顏色
        UpdateItemSelection();
    }

    /// <summary>
    /// 更新選中效果 顏色 文本 滑動
    /// </summary>
    public void UpdateItemSelection()
    {
        selectedItem = Mathf.Clamp(selectedItem, 0, availableItem.Count - 1);
        for (int i = 0; i < slotUIList.Count; i++)
        {
            if (selectedItem == i)
                slotUIList[i].NameText.color = GlobalSettings.i.HighlightedColor;
            else
                slotUIList[i].NameText.color = Color.black;
        }

        if (availableItem.Count > 0)
        {
            var item = availableItem[selectedItem];
            itemIcon.sprite = item.Icon;
            itemDescription.text = item.Description;
        }

        HandleScrolling();
    }

    /// <summary>
    /// 滾動
    /// </summary>
    void HandleScrolling()
    {
        if (slotUIList.Count <= itemsInViewport) return;

        //根據當前選定的專案（selectedItem）和視圖中可見項目數量的一半（itemsInViewport/2），計算出要滾動的位置（scrollPos）
        float scrollPos = Mathf.Clamp(selectedItem - itemsInViewport / 2, 0, selectedItem) * slotUIList[0].Height;
        //改變物品集合 的 高度值  //切換的效果
        itemListRect.localPosition = new Vector3(itemListRect.localPosition.x, scrollPos);

        //顯示箭頭
        bool showUpArrow = selectedItem > itemsInViewport / 2;
        upArrow.gameObject.SetActive(showUpArrow);

        bool showDownArrow = selectedItem + itemsInViewport / 2 < slotUIList.Count;
        downArrow.gameObject.SetActive(showDownArrow);
    }

}
