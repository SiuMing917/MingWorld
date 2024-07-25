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
    /// 選擇交易類型
    /// </summary>
    /// <returns></returns>
    IEnumerator StartMenuState()
    {
        int selectedChoice = 0;
        yield return DialogManager.Instance.ShowDialogText("靚仔，您想做咩啊？依度係正當商店啊！",
            choices: new List<string>() { "買", "賣", "再見" },
            onChoiceSelected: (choiceIndex) => selectedChoice = choiceIndex);


        if (selectedChoice == 0)
        {
            //買
            ShopBuyingState.i.AvailableItems = AvailableItems;
            yield return gc.StateMachine.PushAndWait(ShopBuyingState.i);
        }
        else if (selectedChoice == 1)
        {
            //賣
            yield return gc.StateMachine.PushAndWait(ShopSellingState.i);
        }
        else if (selectedChoice == 2)
        {
            //退出
        }
        gc.StateMachine.Pop();
    }
    /// <summary>
    /// 畫面控制
    /// </summary>
}
