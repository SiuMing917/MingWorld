using GDEUtils.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartyState : State<GameControlller>
{
    [SerializeField] PartyScreen partyScreen;

    public Pokemon SelectedPokemon { get; private set; }
    public static PartyState i { get; private set; }

    private void Awake()
    {
        i = this;
    }

    GameControlller gc;

    public override void Enter(GameControlller owner)
    {
        gc = owner;

        SelectedPokemon = null;
        partyScreen.gameObject.SetActive(true);
        partyScreen.OnSelected += OnPokemonSelected;
        partyScreen.OnBack += OnBack;
    }

    public override void Execute()
    {
        partyScreen.HandleUpdate();
    }

    public override void Exit()
    {
        partyScreen.gameObject.SetActive(false);
        partyScreen.OnSelected -= OnPokemonSelected;
        partyScreen.OnBack -= OnBack;
    }

    void OnPokemonSelected(int selection)
    {
        SelectedPokemon = partyScreen.SelectedMember;

        var prevState = gc.StateMachine.GetPrevState();
        if (prevState == InventoryState.i)
        {
            //用道具
            Debug.Log("用道具");
            StartCoroutine(GoToUseItemState());
        }
        else if(prevState == BattleState.i)
        {
            var battleState = prevState as BattleState;

            //獲得隊伍中當前選中的Pokemon

            if (SelectedPokemon.HP <= 0)
            {
                partyScreen.SetMessageText("學生無法戰鬥");
                return;
            }
            if (SelectedPokemon == battleState.BattleSystem.PlayerUnit.Pokemon)
            {
                partyScreen.SetMessageText("當前出場為選中的學生");
                return;
            }

            gc.StateMachine.Pop();
        }
        else
        {
            //要做： Pokemon一覽畫面
            Debug.Log($"選擇學生at Index {selection}");
        }
    }

    IEnumerator GoToUseItemState()
    {
        yield return gc.StateMachine.PushAndWait(UseItemState.i);
        gc.StateMachine.Pop();
    }

    void OnBack()
    {
        SelectedPokemon = null;

        var prevState = gc.StateMachine.GetPrevState();
        if(prevState == BattleState.i)
        {
            var battleState = prevState as BattleState;
            if (battleState.BattleSystem.PlayerUnit.Pokemon.HP < 0)
            {
                partyScreen.SetMessageText("請選擇學生");
                return;
            }
        }
        gc.StateMachine.Pop();
    }
}
