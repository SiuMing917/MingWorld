using GDEUtils.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopSellingState : State<GameControlller>
{
    [SerializeField] InventoryUI inventoryUI;
    [SerializeField] WalletUI walletUI;
    [SerializeField] CountSelectorUI countSelectorUI;

    public static ShopSellingState i { get; private set; }

    private void Awake()
    {
        i = this;
    }

    Inventory inventory;

    private void Start()
    {
        inventory = Inventory.GetInventory();
    }

    //Input
    public List<ItemBase> AvailableItems { get; set; }

    GameControlller gc;

    public override void Enter(GameControlller owner)
    {
        gc = owner;

        StartCoroutine(StartSellingState());
    }

    IEnumerator StartSellingState()
    {
        yield return gc.StateMachine.PushAndWait(InventoryState.i);

        var selectedItem = InventoryState.i.SelectedItem;
        if (selectedItem != null)
        {
            yield return SellItem(selectedItem);
            StartCoroutine(StartSellingState());
        }
        else
            gc.StateMachine.Pop();
    }

    /// <summary>
    /// 賣物品
    /// </summary>
    /// <param name="item">出售物品</param>
    /// <returns></returns>
    IEnumerator SellItem(ItemBase item)
    {

        if (!item.IsSellable)
        {
            yield return DialogManager.Instance.ShowDialogText("呢樣野有用噶！你唔可以賣佢!");
            yield break;
        }
        //顯示錢包
        walletUI.Show();

        float sellingPrice = Mathf.Round(item.Price / 2);
        int countToSell = 1;

        //獲得背包物品的數量
        //實現多賣
        int itemCount = inventory.GetItemCount(item);
        if (itemCount > 1)
        {
            yield return DialogManager.Instance.ShowDialogText($"你想要賣多少呢?",
                waitForinput: false, autoClose: false);
            //通過事件為將 選中的數目為 coutTosell賦值
            yield return countSelectorUI.ShowSelector(itemCount, sellingPrice,
                 (selectedCount) => countToSell = selectedCount);
        }

        //計算
        sellingPrice = countToSell * sellingPrice;

        int selectedChoice = 0;
        yield return DialogManager.Instance.ShowDialogText($"該物品出售價錢為{sellingPrice}是否要賣掉它？",
            choices: new List<string>() { "是", "否", },
            onChoiceSelected: (choiceIndex) => selectedChoice = choiceIndex);

        if (selectedChoice == 0)
        {
            //是
            inventory.RemoveItem(item, countToSell);
            //TODO：將錢添加到玩家的錢包中
            Wallet.i.AddMoney(sellingPrice);
            yield return DialogManager.Instance.ShowDialogText($"失去{item.Name} 獲得{sellingPrice}!");
        }

        //買後，Close銀包
        walletUI.Close();
    }
}
