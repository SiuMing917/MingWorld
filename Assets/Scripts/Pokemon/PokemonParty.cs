using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PokemonParty : MonoBehaviour
{
    [SerializeField] List<Pokemon> pokemons;

    public event Action OnUpdated;

    public List<Pokemon> Pokemons
    {
        get
        {
            return pokemons;
        }
        set
        {
            pokemons = value;

            OnUpdated.Invoke();
        }
    }

    private void Awake()
    {
        foreach (var pokemon in Pokemons)
        {
            pokemon.Init();
        }
    }
    private void Start()
    {
    }
    /// <summary>
    /// 返回Pokemon HP中大於0 且為list集合第一個 
    /// </summary>
    /// <returns></returns>
    public Pokemon GetHealthyPokemon()
    {
        //Link  返回Pokemon中大於0 且為list集合第一個
        return Pokemons.Where(x => x.HP > 0).FirstOrDefault();
    }


    /// <summary>
    /// 加入隊伍
    /// </summary>
    /// <param name="newPokemon"></param>
    public void AddPokemon(Pokemon newPokemon)
    {
        if (pokemons.Count < 6)
        {
            pokemons.Add(newPokemon);

            //Pokemon添加時更新資料
            OnUpdated?.Invoke();
        }
        else
        {
            //要加入電腦中
        }
    }

    /// <summary>
    /// 檢測寶可夢隊伍中是否有可進化的
    /// </summary>
    /// <returns></returns>
    public bool CheckForEvolution()
    {
        //這個條件會對集合中的每個元素（這裡是 Pokemon 物件 p）調用 CheckForEvolution() 方法，
        //判斷其返回值是否不為 null。如果有任何一個元素滿足條件，Any() 方法就會返回 true，否則返回 false。
        return pokemons.Any(p => p.CheckForEvolution() != null);
    }

    /// <summary>
    /// 執行Pokemon進化
    /// </summary>
    /// <returns></returns>
    public IEnumerator RunEvolution()
    {
        foreach (var pokemon in pokemons)
        {
            var evoution = pokemon.CheckForEvolution();
            if (evoution != null)
            {
                yield return EvolutionState.i.Evolve(pokemon, evoution);
            }
        }
    }
    /// <summary>
    /// 更新UI畫面
    /// </summary>
    public void PartyUpdated()
    {
        OnUpdated?.Invoke();
    }



    /// <summary>
    /// 獲得PlayerController 下的PokemonParty元件
    /// 也就是玩家的寶可夢隊伍
    /// </summary>
    /// <returns></returns>
    public static PokemonParty GetPlayerParty()
    {
        return FindObjectOfType<PlayerController>().GetComponent<PokemonParty>();
    }
}
