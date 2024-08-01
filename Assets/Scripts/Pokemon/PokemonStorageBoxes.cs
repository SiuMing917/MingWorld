using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokemonStorageBoxes : MonoBehaviour
{
    public Pokemon[,] boxes = new Pokemon[16, 30];

    public void AddPokemon(Pokemon pokemon, int boxIndex, int slotIndex)
    {
        boxes[boxIndex, slotIndex] = pokemon;
    }

    public void RemovePokemon(int boxIndex, int slotIndex)
    {
        boxes[boxIndex, slotIndex] = null;
    }

    public Pokemon GetPokemon(int boxIndex, int slotIndex)
    {
        return boxes[boxIndex, slotIndex];
    }

    public static PokemonStorageBoxes GetPlayerStorageBoxes()
    {
        return FindObjectOfType<PlayerController>().GetComponent<PokemonStorageBoxes>();
    }

    public IEnumerable<PokemonSaveData> GetAllPokemonData()
    {
        for (int i = 0; i < 16; i++)
        {
            for (int j = 0; j < 30; j++)
            {
                var pokemon = boxes[i, j];
                if (pokemon != null)
                {
                    yield return pokemon.GetSaveData();
                }
            }
        }
    }
}
