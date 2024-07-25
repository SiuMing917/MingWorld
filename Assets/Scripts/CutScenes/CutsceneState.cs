using GDEUtils.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutsceneState : State<GameControlller>
{
    public static CutsceneState i { get; private set; }
    private void Awake()
    {
        i = this;
    }

    public override void Execute()
    {
        PlayerController.i.Character.HandleUpdate();
    }
}
