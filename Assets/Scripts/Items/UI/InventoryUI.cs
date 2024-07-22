using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum InventoryUIState { ItemSelection, PartySelection, MoveToForget, Busy }

public class InventoryUI : MonoBehaviour
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

    [SerializeField] PartyScreen partyScreen;
    [SerializeField] MoveSelectonUI moveSelectonUI;

    Action<ItemBase> onItemUsed;

    int selectedItem = 0;
    int selectedCategory = 0;

    MoveBase moveToLearn;

    const int itemsInViewport = 8;

    List<ItemSlotUI> slotUIList;
    Inventory inventory;
    InventoryUIState state;
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
        UpdateItemSelection();
    }


    /// <summary>
    /// 背包/庫存  更新邏輯方法  
    /// </summary>
    /// <param name="onBack">返回</param>
    /// <param name="onItemUsed">選中物品執行的邏輯 扔球、出售 使用</param>
    public void HandleUpdate(Action onBack, Action<ItemBase> onItemUsed = null)
    {
        this.onItemUsed = onItemUsed;

        if (state == InventoryUIState.ItemSelection)
        {
            int prevSelecton = selectedItem;
            int prevCategory = selectedCategory;

            if (Input.GetKeyDown(KeyCode.DownArrow))
                selectedItem++;
            else if (Input.GetKeyDown(KeyCode.UpArrow))
                selectedItem--;
            else if (Input.GetKeyDown(KeyCode.RightArrow))
                selectedCategory++;
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
                selectedCategory--;

            //循環 切換
            if (selectedCategory > Inventory.ItemCategories.Count - 1)
                selectedCategory = 0;
            else if (selectedCategory < 0)
                selectedCategory = Inventory.ItemCategories.Count - 1;

            //selectedCategory = Mathf.Clamp(selectedCategory, 0, Inventory.ItemCategories.Count - 1);
            selectedItem = Mathf.Clamp(selectedItem, 0, inventory.GetSlotsByCategory(selectedCategory).Count - 1);

            //下標變動時 更新清單/切換頁面
            if (prevCategory != selectedCategory)
            {
                ResetSelection();
                categoryText.text = Inventory.ItemCategories[selectedCategory];
                UpdateItemList();
            }

            else if (prevSelecton != selectedItem)
            {
                UpdateItemSelection();
            }

            if (Input.GetKeyDown(KeyCode.Z))
            {
                //打開隊伍
                // OpenPartyScreen();
                //使用物品
                StartCoroutine(ItemSelected());
            }
            else if (Input.GetKeyDown(KeyCode.X))
                onBack?.Invoke();
        }
        //當畫面切換到Pokemon隊伍介面時
        else if (state == InventoryUIState.PartySelection)
        {
            Action onSelection = () =>
            {
                //使用物品 協程
                StartCoroutine(UseItem());
            };
            Action onBackPartyScreen = () =>
            {
                ClosePartyScreen();
            };

            //調用PartyScreen 的選中方法
            partyScreen.HandleUpdate(onSelection, onBackPartyScreen);
        }
        else if (state == InventoryUIState.MoveToForget)
        {
            Action<int> onMoveSelected = (int moveIndex) =>
            {
                StartCoroutine(OnMoveToForgetSelected(moveIndex));
            };

            moveSelectonUI.HandleMoveSelection(onMoveSelected);
        }
    }
    /// <summary>
    /// 區分 選中類型 執行 
    /// </summary>
    IEnumerator ItemSelected()
    {
        //確保協程運行時不會有其他事情發生
        state = InventoryUIState.Busy;

        var item = inventory.GetItem(selectedItem, selectedCategory);
        //在 商店中的行為
        if (GameControlller.Instance.State == GameState.Shop)
        {
            onItemUsed?.Invoke(item);
            state = InventoryUIState.ItemSelection;
            yield break;
        }
        //戰鬥狀態
        if (GameControlller.Instance.State == GameState.Battle)
        {
            //在戰鬥中
            if (!item.CanUseInBattle)
            {
                yield return DialogManager.Instance.ShowDialogText($"這個物品不能在戰鬥中使用");
                state = InventoryUIState.ItemSelection;
                yield break;
            }
        }
        else
        {
            //不在戰鬥中
            if (!item.CanUseOutsideBattle)
            {
                yield return DialogManager.Instance.ShowDialogText($"這個物品現在不能使用");
                state = InventoryUIState.ItemSelection;
                yield break;
            }
        }

        //選中的是精靈球時直接使用物品
        //其他恢復物品等等打開隊伍使用

        if (selectedCategory == (int)ItemCategory.Pokeballs)
        {
            StartCoroutine(UseItem());
        }
        else
        {
            OpenPartyScreen();
            //當從技能學習列表打開 寶可夢隊伍介面是 顯示 學習技能文本
            if (item is TmItem)
                partyScreen.ShowIfTmIsUsable(item as TmItem);

        }
    }


    /// <summary>
    /// 使用物品
    /// </summary>
    /// <returns></returns>
    IEnumerator UseItem()
    {
        state = InventoryUIState.Busy;

        yield return HandleTmItems();

        var item = inventory.GetItem(selectedItem, selectedCategory);
        var pokemon = partyScreen.SelectedMember;

        //處理進化物品
        if (item is EvolutionItem)
        {
            //檢測寶可夢進化 可用的進化物品
            var evolution = pokemon.CheckForEvolution(item);
            if (evolution != null)
            {
                yield return EvolutionManger.i.Evolve(pokemon, evolution);
            }
            else
            {
                yield return DialogManager.Instance.ShowDialogText("物品沒有效果");
                ClosePartyScreen();
                yield break;
            }
        }

        var usedItem = inventory.UseItem(selectedItem, partyScreen.SelectedMember, selectedCategory);
        if (usedItem != null)
        {
            if ((usedItem is RecoverItem))
                yield return DialogManager.Instance.ShowDialogText($"玩家使用了{usedItem.Name}");
            onItemUsed?.Invoke(usedItem);
        }
        else
        {
            if (selectedCategory == (int)ItemCategory.Items)
                yield return DialogManager.Instance.ShowDialogText("物品沒有效果");
        }
        ClosePartyScreen();
    }

    /// <summary>
    /// 技能學習處理函數
    /// </summary>
    /// <returns></returns>
    IEnumerator HandleTmItems()
    {
        var tmItem = inventory.GetItem(selectedItem, selectedCategory) as TmItem;
        if (tmItem == null)
            yield break;

        var pokemon = partyScreen.SelectedMember;

        //技能擁有判定
        if (pokemon.HasMove(tmItem.Move))
        {
            yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name}已經學會了{tmItem.Move.Name}");
            yield break;
        }
        //不能學習的技能判定
        if (!tmItem.CanBeTaught(pokemon))
        {
            yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name}不能學習{tmItem.Move.Name}");
            yield break;
        }

        if (pokemon.Moves.Count < PokemonBase.MaxNumOfMoves)
        {
            pokemon.LearnMove(tmItem.Move);
            yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name}學會了{tmItem.Move.Name}");
        }
        else
        {
            yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name}嘗試學習{tmItem.Move.Name}");
            yield return DialogManager.Instance.ShowDialogText($"但是技能數量超過了{PokemonBase.MaxNumOfMoves}");
            yield return ChooseMoveToForget(pokemon, tmItem.Move);
            yield return new WaitUntil(() => state != InventoryUIState.MoveToForget);
        }

    }

    IEnumerator ChooseMoveToForget(Pokemon pokemon, MoveBase newMove)
    {
        state = InventoryUIState.Busy;
        yield return DialogManager.Instance.ShowDialogText($"請選擇要遺忘的技能", true, false);
        moveSelectonUI.gameObject.SetActive(true);
        moveSelectonUI.SetMoveDate(pokemon.Moves.Select(x => x.Base).ToList(), newMove);

        //將新技能賦值給moveTolearn 以便調用
        moveToLearn = newMove;
        state = InventoryUIState.MoveToForget;
    }

    /// <summary>
    /// 更新選中效果 顏色 文本 滑動
    /// </summary>
    public void UpdateItemSelection()
    {
        var slots = inventory.GetSlotsByCategory(selectedCategory);
        selectedItem = Mathf.Clamp(selectedItem, 0, slots.Count - 1);
        for (int i = 0; i < slotUIList.Count; i++)
        {
            if (selectedItem == i)
                slotUIList[i].NameText.color = GlobalSettings.i.HighlightedColor;
            else
                slotUIList[i].NameText.color = Color.black;
        }

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


    /// <summary>
    /// 打開隊伍畫面
    /// </summary>
    void OpenPartyScreen()
    {
        state = InventoryUIState.PartySelection;
        partyScreen.gameObject.SetActive(true);
    }

    void ClosePartyScreen()
    {
        state = InventoryUIState.ItemSelection;
        //清空顯示的文本
        partyScreen.ClaerMemberSlotMessage();
        partyScreen.gameObject.SetActive(false);
    }

    /// <summary>
    /// 協程 控制技能遺忘/替換邏輯
    /// </summary>
    /// <param name="moveIndex"></param>
    /// <returns></returns>
    IEnumerator OnMoveToForgetSelected(int moveIndex)
    {
        var pokemon = partyScreen.SelectedMember;

        DialogManager.Instance.CloseDialog();
        moveSelectonUI.gameObject.SetActive(false);
        //選擇遺忘新的技能
        if (moveIndex == PokemonBase.MaxNumOfMoves)
        {
            //不學習新技能
            yield return (DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name}沒有學習{moveToLearn.Name}"));
        }
        else
        {
            //忘記已有的技能，學習新的技能
            var selectedMove = pokemon.Moves[moveIndex];
            yield return (DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name}遺忘了{selectedMove.Base.Name}，學習了{moveToLearn.Name}"));

            //賦值替換 技能 忘記 學習
            pokemon.Moves[moveIndex] = new Move(moveToLearn);
        }
        moveToLearn = null;
        state = InventoryUIState.ItemSelection;
    }
}
