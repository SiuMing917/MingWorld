using GDEUtils.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopMenuState : State<GameControlller>
{
    public static ShopMenuState i { get; private set; }

    private void Awake()
    {
        i = this;
    }

    //Input
    public List<ItemBase> AvailableItems { get; set; }

    GameControlller gc;

    public override void Enter(GameControlller owner)
    {
        gc = owner;

        StartCoroutine(StartMenuState());
    }

    /// <summary>
    /// ��ܥ������
    /// </summary>
    /// <returns></returns>
    IEnumerator StartMenuState()
    {
        int selectedChoice = 0;
        yield return DialogManager.Instance.ShowDialogText("谥J�A�z�Q�����ڡH�̫׫Y����ө��ڡI",
            choices: new List<string>() { "�R", "��", "�A��" },
            onChoiceSelected: (choiceIndex) => selectedChoice = choiceIndex);


        if (selectedChoice == 0)
        {
            //�R
            ShopBuyingState.i.AvailableItems = AvailableItems;
            yield return gc.StateMachine.PushAndWait(ShopBuyingState.i);
        }
        else if (selectedChoice == 1)
        {
            //��
            yield return gc.StateMachine.PushAndWait(ShopSellingState.i);
        }
        else if (selectedChoice == 2)
        {
            //�h�X
        }
        gc.StateMachine.Pop();
    }
    /// <summary>
    /// �e������
    /// </summary>
}
