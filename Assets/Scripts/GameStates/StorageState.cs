using GDEUtils.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorageState : State<GameControlller>
{
    [SerializeField] PokemonStorageUI storageUI;

    public bool isMovingPokemon = false;
    int selectedSlotToMove = 0;
    Pokemon selectedPokemonToMove = null;

    PokemonParty party;
    public static StorageState i { get; private set; }

    private void Awake()
    {
        i = this;
        party = PokemonParty.GetPlayerParty();
    }

    GameControlller gc;
    public override void Enter(GameControlller owner)
    {
        gc = owner;

        storageUI.gameObject.SetActive(true);
        storageUI.SetDataInPartySlots();
        storageUI.SetDataInStorageSlots();

        storageUI.OnSelected += OnSelected;
        storageUI.OnBack += OnBack;
    }

    public override void Execute()
    {
        storageUI.HandleUpdate();
    }
    public override void Exit()
    {
        storageUI.gameObject.SetActive(false);
        storageUI.OnSelected -= OnSelected;
        storageUI.OnBack -= OnBack;
    }

    void OnSelected(int slotIndex)
    {
        if(!isMovingPokemon)
        {
            var pokemon = storageUI.TakePokemonFromSlot(slotIndex);
            if(pokemon != null)
            {
                isMovingPokemon = true;
                selectedSlotToMove = slotIndex;
                selectedPokemonToMove = pokemon;
                storageUI.SetMovingPokemonImage(slotIndex);
                Debug.Log("isMovingMode");
            }
        }
        else
        {
            isMovingPokemon = false;

            int firstSlotIndex = selectedSlotToMove;
            int secondSlotIndex = slotIndex;

            var secondPokemon = storageUI.TakePokemonFromSlot(slotIndex);
            storageUI.SetMovingPokemonImage(slotIndex);

            if (secondPokemon == null && storageUI.IsPartySlot(firstSlotIndex) && storageUI.IsPartySlot(secondSlotIndex))
            {
                storageUI.PutPokemonIntoSlot(selectedPokemonToMove, selectedSlotToMove);

                storageUI.SetDataInStorageSlots();
                storageUI.SetDataInPartySlots();

                return;
            }

            if (party.Pokemons.Count > 1 || !storageUI.IsPartySlot(firstSlotIndex))
            {
                storageUI.PutPokemonIntoSlot(selectedPokemonToMove, secondSlotIndex);
            }
            else if(secondPokemon != null)
            {
                storageUI.PutPokemonIntoSlot(selectedPokemonToMove, secondSlotIndex);
            }
            else
            {
                storageUI.PutPokemonIntoSlot(selectedPokemonToMove, selectedSlotToMove);
                storageUI.SetDataInStorageSlots();
                storageUI.SetDataInPartySlots();

                return;
            }

            if(secondPokemon != null)
            {
                storageUI.PutPokemonIntoSlot(secondPokemon, firstSlotIndex);
            }

            party.Pokemons.RemoveAll(p => p == null);
            party.PartyUpdated();

            storageUI.SetDataInStorageSlots();
            storageUI.SetDataInPartySlots();
        }
    }

    void OnBack()
    {
        if(isMovingPokemon)
        {
            isMovingPokemon = false;
            storageUI.PutPokemonIntoSlot(selectedPokemonToMove, selectedSlotToMove);
            storageUI.SetDataInStorageSlots();
            storageUI.SetDataInPartySlots();
        }
        else
        {
            gc.StateMachine.Pop();
        }
    }
}
