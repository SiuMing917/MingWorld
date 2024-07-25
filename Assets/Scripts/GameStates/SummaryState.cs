using GDEUtils.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummaryState : State<GameControlller>
{
    [SerializeField] SummaryScreenUI summaryScreen;

    // Input

    public int SelectedPokemonIndex { get; set; }

    public static SummaryState i { get; set; }

    private void Awake()
    {
        i = this;
    }

    List<Pokemon> playerParty;
    private void Start()
    {
        playerParty = PlayerController.i.GetComponent<PokemonParty>().Pokemons;
    }

    GameControlller gc;

    public override void Enter(GameControlller owner)
    {
        gc = owner;

        summaryScreen.gameObject.SetActive(true);
        summaryScreen.SetBasicDetails(playerParty[SelectedPokemonIndex]);
        summaryScreen.SetSkills();
    }

    public override void Execute()
    {

        int prevSelection = SelectedPokemonIndex;

        if(Input.GetKeyDown(KeyCode.X))
        {
                gc.StateMachine.Pop();
                return;
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            SelectedPokemonIndex += 1;

            if (SelectedPokemonIndex >= playerParty.Count)
                SelectedPokemonIndex = 0;
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            SelectedPokemonIndex -= 1;

            if (SelectedPokemonIndex < 0)
                SelectedPokemonIndex = playerParty.Count-1;
        }

        if(SelectedPokemonIndex != prevSelection)
        {
            summaryScreen.SetBasicDetails(playerParty[SelectedPokemonIndex]);
            summaryScreen.SetSkills();
        }
    }

    public override void Exit()
    {
        summaryScreen.gameObject.SetActive(false);
    }
}
