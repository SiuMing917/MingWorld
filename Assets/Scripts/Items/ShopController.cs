using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShopState { Memu, Buying, Selling, Busy }

public class ShopController : MonoBehaviour
{
    [SerializeField] InventoryUI inventoryUI;
    [SerializeField] WalletUI walletUI;
    [SerializeField] CountSelectorUI countSelectorUI;
    [SerializeField] ShopUI shopUI;
    [SerializeField] Vector2 shopCameraOffset;

    public event Action OnStart;
    public event Action OnFinish;

    ShopState state;

    Merchant merchant;
    public static ShopController i { get; private set; }

    private void Awake()
    {
        i = this;
    }

    Inventory inventory;

    private void Start()
    {
        inventory = Inventory.GetInventory();
    }

    /// <summary>
    /// 開始交易
    /// </summary>
    /// <param name="merchant"></param>
    /// <returns></returns>
    public IEnumerator StarTrading(Merchant merchant)
    {
        this.merchant = merchant;

        OnStart?.Invoke();
        yield return StartMemuState();
    }

    /// <summary>
    /// 選擇交易類型
    /// </summary>
    /// <returns></returns>
    IEnumerator StartMemuState()
    {
        state = ShopState.Memu;


        int selectedChoice = 0;
        yield return DialogManager.Instance.ShowDialogText("靚仔，您想做咩啊？依度係正當商店啊！",
            choices: new List<string>() { "買", "賣", "再見" },
            onChoiceSelected: (choiceIndex) => selectedChoice = choiceIndex);


        if (selectedChoice == 0)
        {
            //買

            yield return GameControlller.Instance.MoveCamera(shopCameraOffset);
            walletUI.Show();
            shopUI.Show(merchant.AvailableItems, (item) => StartCoroutine(BuyItem(item)),
              () => StartCoroutine(OnBackFromBuying()));

            state = ShopState.Buying;

        }
        else if (selectedChoice == 1)
        {
            //賣
            state = ShopState.Selling;
            inventoryUI.gameObject.SetActive(true);
        }
        else if (selectedChoice == 2)
        {
            OnFinish.Invoke();
            //退出
        }
    }
    /// <summary>
    /// 畫面控制
    /// </summary>
    public void HandleUpdate()
    {
        if (state == ShopState.Selling)
        {
            inventoryUI.HandleUpdate(OnBackFromSelling,
                (selectedItem) => StartCoroutine(SellItem(selectedItem)));
        }
        else if (state == ShopState.Buying)
        {
            shopUI.HandleUpdate();
        }
    }
    /// <summary>
    /// 從賣 的介面 回到交易選擇介面
    /// </summary>
    void OnBackFromSelling()
    {
        inventoryUI.gameObject.SetActive(false);
        StartCoroutine(StartMemuState());
    }
    /// <summary>
    /// 賣物品
    /// </summary>
    /// <param name="item">出售物品</param>
    /// <returns></returns>
    IEnumerator SellItem(ItemBase item)
    {
        //賣野邏輯，係Sellable就可以賣
        state = ShopState.Busy;

        if (!item.IsSellable)
        {
            yield return DialogManager.Instance.ShowDialogText("呢樣野有用噶！你唔可以賣佢!");
            state = ShopState.Selling;
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
            yield return DialogManager.Instance.ShowDialogText($"你想要卖多少呢?",
                waitForinput: false, autoClose: false);
            //通過事件為將 選中的數目為 coutTosell賦值
            yield return countSelectorUI.ShowSelector(itemCount, sellingPrice,
                 (selectedCount) => countToSell = selectedCount);
        }

        //計算
        sellingPrice = countToSell * sellingPrice;

        int selectedChoice = 0;
        yield return DialogManager.Instance.ShowDialogText($"该物品出售价格为{sellingPrice}是否要卖掉它",
            choices: new List<string>() { "是", "否", },
            onChoiceSelected: (choiceIndex) => selectedChoice = choiceIndex);

        if (selectedChoice == 0)
        {
            //是
            inventory.RemoveItem(item, countToSell);
            //TODO：將錢添加到玩家的錢包中
            Wallet.i.AddMoney(sellingPrice);
            yield return DialogManager.Instance.ShowDialogText($"失去{item.Name} 获得{sellingPrice}!");
        }

        //出售玩 關閉錢包
        walletUI.Close();

        state = ShopState.Selling;
    }

    /// <summary>
    /// 購買
    /// </summary>
    /// <param name="item">選擇購買的物品</param>
    /// <returns></returns>
    IEnumerator BuyItem(ItemBase item)
    {
        state = ShopState.Busy;

        yield return DialogManager.Instance.ShowDialogText("你想要買幾多個呢?",
            waitForinput: false, autoClose: false);
        int countToBuy = 1;
        yield return countSelectorUI.ShowSelector(100, item.Price,
            (selectedCount) => countToBuy = selectedCount);
        DialogManager.Instance.CloseDialog();

        float totalPrice = item.Price * countToBuy;


        if (Wallet.i.HasMoney(totalPrice))
        {
            int selectedChoice = 0;
            yield return DialogManager.Instance.ShowDialogText($"將會消費{totalPrice}",
                choices: new List<string>() { "是", "否", },
                onChoiceSelected: (choiceIndex) => selectedChoice = choiceIndex);

            //选择购买
            if (selectedChoice == 0)
            {
                //是
                inventory.AddItem(item, countToBuy);
                Wallet.i.takeMoney(totalPrice);
                yield return DialogManager.Instance.ShowDialogText("多謝幫襯~~~");
            }

        }
        else
        {
            yield return DialogManager.Instance.ShowDialogText("無錢都想來買野？!");
        }

        state = ShopState.Buying;
    }
    /// <summary>
    /// 從購買介面返回
    /// </summary>
    IEnumerator OnBackFromBuying()
    {
        yield return GameControlller.Instance.MoveCamera(-shopCameraOffset);

        shopUI.Close();
        walletUI.Close();
        StartCoroutine(StartMemuState());

    }

}
