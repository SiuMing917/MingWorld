using GDEUtils.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleState : State<GameControlller>
{
    [SerializeField] BattleSystem battleSystem;

    //Input
    public BattleTrigger trigger { get; set; }
    public TrainerController trainer { get; set; }

    public static BattleState i { get; private set; }
    private void Awake()
    {
        i = this;
    }

    GameControlller gc;
    public override void Enter(GameControlller owner)
    {
        gc = owner;

        //啟動battleSystem
        battleSystem.gameObject.SetActive(true);
        //關閉 世界地圖攝像機
        gc.WorldCamera.gameObject.SetActive(false);

        //將PokemonParty元件附加到PlayerController物件上
        var playerParty = gc.PlayerController.GetComponent<PokemonParty>();

        //Check 野生 or NPC對戰
        if (trainer == null)
        {
            //從當前場景中  獲得MapArea元件（腳本  調用生成野生寶可夢的方法
            var wildPokemon = gc.CurrentScene.GetComponent<MapArea>().GetWildPokemon(trigger);
            //複製出新的寶可夢 創建一個物件
            var wildPokemonCopy = new Pokemon(wildPokemon.Base, wildPokemon.Level);
            battleSystem.StartBattle(playerParty, wildPokemonCopy, trigger);
        }
        else
        {
            var trainerParty = trainer.GetComponent<PokemonParty>();
            battleSystem.StartTrainerBattle(playerParty, trainerParty);
        }

        battleSystem.OnBattleOver += EndBattle;
    }

    public override void Execute()
    {
        battleSystem.HandleUpdate();
    }

    public override void Exit()
    {
        //啟動battleSystem
        battleSystem.gameObject.SetActive(false);
        //關閉 世界地圖攝像機
        gc.WorldCamera.gameObject.SetActive(true);

        battleSystem.OnBattleOver -= EndBattle;
    }

    void EndBattle(bool won)
    {
        if (trainer != null && won == true)
        {
            trainer.BattleLost();
            trainer = null;
        }

        gc.StateMachine.Pop();
    }

    public BattleSystem BattleSystem => battleSystem;
}
