using GDEUtils.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseState : State<GameControlller>
{
    public static PauseState i { get; private set; }
    private void Awake()
    {
        i = this;
    }
}
