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

        //�Ұ�battleSystem
        battleSystem.gameObject.SetActive(true);
        //���� �@�ɦa���ṳ��
        gc.WorldCamera.gameObject.SetActive(false);

        //�NPokemonParty������[��PlayerController����W
        var playerParty = gc.PlayerController.GetComponent<PokemonParty>();

        //Check ���� or NPC���
        if (trainer == null)
        {
            //�q��e������  ��oMapArea����]�}��  �եΥͦ������_�i�ڪ���k
            var wildPokemon = gc.CurrentScene.GetComponent<MapArea>().GetWildPokemon(trigger);
            //�ƻs�X�s���_�i�� �Ыؤ@�Ӫ���
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
        //�Ұ�battleSystem
        battleSystem.gameObject.SetActive(false);
        //���� �@�ɦa���ṳ��
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
