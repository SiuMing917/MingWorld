using GDEUtils.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeRoamState : State<GameControlller>
{
    /// <summary>
    /// State可以令GameController知道是What State
    /// </summary>
    public static FreeRoamState i { get; private set; }

    private void Awake()
    {
        i = this;
    }


    //GameController以前State做的東西全部放這裏
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
