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
    /// �檫�~
    /// </summary>
    /// <param name="item">�X�⪫�~</param>
    /// <returns></returns>
    IEnumerator SellItem(ItemBase item)
    {

        if (!item.IsSellable)
        {
            yield return DialogManager.Instance.ShowDialogText("�O�˳����ξ��I�A���i�H���\!");
            yield break;
        }
        //��ܿ��]
        walletUI.Show();

        float sellingPrice = Mathf.Round(item.Price / 2);
        int countToSell = 1;

        //��o�I�]���~���ƶq
        //��{�h��
        int itemCount = inventory.GetItemCount(item);
        if (itemCount > 1)
        {
            yield return DialogManager.Instance.ShowDialogText($"�A�Q�n��h�֩O?",
                waitForinput: false, autoClose: false);
            //�q�L�ƥ󬰱N �襤���ƥج� coutTosell���
            yield return countSelectorUI.ShowSelector(itemCount, sellingPrice,
                 (selectedCount) => countToSell = selectedCount);
        }

        //�p��
        sellingPrice = countToSell * sellingPrice;

        int selectedChoice = 0;
        yield return DialogManager.Instance.ShowDialogText($"�Ӫ��~�X�������{sellingPrice}�O�_�n�汼���H",
            choices: new List<string>() { "�O", "�_", },
            onChoiceSelected: (choiceIndex) => selectedChoice = choiceIndex);

        if (selectedChoice == 0)
        {
            //�O
            inventory.RemoveItem(item, countToSell);
            //TODO�G�N���K�[�쪱�a�����]��
            Wallet.i.AddMoney(sellingPrice);
            yield return DialogManager.Instance.ShowDialogText($"���h{item.Name} ��o{sellingPrice}!");
        }

        //�R��AClose�ȥ]
        walletUI.Close();
    }
}
