using GDEUtils.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSelectionState : State<BattleSystem>
{
    [SerializeField] MoveSelectionUI selectionUI;
    [SerializeField] GameObject moveDetilsUI;
    
    // Input
    public List<Move> Moves { get; set; }
    public Pokemon Pokemon { get; set; }
    public Move UseStruggle { get; set; }
    public Move UseRest { get; set; }
    public Move NoneMove { get; set; }

    public static MoveSelectionState i { get; private set; }

    private void Awake()
    {
        i = this;
    }

    BattleSystem bs;
    public override void Enter(BattleSystem owner)
    {
        bs = owner;
        selectionUI.SetMoves(Moves, Pokemon, UseStruggle, UseRest, NoneMove);

        selectionUI.gameObject.SetActive(true);
        selectionUI.OnSelected += OnMoveSelected;
        selectionUI.OnBack += OnBack;

        moveDetilsUI.SetActive(true);
        bs.DialogBox.EnableDialogText(false);
    }

    public override void Execute()
    {
        selectionUI.HandleUpdate();
    }

    public override void Exit()
    {
        selectionUI.gameObject.SetActive(false);
        selectionUI.OnSelected -= OnMoveSelected;
        selectionUI.OnBack -= OnBack;

        selectionUI.ClearItems();

        moveDetilsUI.SetActive(false);
        bs.DialogBox.EnableDialogText(true);
    }

    void OnMoveSelected(int selection)
    {
        
        bs.SelectedMove = selection;
        var move = UseRest;
        if (bs.SelectedMove < bs.PlayerUnit.Pokemon.Moves.Count)
        {
            move = bs.PlayerUnit.Pokemon.Moves[bs.SelectedMove];
        }
        else if(bs.SelectedMove == 4)
        {
            move = UseStruggle;
        }
        else if(bs.SelectedMove == 5)
        {
            move = UseRest;
        }
        else
        {
            Debug.Log("沒有技能");
            return;
        }

        int pokemonEnergy = bs.PlayerUnit.Pokemon.ENERGY;

        if (move.PP <= 0)
        {
            Debug.Log("PP不足");
            return;
        }
        if (pokemonEnergy < (move.Base.Energy / 2))
        {
            Debug.Log("能量不足");
            return;
        }

        bs.StateMachine.ChangeState(RunTurnState.i);
    }

    void OnBack()
    {
        bs.StateMachine.ChangeState(ActionSelectionState.i);
    }
}
