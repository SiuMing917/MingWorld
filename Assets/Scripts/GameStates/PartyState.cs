using GDEUtils.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartyState : State<GameControlller>
{
    [SerializeField] PartyScreen partyScreen;

    public Pokemon SelectedPokemon { get; private set; }

    bool isSwitchingPosition;
    int selectedIndexForSwitching = 0;
    public static PartyState i { get; private set; }

    private void Awake()
    {
        i = this;
    }

    PokemonParty playerParty;
    private void Start()
    {
        playerParty = PlayerController.i.GetComponent<PokemonParty>();
    }

    GameControlller gc;

    public override void Enter(GameControlller owner)
    {
        gc = owner;

        SelectedPokemon = null;

        partyScreen.gameObject.SetActive(true);

        //正常進入就清理學習技能相關文本
        if(!gc.PartyScreen.isUsingTm)
        {
            gc.PartyScreen.ClaerMemberSlotMessage();
        }

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

        StartCoroutine(PokemonSelectedAction(selection));
    }

    IEnumerator PokemonSelectedAction(int selectedPokemonIndex)
    {
        var prevState = gc.StateMachine.GetPrevState();

        if (prevState == InventoryState.i)
        {
            //用道具
            Debug.Log("用道具");
            StartCoroutine(GoToUseItemState());
        }
        else if (prevState == BattleState.i)
        {
            var battleState = prevState as BattleState;

            DynamicMenuState.i.MenuItems = new List<string>() { "交換上場", "能力概覽", "取消操作" };
            yield return gc.StateMachine.PushAndWait(DynamicMenuState.i);
            if (DynamicMenuState.i.SelectedItem == 0)
            {
                //交換Pokemon出場
                //獲得隊伍中當前選中的Pokemon

                if (SelectedPokemon.HP <= 0)
                {
                    partyScreen.SetMessageText("學生無法戰鬥");
                    yield break;
                }
                if (SelectedPokemon == battleState.BattleSystem.PlayerUnit.Pokemon)
                {
                    partyScreen.SetMessageText("當前出場為選中的學生");
                    yield break;
                }
            }
            else if (DynamicMenuState.i.SelectedItem == 1)
            {
                //概覽
                SummaryState.i.SelectedPokemonIndex = selectedPokemonIndex;
                yield return gc.StateMachine.PushAndWait(SummaryState.i);
                yield break;
            }
            else
            {
                yield break;
            }

            gc.StateMachine.Pop();
        }
        else
        {
            if(isSwitchingPosition)
            {
                if (selectedIndexForSwitching == selectedPokemonIndex)
                {
                    partyScreen.SetMessageText("不可以與自己交換位置！！！");
                    yield break;
                }

                isSwitchingPosition = false;

                var tmpPokemon = playerParty.Pokemons[selectedIndexForSwitching];
                playerParty.Pokemons[selectedIndexForSwitching] = playerParty.Pokemons[selectedPokemonIndex];
                playerParty.Pokemons[selectedPokemonIndex] = tmpPokemon;
                playerParty.PartyUpdated();

                yield break;

            }

            DynamicMenuState.i.MenuItems = new List<string>() { "能力概覽", "交換位置", "取消操作" };
            yield return gc.StateMachine.PushAndWait(DynamicMenuState.i);
            if(DynamicMenuState.i.SelectedItem == 0)
            {
                //Pokemon一覽畫面
                SummaryState.i.SelectedPokemonIndex = selectedPokemonIndex;
                yield return gc.StateMachine.PushAndWait(SummaryState.i);
            }
            else if (DynamicMenuState.i.SelectedItem == 1)
            {
                //交換Pokemon位置
                isSwitchingPosition = true;
                selectedIndexForSwitching = selectedPokemonIndex;
                partyScreen.SetMessageText("請選擇要與當前學生交換位置的學生。");
            }
            else
            {
                yield break;
            }
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
