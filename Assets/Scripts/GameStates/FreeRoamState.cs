using GDEUtils.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeRoamState : State<GameControlller>
{
    /// <summary>
    /// State�i�H�OGameController���D�OWhat State
    /// </summary>
    public static FreeRoamState i { get; private set; }

    private void Awake()
    {
        i = this;
    }


    //GameController�H�eState�����F�������o��
    GameControlller gc;
    public override void Enter(GameControlller owner)
    {
        gc = owner;
    }
    public override void Execute()
    {
        PlayerController.i.HandleUpdate();

        if (Input.GetKeyDown(KeyCode.Return))
            gc.StateMachine.Push(GameMenuState.i);
    }
}
