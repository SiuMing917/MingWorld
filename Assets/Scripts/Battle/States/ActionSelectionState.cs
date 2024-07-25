using GDEUtils.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionSelectionState : State<BattleSystem>
{
    [SerializeField] ActionSelectionUI selectionUI;

    public static ActionSelectionState i { get; private set; }
    private void Awake()
    {
        i = this;
    }

    BattleSystem bs;
    public override void Enter(BattleSystem owner)
    {
        bs = owner;

        selectionUI.gameObject.SetActive(true);
        selectionUI.OnSelected += OnActionSelected;

        bs.DialogBox.SetDialog("想做咩?");
    }

    public override void Execute()
    {
        selectionUI.HandleUpdate();
    }

    public override void Exit()
    {
        selectionUI.gameObject.SetActive(false);
        selectionUI.OnSelected -= OnActionSelected;
    }

    void OnActionSelected(int selection)
    {
        if(selection == 0)
        {
            //戰鬥
            bs.SelectedAction = BattleAction.Move;
            //SET 掙扎和休息技能
            Move useStruggle = new Move(bs.DefaultMoveBases[0]);
            Move useRest = new Move(bs.DefaultMoveBases[1]);
            Move noneMove= new Move(bs.DefaultMoveBases[2]);

            MoveSelectionState.i.Moves = bs.PlayerUnit.Pokemon.Moves;
            MoveSelectionState.i.Pokemon = bs.PlayerUnit.Pokemon;
            MoveSelectionState.i.UseStruggle = useStruggle;
            MoveSelectionState.i.UseRest = useRest;
            MoveSelectionState.i.NoneMove = noneMove;

            bs.StateMachine.ChangeState(MoveSelectionState.i);
        }
        else if (selection == 1)
        {
            //背包
            StartCoroutine(GoToInventoryState());
        }
        else if (selection == 2)
        {
            //更換學生
            StartCoroutine(GoToPartyState());
        }
        else if (selection == 3)
        {
            //逃跑
            bs.SelectedAction = BattleAction.Run;
            bs.StateMachine.ChangeState(RunTurnState.i);
        }
    }

    IEnumerator GoToPartyState()
    { 
        //更換學生
        yield return GameControlller.Instance.StateMachine.PushAndWait(PartyState.i);
        var selectedPokemon = PartyState.i.SelectedPokemon;
        if(selectedPokemon != null)
        {
            bs.SelectedAction = BattleAction.SwitchPokemon;
            bs.SelectedPekomon = selectedPokemon;
            bs.StateMachine.ChangeState(RunTurnState.i);
        }
    }

    IEnumerator GoToInventoryState()
    {
        //使用背包道具
        yield return GameControlller.Instance.StateMachine.PushAndWait(InventoryState.i);
        var selectedItem = InventoryState.i.SelectedItem;
        if(selectedItem != null)
        {
            bs.SelectedAction = BattleAction.UseItem;
            bs.SelectedItem = selectedItem;
            bs.StateMachine.ChangeState(RunTurnState.i);
        }
    }
}
