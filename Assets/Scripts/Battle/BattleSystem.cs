using DG.Tweening;
using GDEUtils.StateMachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 玩家行爲
/// </summary>
public enum BattleAction { Move, SwitchPokemon, UseItem, Run }

public enum BattleTrigger { LongGrass, Water}

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;

    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;

    [SerializeField] Image playerImage;
    [SerializeField] Image trainerImage;
    [SerializeField] GameObject pokeballSprite;

    [SerializeField] MoveToForgetSelectionUI moveSelectionUI;
    [SerializeField] InventoryUI inventoryUI;

    [Header("音樂")]
    //聲音
    [SerializeField] AudioClip wildBattleMusic;
    [SerializeField] AudioClip trainerBattleMusic;
    [SerializeField] AudioClip battleVictoryMusic;

    [Header("戰鬥背景圖")]
    [SerializeField] Image backgroundImage;
    [SerializeField] Sprite grassBackground;
    [SerializeField] Sprite waterBackground;

    [Header("共用技能")]
    [SerializeField] List<MoveBase> defaultMoveBases;

    //區分戰鬥是否勝利或者失敗  事件用來觸發
    public event Action<bool> OnBattleOver;

    public StateMachine<BattleSystem> StateMachine { get; private set; }

    public int SelectedMove { get; set; }
    public BattleAction SelectedAction { get; set; }
    public Pokemon SelectedPekomon { get; set; }
    public ItemBase SelectedItem { get; set; }
    public bool IsBattleOver { get; private set; }

    public PokemonParty PlayerParty { get; private set; }
    public Pokemon WildPokemon { get; private set; }
    public PokemonParty TrainerParty { get; private set; }

    public bool IsTrainerBattle { get; private set; } = false;
    PlayerController player;
    public TrainerController Trainer { get; private set; }

    /// <summary>
    /// 逃跑次數
    /// </summary>
    public int escapeAttempts { get; set; }

    /// <summary>
    /// 觸發野生Pokemon戰鬥
    /// </summary>
    /// <param name="PlayerParty"></param>
    /// <param name="WildPokemon"></param>
    //先整一個Battle Trigger去判斷戰鬥内容
    BattleTrigger battleTrigger;
    public void StartBattle(PokemonParty PlayerParty, Pokemon WildPokemon, BattleTrigger trigger = BattleTrigger.LongGrass)
    {
        //IsTrainerBattle = false;
        this.PlayerParty = PlayerParty;
        this.WildPokemon = WildPokemon;
        player = PlayerParty.GetComponent<PlayerController>();
        IsTrainerBattle = false;

        battleTrigger = trigger;

        AudioManager.i.PlayMusic(wildBattleMusic);

        StartCoroutine(SetupBattle());
    }

    /// <summary>
    /// 觸發和NPC的戰鬥
    /// </summary>
    /// <param name="playParty">玩家</param>
    /// <param name="pokemonParty">戰鬥員</param>
    public void StartTrainerBattle(PokemonParty PlayerParty, PokemonParty TrainerParty, BattleTrigger trigger = BattleTrigger.LongGrass)
    {
        this.PlayerParty = PlayerParty;
        this.TrainerParty = TrainerParty;

        IsTrainerBattle = true;
        player = PlayerParty.GetComponent<PlayerController>();
        Trainer = TrainerParty.GetComponent<TrainerController>();

        battleTrigger = trigger;

        AudioManager.i.PlayMusic(trainerBattleMusic);

        StartCoroutine(SetupBattle());
    }

    //戰鬥行爲總協程
    public IEnumerator SetupBattle()
    {
        StateMachine = new StateMachine<BattleSystem>(this);

        //開始不顯示戰鬥UI
        playerUnit.Clear();
        enemyUnit.Clear();

        //決定戰鬥背景
        backgroundImage.sprite = grassBackground;//Initialize先，之後再決定用邊個
        if(battleTrigger == BattleTrigger.LongGrass)
        {
            backgroundImage.sprite = grassBackground;
        }
        else if(battleTrigger == BattleTrigger.Water)
        {
            backgroundImage.sprite = waterBackground;
        }

        if (!IsTrainerBattle)
        {
            //野生Pokemon對戰
            playerUnit.Setup(PlayerParty.GetHealthyPokemon());
            enemyUnit.Setup(WildPokemon);

            dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);

            //用 yield return 啓動另一個協程 等待協程結束
            yield return dialogBox.TypeDialog("野生且單純的" + enemyUnit.Pokemon.Base.Name + "出現了!");
        }
        else
        {
            //戰鬥員對決

            //進入戰鬥先 顯示玩家 不顯示Pokemon
            playerUnit.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(false);

            playerImage.gameObject.SetActive(true);
            trainerImage.gameObject.SetActive(true);

            playerImage.sprite = player.Sprite;
            trainerImage.sprite = Trainer.Sprite;

            yield return dialogBox.TypeDialog($"{Trainer.Name}想要同你戰鬥");

            //對手派出第一隻Pokemon
            trainerImage.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(true);
            var trainPokemon = TrainerParty.GetHealthyPokemon();
            enemyUnit.Setup(trainPokemon);
            yield return dialogBox.TypeDialog(Trainer.Name + "派出" + enemyUnit.Pokemon.Base.Name + "！");

            //玩家派出Pokemon
            playerImage.gameObject.SetActive(false);
            playerUnit.gameObject.SetActive(true);
            var playerPokemon = PlayerParty.GetHealthyPokemon();
            playerUnit.Setup(playerPokemon);
            yield return dialogBox.TypeDialog("去啦！" + playerUnit.Pokemon.Base.Name + "！");
            dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);
        }

        IsBattleOver = false;
        //逃跑次數
        escapeAttempts = 0;
        
        partyScreen.Init();

        StateMachine.ChangeState(ActionSelectionState.i);
        
    }

    /// <summary>
    /// 觸發戰鬥結束
    /// </summary>
    /// <param name="won"></param>
    public void BattleOver(bool won)
    {
        IsBattleOver = true;
        //state = BattleStates.BattleOver;
        //讓隊伍的所有Pokemon調用狀態Initialize方法
        PlayerParty.Pokemons.ForEach(p => p.OnBattleOver());
        //通知游戲控制器（Game Controller） 結束戰鬥事件
        playerUnit.Hud.ClearData();
        OnBattleOver(won);
    }

    /// <summary>
    ///戰鬥中的行爲邏輯,已全部變成State
    /// </summary>
    public void HandleUpdate()
    {
        StateMachine.Execute();
    }


    /// <summary>
    /// 切換Pokemon
    /// </summary>
    /// <param name="newPokemon">要切換的學生</param>
    /// <returns></returns>
    public IEnumerator SwitchPokemon(Pokemon newPokemon)
    {
        if (playerUnit.Pokemon.HP > 0)
        {
            yield return dialogBox.TypeDialog($"返來啦{playerUnit.Pokemon.Base.Name},休息備戰打宿儺");
            //切換動畫
            playerUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);
        }

        playerUnit.Setup(newPokemon);

        dialogBox.SetMoveNames(newPokemon.Moves);
        yield return dialogBox.TypeDialog("輪到你打宿儺啦！ " + newPokemon.Base.Name + "！");

    }

    /// <summary>
    /// 派出下一個健康的Pokemon
    /// </summary>
    /// <returns></returns>
    public IEnumerator SendNextTrainerPokemon()
    {
        var nextPokemon = TrainerParty.GetHealthyPokemon();
        enemyUnit.Setup(nextPokemon);
        yield return dialogBox.TypeDialog($"{Trainer.Name}派出{nextPokemon.Base.Name}");
    }

    /// <summary>
    /// 扔精靈球行爲
    /// </summary>
    /// <returns></returns>
    public IEnumerator ThrowPokeball(PokeballItem pokeballItem)
    {
        //不能捕獲他人的Pokemon
        if (IsTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"你不能收服他人的\"夥伴/學生/下屬/etc.\"");
            yield break;
        }

        yield return dialogBox.TypeDialog($"{player.Name}扔出{pokeballItem.Name}!");
        var pokeballObj = Instantiate(pokeballSprite, playerUnit.transform.position - new Vector3(2, 0), Quaternion.identity);
        var pokeball = pokeballObj.gameObject.GetComponent<SpriteRenderer>();
        pokeball.sprite = pokeballItem.Icon;

        //動畫
        //直到動畫完成才執行下一個
        yield return pokeball.transform.DOJump(enemyUnit.transform.position + new Vector3(0, 2), 2f, 1, 1f).WaitForCompletion();

        //進入精靈球動畫
        yield return enemyUnit.PlayCaptureAnimation();
        //掉下
        yield return pokeball.transform.DOMoveY(enemyUnit.transform.position.y - 1.5f, 0.5f).WaitForCompletion();


        int shakeCount = TryToCatchPokemon(enemyUnit.Pokemon, pokeballItem);

        //搖晃
        for (int i = 0; i < Mathf.Min(shakeCount, 3); i++)
        {
            yield return new WaitForSeconds(0.5f);
            yield return pokeball.transform.DOPunchRotation(new Vector3(0, 0, 10f), 0.8f).WaitForCompletion();
        }
        if (shakeCount == 4)
        {
            //收服成功
            yield return dialogBox.TypeDialog($"成功拐騙{enemyUnit.Pokemon.Base.Name}!");
            yield return pokeball.DOFade(0, 1.5f).WaitForCompletion();

            int currentPokemonNumb = PlayerParty.Pokemons.Count;

            PlayerParty.AddPokemon(enemyUnit.Pokemon);
            if(currentPokemonNumb < 6)
                yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name}加入了你的小隊中");
            else
                yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name}加入了你的學生待定區中");

            Destroy(pokeball);
            BattleOver(true);
        }
        else
        {
            //收服失敗
            yield return new WaitForSeconds(1f);
            pokeball.DOFade(0, 0.2f);
            yield return enemyUnit.PlayBreakOutAnimation();

            if (shakeCount < 2)
                yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name}拒絕了你的虛假善意!");
            else
                yield return dialogBox.TypeDialog($"差少少就成功收服了!");

            Destroy(pokeball);


        }
    }


    /// <summary>
    /// 計算收服是否成功
    /// </summary>
    /// <param name="pokemon">學生</param>
    /// <returns>搖晃次數 如果等於四次則成功收服</returns>
    int TryToCatchPokemon(Pokemon pokemon, PokeballItem pokeballItem)
    {
        float a = (3 * pokemon.MaxHp - 2 * pokemon.HP) * pokemon.Base.CatchRate *
            pokeballItem.CatchRateModfier * ConditionDB.GetStatusBonus(pokemon.Status) / (3 * pokemon.MaxHp);
        //大於255時
        if (a >= 255)
            //搖晃四下 成功收服！
            return 4;

        float b = 1048560 / Mathf.Sqrt(Mathf.Sqrt(16711680 / a));

        int shakeCount = 0;
        while (shakeCount < 4)
        {
            if (UnityEngine.Random.Range(0, 65535) >= b)
                break;
            ++shakeCount;
        }

        return shakeCount;
    }


    public BattleDialogBox DialogBox => dialogBox;

    public BattleUnit PlayerUnit => playerUnit;
    public BattleUnit EnemyUnit => enemyUnit;
    public List<MoveBase> DefaultMoveBases => defaultMoveBases;

    public PartyScreen PartyScreen => partyScreen;

    public AudioClip BattleVictoryMusic => battleVictoryMusic;
}
