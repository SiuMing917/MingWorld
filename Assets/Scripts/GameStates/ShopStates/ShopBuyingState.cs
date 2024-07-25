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
        //�R

        yield return GameControlller.Instance.MoveCamera(shopCameraOffset);
        walletUI.Show();
        shopUI.Show(AvailableItems, (item) => StartCoroutine(BuyItem(item)),
          () => StartCoroutine(OnBackFromBuying()));

        browseItems = true;
    }

    /// <summary>
    /// �R��
    /// </summary>
    /// <param name="item">��n�R���F��</param>
    /// <returns></returns>
    IEnumerator BuyItem(ItemBase item)
    {
        browseItems = false;

        yield return DialogManager.Instance.ShowDialogText("�A�Q�n�R�X�h�өO?",
            waitForinput: false, autoClose: false);
        int countToBuy = 1;
        yield return countSelectorUI.ShowSelector(100, item.Price,
            (selectedCount) => countToBuy = selectedCount);
        DialogManager.Instance.CloseDialog();

        float totalPrice = item.Price * countToBuy;


        if (Wallet.i.HasMoney(totalPrice))
        {
            int selectedChoice = 0;
            yield return DialogManager.Instance.ShowDialogText($"�N�|���O{totalPrice}",
                choices: new List<string>() { "�O", "�_", },
                onChoiceSelected: (choiceIndex) => selectedChoice = choiceIndex);

            //����ʶR
            if (selectedChoice == 0)
            {
                //�O
                inventory.AddItem(item, countToBuy);
                Wallet.i.takeMoney(totalPrice);
                yield return DialogManager.Instance.ShowDialogText("�h����Ũ~~~");
            }

        }
        else
        {
            yield return DialogManager.Instance.ShowDialogText("�L�����Q�ӶR���H!");
        }

        browseItems = true;
    }
    /// <summary>
    /// �q�ʶR�e����^
    /// </summary>
    IEnumerator OnBackFromBuying()
    {
        yield return GameControlller.Instance.MoveCamera(-shopCameraOffset);

        shopUI.Close();
        walletUI.Close();
        gc.StateMachine.Pop();
    }
}
