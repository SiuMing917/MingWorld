using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public enum ItemCategory { Items, Pokeballs, Tms, Importants };

public class Inventory : MonoBehaviour, ISavable
{
    [SerializeField] List<ItemSlot> slots;
    [SerializeField] List<ItemSlot> pokeballSlots;
    [SerializeField] List<ItemSlot> tmSlots;
    [SerializeField] List<ItemSlot> importantSlots;

    List<List<ItemSlot>> allSlots;

    public event Action onUpdated;


    private void Awake()
    {
        allSlots = new List<List<ItemSlot>> { slots, pokeballSlots, tmSlots, importantSlots };
    }

    public static List<string> ItemCategories { get; set; } = new List<string>()
    {
        "物品","精靈球","技能學習","貴重物品"
    };
    /// <summary>
    /// 通過下標返回對應庫存類型對象清單
    /// </summary>
    /// <param name="categoryIndex"></param>
    /// <returns></returns>
    public List<ItemSlot> GetSlotsByCategory(int categoryIndex)
    {
        return allSlots[categoryIndex];
    }
    /// <summary>
    /// 獲得對應類型的對應下標的物品對象
    /// </summary>
    /// <param name="itemIndex">選中的對應下標</param>
    /// <param name="categoryIndex">對應的類型下標</param>
    /// <returns></returns>
    public ItemBase GetItem(int itemIndex, int categoryIndex)
    {
        var currenSlots = GetSlotsByCategory(categoryIndex);
        return currenSlots[itemIndex].Item;
    }

    /// <summary>
    /// 使用物品 
    /// </summary>
    /// <param name="itemIndex">物品對象下標</param>
    /// <param name="selectedPokemon">寶可夢作用物件</param>

    public ItemBase UseItem(int itemIndex, Pokemon selectedPokemon, int selectedCategory)
    {
        var item = GetItem(itemIndex, selectedCategory);
        return UseItem(item, selectedPokemon);
    }

    public ItemBase UseItem(ItemBase item, Pokemon selectedPokemon)
    {
        bool itemUsed = item.Use(selectedPokemon);
        if (itemUsed)
        {
            //物品不是可重複使用時 才刪除
            if (!item.IsReusable)
                RemoveItem(item);
            return item;
        }
        return null;
    }

    /// <summary>
    ///添加對應物品
    /// </summary>
    /// <param name="item"></param>
    /// <param name="count"></param>
    public void AddItem(ItemBase item, int count = 1)
    {
        int category = (int)GetCategoryFormItem(item);
        var currenSlots = GetSlotsByCategory(category);
        //從集合中找到對應 於item 匹配的項  沒有返回空
        var itemSlot = currenSlots.FirstOrDefault(slots => slots.Item == item);
        if (itemSlot != null)
        {
            itemSlot.Count += count;
        }
        else
        {
            currenSlots.Add(new ItemSlot()
            {
                Item = item,
                Count = count
            });
        }
        onUpdated?.Invoke();
    }

    public void RemoveItem(ItemBase item, int countToReMove = 1)
    {
        int category = (int)GetCategoryFormItem(item);
        var currenSlots = GetSlotsByCategory(category);
        //從集合中找到對應 於item 匹配的項  沒有返回Null
        var itemSlot = currenSlots.FirstOrDefault(slots => slots.Item == item);
        //它的作用是從一個名為 slots 的集合中找到第一個具有特定 item 的 slot
        itemSlot.Count -= countToReMove;
        if (itemSlot.Count == 0)
            currenSlots.Remove(itemSlot);

        onUpdated?.Invoke();
    }
    /// <summary>
    /// 獲得物品的數量
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public int GetItemCount(ItemBase item)
    {
        int category = (int)GetCategoryFormItem(item);
        var currenSlots = GetSlotsByCategory(category);
        //從集合中找到對應 於item 匹配的項  沒有返回空
        var itemSlot = currenSlots.FirstOrDefault(slots => slots.Item == item);
        //它的作用是從一個名為 slots 的集合中找到第一個具有特定 item 的 slot

        if (item != null)
            return itemSlot.Count;
        else
            return 0;
    }

    /// <summary>
    /// 是否存在物品
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool HasItem(ItemBase item)
    {
        int category = (int)GetCategoryFormItem(item);
        var currenSlots = GetSlotsByCategory(category);

        return currenSlots.Exists(slots => slots.Item == item);
    }

    /// <summary>
    /// 通過物品基類 返回物品對應的枚舉類型
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    ItemCategory GetCategoryFormItem(ItemBase item)
    {
        if (item is RecoverItem || item is EvolutionItem)
            return ItemCategory.Items;
        else if (item is PokeballItem)
            return ItemCategory.Pokeballs;
        else if (item is TmItem)
            return ItemCategory.Tms;
        else
            return ItemCategory.Importants;
    }
    /// <summary>
    /// 找到PlayerController的Inventory組件
    /// </summary>
    /// <returns></returns>
    public static Inventory GetInventory()
    {
        return FindObjectOfType<PlayerController>().GetComponent<Inventory>();
    }

    public object CaptureState()
    {
        var saveData = new InventorySaveData()
        {
            //獲得每一個類型的每一個物件的資料
            items = slots.Select(i => i.GetSaveData()).ToList(),
            pokeballs = pokeballSlots.Select(i => i.GetSaveData()).ToList(),
            tms = tmSlots.Select(i => i.GetSaveData()).ToList(),
            importants = importantSlots.Select(i => i.GetSaveData()).ToList()
        };
        return saveData;
    }

    public void RestoreState(object state)
    {
        var saveData = state as InventorySaveData;
        //通過構造函數為每個物品項的每個物品 賦值資料
        slots = saveData.items.Select(i => new ItemSlot(i)).ToList();
        pokeballSlots = saveData.pokeballs.Select(i => new ItemSlot(i)).ToList();
        tmSlots = saveData.tms.Select(i => new ItemSlot(i)).ToList();
        importantSlots = saveData.importants.Select(i => new ItemSlot(i)).ToList();

        allSlots = new List<List<ItemSlot>> { slots, pokeballSlots, tmSlots, importantSlots };
        //Update個頁面
        onUpdated?.Invoke();
    }

}

[Serializable]
public class ItemSlot
{
    [SerializeField] ItemBase item;
    [SerializeField] int count;

    public ItemSlot()
    {

    }
    /// <summary>
    /// 通過構造函數恢復資料
    /// </summary>
    /// <param name="saveData"></param>
    public ItemSlot(ItemSaveData saveData)
    {
        item = ItemDB.GetObjectByName(saveData.name);
        count = saveData.count;
    }

    /// <summary>
    /// 獲得要保存的資料
    /// </summary>
    /// <returns></returns>
    public ItemSaveData GetSaveData()
    {
        var saveData = new ItemSaveData()
        {
            name = item.name,
            count = count
        };
        return saveData;
    }

    public ItemBase Item
    {
        get => item;
        set => item = value;
    }

    public int Count
    {
        get => count;
        set => count = value;
    }
}

/// <summary>
/// 保存 資料的名字和數量
/// </summary>
[Serializable]
public class ItemSaveData
{
    public string name;
    public int count;
}

/// <summary>
/// 保存不同物品類型資料
/// </summary>
[Serializable]
public class InventorySaveData
{
    public List<ItemSaveData> items;
    public List<ItemSaveData> pokeballs;
    public List<ItemSaveData> tms;
    public List<ItemSaveData> importants;
}
