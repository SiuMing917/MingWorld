using GDE.GenericSelectionUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : SelectionUI<TextSlot>
{
    //物品  Item List UI合集
    [SerializeField] GameObject itemList;
    //為 UI賦值的物品槽item slot
    [SerializeField] ItemSlotUI itemSlotUI;

    //類型描述
    [SerializeField] Text categoryText;
    [SerializeField] Image itemIcon;
    [SerializeField] Text itemDescription;

    [SerializeField] Image upArrow;
    [SerializeField] Image downArrow;

    int selectedCategory = 0;

    const int itemsInViewport = 8;

    List<ItemSlotUI> slotUIList;
    Inventory inventory;
    RectTransform itemListRect;

    private void Awake()
    {
        //獲得玩家對象下的  物品/庫存物件元件
        inventory = Inventory.GetInventory();
        itemListRect = itemList.GetComponent<RectTransform>();
    }

    private void Start()
    {

        UpdateItemList();
        inventory.onUpdated += UpdateItemList;
    }

    /// <summary>
    /// 更新物品UI DATA
    /// </summary>
    void UpdateItemList()
    {
        //清除所有child Object
        foreach (Transform child in itemList.transform)
        {
            Destroy(child.gameObject);
        }

        slotUIList = new List<ItemSlotUI>();
        //獲得玩家物件下的物品屬性
        foreach (var itemSlot in inventory.GetSlotsByCategory(selectedCategory))
        {

            //在（Child Class，Parent Class）Parent Class下生成Child Class
            var slotUIObj = Instantiate(itemSlotUI, itemList.transform);
            //為UI賦值
            slotUIObj.SetData(itemSlot);

            //將生成的每一項加入到集合中  用於更新選中效果
            slotUIList.Add(slotUIObj);
        }
        //打開時更新選中的顏色
        //UpdateItemSelection();

        SetItems(slotUIList.Select(s => s.GetComponent<TextSlot>()).ToList());

        UpdateSelectionInUI();
    }

    public override void HandleUpdate()
    {
        int prevCategory = selectedCategory;

        if (Input.GetKeyDown(KeyCode.RightArrow))
            ++selectedCategory;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            --selectedCategory;

        //循環 切換
        if (selectedCategory > Inventory.ItemCategories.Count - 1)
            selectedCategory = 0;
        else if (selectedCategory < 0)
            selectedCategory = Inventory.ItemCategories.Count - 1;

        //下標變動時 更新清單/切換頁面
        if (prevCategory != selectedCategory)
        {
            ResetSelection();
            categoryText.text = Inventory.ItemCategories[selectedCategory];
            UpdateItemList();
        }

        base.HandleUpdate();
    }


    public override void UpdateSelectionInUI()
    {
        base.UpdateSelectionInUI();

        var slots = inventory.GetSlotsByCategory(selectedCategory);

        if (slots.Count > 0)
        {
            var item = slots[selectedItem].Item;
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


        bool showUpArrow = selectedItem > itemsInViewport / 2;
        upArrow.gameObject.SetActive(showUpArrow);

        bool showDownArrow = selectedItem + itemsInViewport / 2 < slotUIList.Count;
        downArrow.gameObject.SetActive(showDownArrow);
    }

    /// <summary>
    /// 切换頁面時重置
    /// </summary>
    void ResetSelection()
    {
        selectedItem = 0;

        upArrow.gameObject.SetActive(false);
        downArrow.gameObject.SetActive(false);

        itemIcon.sprite = null;
        itemDescription.text = "";
    }

    public ItemBase SelectedItem => inventory.GetItem(selectedItem, selectedCategory);

    public int SelectedCategory => selectedCategory;
}
