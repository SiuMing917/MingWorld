using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapArea : MonoBehaviour
{
    [SerializeField] List<PokemonEncounterRecord> wildPokemons;
    [SerializeField] List<PokemonEncounterRecord> wildPokemonsInWater;

    [HideInInspector]
    [SerializeField] int totalChance = 0;

    [HideInInspector]
    [SerializeField] int totalChanceWater = 0;

    private void OnValidate()
    {
        CalculateChancePercentage();
    }

    private void Start()
    {
        CalculateChancePercentage();
    }

    void CalculateChancePercentage()
    {
        totalChance = -1;
        totalChanceWater = -1;

        //Check 野生Pokemon List 係唔係有至少一隻。
        if (wildPokemons.Count > 0)
        {
            totalChance = 0;
            foreach (var record in wildPokemons)
            {
                record.chanceLower = totalChance;
                record.chanceUpper = totalChance + record.chancePercentage;

                totalChance = totalChance + record.chancePercentage;
            }
        }

        if (wildPokemonsInWater.Count > 0)
        {
            totalChanceWater = 0;
            foreach (var record in wildPokemonsInWater)
            {
                record.chanceLower = totalChanceWater;
                record.chanceUpper = totalChanceWater + record.chancePercentage;

                totalChanceWater = totalChanceWater + record.chancePercentage;
            }
        }
    }

    /*
    private void Start()
    {
        int totalChance = 0;
        foreach (var record in wildPokemons)
        {
            record.chanceLower = totalChance;
            record.chanceUpper = totalChance + record.chancePercentage;
            totalChance = totalChance + record.chancePercentage;
        }
    }
    */

    /// <summary>
    /// 返回Pokemon 並給他它始數值 隨機生成
    /// </summary>
    /// <returns></returns>
    public Pokemon GetWildPokemon(BattleTrigger trigger)
    {
        //Initialize Pokemon List，跟住再Set係邊度的野生Pokemon
        var pokemonList = wildPokemons;
        if(trigger == BattleTrigger.LongGrass)
        {
            pokemonList = wildPokemons;
        }
        else if(trigger == BattleTrigger.Water)
        {
            pokemonList = wildPokemonsInWater;
        }

        //隨機數
        int randVal = Random.Range(1, 101);
        //出現概率的Pokemon
        var pokemonRecord = pokemonList.First(p => randVal >= p.chanceLower && randVal <= p.chanceUpper);
        //隨機的Level
        var levelRange = pokemonRecord.levelRange;
        int level = levelRange.y == 0 ? levelRange.x : Random.Range(levelRange.x, levelRange.y + 1);
        //Return出現的Pokemon
        var wildPokemon = new Pokemon(pokemonRecord.pokemon, level);

        //var wildPokemon= wildPokemons[Random.Range(0, wildPokemons.Count)];
        wildPokemon.Init();
        return wildPokemon;
    }


}
/// <summary>
///Pokemon遭遇屬性
/// </summary>
[System.Serializable]
public class PokemonEncounterRecord
{
    public PokemonBase pokemon;
    public Vector2Int levelRange;
    public int chancePercentage;

    public int chanceLower { get; set; }
    public int chanceUpper { get; set; }
}
