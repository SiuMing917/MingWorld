using GDEUtils.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AboutToUseState : State<BattleSystem>
{
    //Input
    public Pokemon NewPokemon { get; set; }

    bool aboutToUseChoice;
    public static AboutToUseState i { get; private set; }
    private void Awake()
    {
        i = this;
    }

    BattleSystem bs;
    public override void Enter(BattleSystem owner)
    {
        bs = owner;
        StartCoroutine(StartState());
    }

    IEnumerator StartState()
    {
        yield return bs.DialogBox.TypeDialog($"{bs.Trainer.Name}將派出{NewPokemon.Base.Name}, 是否要切換學生對抗?");
        bs.DialogBox.EnableChoiceBox(true);
    }

    public override void Execute()
    {
        if (!bs.DialogBox.IsChoiceBoxEnabled)
            return;

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
            aboutToUseChoice = !aboutToUseChoice;

        bs.DialogBox.UpdaChoiceBox(aboutToUseChoice);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            bs.DialogBox.EnableChoiceBox(false);
            if (aboutToUseChoice == true)
            {
                //當玩家打開隊伍切換Pokemon結束時
                //對手派出Pokemon
                StartCoroutine(SwitchAndContinueBattle());
            }
            else
            {
                //沒有 則打開
                StartCoroutine(ContinueBattle());
            }
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            bs.DialogBox.EnableChoiceBox(false);
            StartCoroutine(ContinueBattle());
        }
    }

    IEnumerator SwitchAndContinueBattle()
    {
        yield return GameControlller.Instance.StateMachine.PushAndWait(PartyState.i);
        var selectedPokemon = PartyState.i.SelectedPokemon;
        if(selectedPokemon != null)
        {
            yield return bs.SwitchPokemon(selectedPokemon);
        }

        yield return ContinueBattle();
    }
    IEnumerator ContinueBattle()
    {
        yield return bs.SendNextTrainerPokemon();
        bs.StateMachine.Pop();
    }
}
