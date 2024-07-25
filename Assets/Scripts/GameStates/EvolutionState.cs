using GDEUtils.StateMachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EvolutionState : State<GameControlller>
{
    [SerializeField] GameObject evolutionUI;
    [SerializeField] Image pokemonImage;

    [SerializeField] AudioClip evolutionMusic;

    public event Action OnStartEvolution;
    public event Action OnCompleteEvolution;

    public static EvolutionState i { get; private set; }

    private void Awake()
    {
        i = this;
    }
    /// <summary>
    /// 進化執行
    /// </summary>
    /// <param name="pokemon">執行Pokemon</param>
    /// <param name="evolution">進化對象</param>
    /// <returns></returns>
    public IEnumerator Evolve(Pokemon pokemon, Evolution evolution)
    {
        var gc = GameControlller.Instance;
        gc.StateMachine.Push(this);

        evolutionUI.SetActive(true);

        //進化播放音樂 執行
        AudioManager.i.PlayMusic(evolutionMusic);

        pokemonImage.sprite = pokemon.Base.FrontSprite;
        yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name}進化中");

        var oldPokemon = pokemon.Base;
        pokemon.Evolve(evolution);


        pokemonImage.sprite = pokemon.Base.FrontSprite;
        yield return DialogManager.Instance.ShowDialogText($"{oldPokemon.Name}進化成了{pokemon.Base.Name}");

        evolutionUI.SetActive(false);

        gc.PartyScreen.SetPartyDate();
        //進化結束 播放
        AudioManager.i.PlayMusic(gc.CurrentScene.SceneMusic, fade: true);

        gc.StateMachine.Pop();
    }
}
