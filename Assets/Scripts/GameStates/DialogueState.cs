using GDEUtils.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueState : State<GameControlller>
{
    public static DialogueState i { get; private set; }

    private void Awake()
    {
        i = this;
    }
}
