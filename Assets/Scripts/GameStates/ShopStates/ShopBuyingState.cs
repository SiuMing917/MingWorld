using GDEUtils.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopBuyingState : State<GameControlller>
{
    [SerializeField] Vector2 shopCameraOffset;
    [SerializeField] ShopUI shopUI;
    [SerializeField] WalletUI walletUI;
    [SerializeField] CountSelectorUI countSelectorUI;
    public static ShopBuyingState i { get; private set; }

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

    bool browseItems = false;

    GameControlller gc;

    public override void Enter(GameControlller owner)
    {
        gc = owner;

        browseItems = false;
        StartCoroutine(StartBuyingState());
    }

    public override void Execute()
    {
        if(browseItems)
            shopUI.HandleUpdate();
    }

    IEnumerator StartBuyingState()
    {
        //買

        yield return GameControlller.Instance.MoveCamera(shopCameraOffset);
        walletUI.Show();
        shopUI.Show(AvailableItems, (item) => StartCoroutine(BuyItem(item)),
          () => StartCoroutine(OnBackFromBuying()));

        browseItems = true;
    }

    /// <summary>
    /// 買野
    /// </summary>
    /// <param name="item">選要買的東西</param>
    /// <returns></returns>
    IEnumerator BuyItem(ItemBase item)
    {
        browseItems = false;

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

            //選擇購買
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

        browseItems = true;
    }
    /// <summary>
    /// 從購買畫面返回
    /// </summary>
    IEnumerator OnBackFromBuying()
    {
        yield return GameControlller.Instance.MoveCamera(-shopCameraOffset);

        shopUI.Close();
        walletUI.Close();
        gc.StateMachine.Pop();
    }
}
