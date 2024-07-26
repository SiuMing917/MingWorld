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

        //���`�i�J�N�M�z�ǲߧޯ�����奻
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
            //�ιD��
            Debug.Log("�ιD��");
            StartCoroutine(GoToUseItemState());
        }
        else if (prevState == BattleState.i)
        {
            var battleState = prevState as BattleState;

            DynamicMenuState.i.MenuItems = new List<string>() { "�洫�W��", "��O����", "�����ާ@" };
            yield return gc.StateMachine.PushAndWait(DynamicMenuState.i);
            if (DynamicMenuState.i.SelectedItem == 0)
            {
                //�洫Pokemon�X��
                //��o�����e�襤��Pokemon

                if (SelectedPokemon.HP <= 0)
                {
                    partyScreen.SetMessageText("�ǥ͵L�k�԰�");
                    yield break;
                }
                if (SelectedPokemon == battleState.BattleSystem.PlayerUnit.Pokemon)
                {
                    partyScreen.SetMessageText("��e�X�����襤���ǥ�");
                    yield break;
                }
            }
            else if (DynamicMenuState.i.SelectedItem == 1)
            {
                //����
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
                    partyScreen.SetMessageText("���i�H�P�ۤv�洫��m�I�I�I");
                    yield break;
                }

                isSwitchingPosition = false;

                var tmpPokemon = playerParty.Pokemons[selectedIndexForSwitching];
                playerParty.Pokemons[selectedIndexForSwitching] = playerParty.Pokemons[selectedPokemonIndex];
                playerParty.Pokemons[selectedPokemonIndex] = tmpPokemon;
                playerParty.PartyUpdated();

                yield break;

            }

            DynamicMenuState.i.MenuItems = new List<string>() { "��O����", "�洫��m", "�����ާ@" };
            yield return gc.StateMachine.PushAndWait(DynamicMenuState.i);
            if(DynamicMenuState.i.SelectedItem == 0)
            {
                //Pokemon�@���e��
                SummaryState.i.SelectedPokemonIndex = selectedPokemonIndex;
                yield return gc.StateMachine.PushAndWait(SummaryState.i);
            }
            else if (DynamicMenuState.i.SelectedItem == 1)
            {
                //�洫Pokemon��m
                isSwitchingPosition = true;
                selectedIndexForSwitching = selectedPokemonIndex;
                partyScreen.SetMessageText("�п�ܭn�P��e�ǥͥ洫��m���ǥ͡C");
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
                partyScreen.SetMessageText("�п�ܾǥ�");
                return;
            }
        }
        gc.StateMachine.Pop();
    }
}
