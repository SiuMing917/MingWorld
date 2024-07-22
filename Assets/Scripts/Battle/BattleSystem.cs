using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

//枚舉 戰鬥系統的各種狀態
public enum BattleState { Start, ActionSelection, MoveSelection, RunningTurn, Busy, PartyScreen, Bag, AboutToUse, MoveForget, BattleOver }
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

    [SerializeField] MoveSelectonUI moveSelectonUI;
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
    //集合存放寶可夢可以學習的類 包括技能和等級
    [SerializeField] List<MoveBase> defaultMoveBases;

    //public List<MoveBase> LearnableByItems => learnableByItems;
    /// <summary>
    /// 當前戰鬥狀態
    /// </summary>
    BattleState state;
    //當前選中行爲or技能
    int currentAction;
    int currentMove;

    bool aboutToUseChoice = true;

    //區分戰鬥是否勝利或者失敗  事件用來觸發
    public event Action<bool> OnBattleOver;

    PokemonParty playerParty;
    Pokemon wildPokemon;
    PokemonParty trainerParty;

    bool isTrainerBattle = true;
    PlayerController player;
    TrainerController trainer;

    /// <summary>
    /// 逃跑次數
    /// </summary>
    int escapeAttempts;
    MoveBase moveToLearn;



    /// <summary>
    /// 觸發野生Pokemon戰鬥
    /// </summary>
    /// <param name="playerParty"></param>
    /// <param name="wildPokemon"></param>

    //先整一個Battle Trigger去判斷戰鬥内容
    BattleTrigger battleTrigger;
    public void StartBattle(PokemonParty playerParty, Pokemon wildPokemon, BattleTrigger trigger = BattleTrigger.LongGrass)
    {
        //isTrainerBattle = false;
        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;
        player = playerParty.GetComponent<PlayerController>();
        isTrainerBattle = false;

        battleTrigger = trigger;

        AudioManager.i.PlayMusic(wildBattleMusic);

        StartCoroutine(SetupBattle());
    }

    /// <summary>
    /// 觸發和NPC的戰鬥
    /// </summary>
    /// <param name="playParty">玩家</param>
    /// <param name="pokemonParty">戰鬥員</param>
    public void StartTrainerBattle(PokemonParty playerParty, PokemonParty trainerParty, BattleTrigger trigger = BattleTrigger.LongGrass)
    {
        this.playerParty = playerParty;
        this.trainerParty = trainerParty;

        isTrainerBattle = true;
        player = playerParty.GetComponent<PlayerController>();
        trainer = trainerParty.GetComponent<TrainerController>();

        battleTrigger = trigger;

        AudioManager.i.PlayMusic(trainerBattleMusic);

        StartCoroutine(SetupBattle());
    }

    //戰鬥行爲總協程
    public IEnumerator SetupBattle()
    {
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

        if (!isTrainerBattle)
        {
            //野生Pokemon對戰
            playerUnit.Setup(playerParty.GetHealthyPokemon());
            enemyUnit.Setup(wildPokemon);

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
            trainerImage.sprite = trainer.Sprite;

            yield return dialogBox.TypeDialog($"{trainer.Name}想要同你戰鬥");

            //對手派出第一隻Pokemon
            trainerImage.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(true);
            var trainPokemon = trainerParty.GetHealthyPokemon();
            enemyUnit.Setup(trainPokemon);
            yield return dialogBox.TypeDialog(trainer.Name + "派出" + enemyUnit.Pokemon.Base.Name + "！");

            //玩家派出Pokemon
            playerImage.gameObject.SetActive(false);
            playerUnit.gameObject.SetActive(true);
            var playerPokemon = playerParty.GetHealthyPokemon();
            playerUnit.Setup(playerPokemon);
            yield return dialogBox.TypeDialog("去啦！" + playerUnit.Pokemon.Base.Name + "！");
            dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);
        }

        //逃跑次數
        escapeAttempts = 0;

        partyScreen.Init();
        //行動選擇
        ActionSelection();
    }




    /// <summary>
    /// 觸發戰鬥結束
    /// </summary>
    /// <param name="won"></param>
    void BattleOver(bool won)
    {
        state = BattleState.BattleOver;
        //讓隊伍的所有Pokemon調用狀態Initialize方法
        playerParty.Pokemons.ForEach(p => p.OnBattleOver());
        //通知游戲控制器（Game Controller） 結束戰鬥事件
        playerUnit.Hud.ClearData();
        OnBattleOver(won);
    }


    //戰鬥開場對白
    void ActionSelection()
    {
        state = BattleState.ActionSelection;
        dialogBox.SetDialog("想做咩?");
        dialogBox.EnableActonSelector(true);
    }

    /// <summary>
    /// 打開背包
    /// </summary>
    void OpenBag()
    {
        //Set狀態
        state = BattleState.Bag;
        inventoryUI.gameObject.SetActive(true);
    }

    void MoveSelection()
    {
        state = BattleState.MoveSelection;
        dialogBox.EnableActonSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }

    //打開Pokemon隊伍
    void OpenPartyScreen()
    {
        partyScreen.CalledFrom = state;

        state = BattleState.PartyScreen;
        partyScreen.gameObject.SetActive(true);
    }

    /// <summary>
    /// 將要切換Pokemon
    /// </summary>
    /// <param name="newPokemon">敵方切換的Pokemon</param>
    /// <returns></returns>
    IEnumerator AboutToUse(Pokemon newPokemon)
    {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"{trainer.Name}將派出{newPokemon.Base.Name}，" +
            $"是否要切換學生對抗？");
        state = BattleState.AboutToUse;
        dialogBox.EnableChoiceBox(true);
    }

    /// <summary>
    /// 選擇技能遺忘UI顯示
    /// </summary>
    /// <param name="pokemon"></param>
    /// <param name="newMove"></param>
    /// <returns></returns>
    IEnumerator ChooseMoveToForget(Pokemon pokemon, MoveBase newMove)
    {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"請選擇要遺忘的技能。");
        moveSelectonUI.gameObject.SetActive(true);
        moveSelectonUI.SetMoveDate(pokemon.Moves.Select(x => x.Base).ToList(), newMove);

        //將新技能的Value比moveTolearn 以便調用
        moveToLearn = newMove;

        state = BattleState.MoveForget;
    }


    /// <summary>
    /// 回合行動
    /// </summary>
    /// <param name="playerAction">玩家行爲</param>
    /// <returns></returns>
    IEnumerator RunTurns(BattleAction playerAction)
    {
        state = BattleState.RunningTurn;

        //初始化掙扎和休息
        Move useStruggle = new Move(defaultMoveBases[0]);
        Move useRest = new Move(defaultMoveBases[1]);
        
        //選擇戰鬥
        if (playerAction == BattleAction.Move)
        {
            if (playerUnit.Pokemon.Moves.Count > currentMove)
                playerUnit.Pokemon.CurrentMove = playerUnit.Pokemon.Moves[currentMove];
            else if (currentMove == 4)
                playerUnit.Pokemon.CurrentMove = useStruggle;
            else
                playerUnit.Pokemon.CurrentMove = useRest;

            enemyUnit.Pokemon.CurrentMove = enemyUnit.Pokemon.GetRandomMove();


            int playerMovePriority = playerUnit.Pokemon.CurrentMove.Base.Priority;
            int enemyMovePriority = enemyUnit.Pokemon.CurrentMove.Base.Priority;
            //判斷行動優先級
            bool playerGoesFirst = true;
            if (enemyMovePriority > playerMovePriority)
                playerGoesFirst = false;
            else if (enemyMovePriority == playerMovePriority)
                playerGoesFirst = playerUnit.Pokemon.Speed >= enemyUnit.Pokemon.Speed;

            var firstUnit = (playerGoesFirst) ? playerUnit : enemyUnit;
            var secondUnit = (playerGoesFirst) ? enemyUnit : playerUnit;

            var secondPokemon = secondUnit.Pokemon;
            //第一回合
            yield return RunMove(firstUnit, secondUnit, firstUnit.Pokemon.CurrentMove);
            yield return RunAfterTurn(firstUnit);
            if (state == BattleState.BattleOver) yield break;

            //Hp大於0 會和繼續
            if (secondPokemon.HP > 0)
            {
                //第二回合
                yield return RunMove(secondUnit, firstUnit, secondUnit.Pokemon.CurrentMove);
                yield return RunAfterTurn(secondUnit);
                if (state == BattleState.BattleOver) yield break;
            }
        }
        else
        {
            //選擇交換Pokemon
            if (playerAction == BattleAction.SwitchPokemon)
            {
                var selectedPokemon = partyScreen.SelectedMember;
                state = BattleState.Busy;
                yield return SwitchPokemon(selectedPokemon);
            }

            //選擇道具
            else if (playerAction == BattleAction.UseItem)
            {
                dialogBox.EnableActonSelector(false);
                //yield return ThrowPokeball();
            }
            //逃跑
            else if (playerAction == BattleAction.Run)
            {
                dialogBox.EnableActonSelector(false);
                yield return TryToEscape();
            }

            //交換結束 敵人回合
            var enemyMove = enemyUnit.Pokemon.GetRandomMove();
            yield return RunMove(enemyUnit, playerUnit, enemyMove);
            yield return RunAfterTurn(enemyUnit);
            if (state == BattleState.BattleOver) yield break;
        }
        //戰鬥未結束 回到選擇行動Code
        if (state != BattleState.BattleOver)
            ActionSelection();
    }


    /// <summary>
    /// 執行戰鬥行動
    /// </summary>
    /// <param name="sourceUnit">行動方</param>
    /// <param name="targetUnit">敵方</param>
    /// <param name="move">行動方技能</param>
    /// <returns></returns>
    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        //戰鬥開始前檢測行動方是否能移動 不能移動無法行動
        bool canRunMove = sourceUnit.Pokemon.OnBeforeMove();
        if (!canRunMove)
        {
            yield return ShowStatusChanges(sourceUnit.Pokemon);
            yield return sourceUnit.Hud.WaitForHPUpdate();//解決混亂沒有更新HP
            yield break;
        }
        yield return ShowStatusChanges(sourceUnit.Pokemon);

        //pp-1
        move.PP--;

        //*********使用者的Energy根據技能能量消耗**************
        if (sourceUnit.Pokemon.ENERGY < move.Energy)
            sourceUnit.Pokemon.DecreaseENERGY(sourceUnit.Pokemon.ENERGY);
        else
            sourceUnit.Pokemon.DecreaseENERGY(move.Energy);
        

        yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name}使用了{move.Base.Name},能量變化{-move.Energy},剩餘能量{sourceUnit.Pokemon.ENERGY}");

        //判斷技能是否命中
        if (CheckIfMoveHit(move, sourceUnit.Pokemon, targetUnit.Pokemon))
        {
            //攻擊動畫，音效
            sourceUnit.PlayAttackAnimation();
            AudioManager.i.PlaySfx(move.Base.Sound);

            yield return new WaitForSeconds(1f);

            //受到攻擊動畫，音效
            targetUnit.PlayHitAnimation();
            AudioManager.i.PlaySfx(AudioId.Hit);

            //Check係唔係狀態變化技
            if (move.Base.Category == MoveCategory.Status)
            {
                yield return RunMoveEffects(move.Base.Effects, sourceUnit.Pokemon, targetUnit.Pokemon, move.Base.Target);

            }
            else
            {
                //Pokemon HP如果等於0 就退場
                var damageDetails = targetUnit.Pokemon.TakeDamage(move, sourceUnit.Pokemon);
                //HP變化，UI更新
                yield return targetUnit.Hud.WaitForHPUpdate();
                yield return ShowDamgeDetails(damageDetails);
            }

            //第二種狀態生效條件
            if (move.Base.Secondaries != null && move.Base.Secondaries.Count > 0 && targetUnit.Pokemon.HP > 0)
            {
                foreach (var secondary in move.Base.Secondaries)
                {
                    var rnd = UnityEngine.Random.Range(1, 101);
                    if (rnd <= secondary.Change)
                        yield return RunMoveEffects(secondary, sourceUnit.Pokemon, targetUnit.Pokemon, secondary.Target);
                }
            }
            if (targetUnit.Pokemon.HP <= 0)
            {
                yield return HandlePokemonFainted(targetUnit);
            }
        }
        else
        {
            yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name}攻擊未命中!");
        }


        //回合結束後 結算傷害
        //sourceUnit.Pokemon.OnAfterTurn();  //在戰鬥結束後 兩次持續傷害判定 bug
        yield return ShowStatusChanges(sourceUnit.Pokemon);
        yield return sourceUnit.Hud.UpdateHPAsync();

        if (sourceUnit.Pokemon.HP <= 0)
        {
            yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name}GG了!");
            sourceUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);

            CheckForBattleOver(sourceUnit);
        }

    }

    /// <summary>
    /// 行動 技能影響
    /// </summary>
    /// <param name="effects">效果</param>
    /// <param name="source">攻擊方</param>
    /// <param name="target">攻擊目標</param>
    /// <param name="moveTarget">技能作用目標</param>
    /// <returns></returns>
    IEnumerator RunMoveEffects(MoveEffects effects, Pokemon source, Pokemon target, MoveTarget moveTarget)
    {

        //狀態 BUFF類
        if (effects.Boosts != null)
        {
            //作用目標是否為自己
            if (moveTarget == MoveTarget.Self)
                source.ApplyBoosts(effects.Boosts);
            else
                target.ApplyBoosts(effects.Boosts);
        }

        //狀態 DeBuff異常類
        if (effects.Status != ConditionID.無)
        {
            target.SetStatus(effects.Status);
        }

        //不穩定狀態 混亂/異常類 等
        if (effects.VolatileStatus != ConditionID.無)
        {
            target.SetVolatileStatus(effects.VolatileStatus);
        }


        //顯示 Pokemon 狀態
        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);
    }

    /// <summary>
    /// 行動結束後的回合 
    /// </summary>
    /// <param name="sourceUnit"></param>
    /// <returns></returns>
    IEnumerator RunAfterTurn(BattleUnit sourceUnit)
    {
        //戰鬥結束 不執行協程
        if (state == BattleState.BattleOver) yield break;
        //當狀態 state=BattleState.RunningTurn時 才執行下面的CODE
        yield return new WaitUntil(() => state == BattleState.RunningTurn);

        //燒傷或中毒在回合結束後結算。
        sourceUnit.Pokemon.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.Pokemon);

        yield return sourceUnit.Hud.WaitForHPUpdate();

        if (sourceUnit.Pokemon.HP <= 0)
        {
            StartCoroutine(HandlePokemonFainted(sourceUnit));

            //當狀態 state=BattleState.RunningTurn時 才執行下面的代碼
            yield return new WaitUntil(() => state == BattleState.RunningTurn);

        }
    }
    /// <summary>
    /// 命中判定
    /// </summary>
    /// <param name="move">技能</param>
    /// <param name="source"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    bool CheckIfMoveHit(Move move, Pokemon source, Pokemon target)
    {
        //必定命中
        if (move.Base.AlwaysHit)
            return true;

        float moveAccuracy = move.Base.Accuracy;
        int accuracy = source.StatBoosts[Stat.命中率];
        int evasion = target.StatBoosts[Stat.閃避率];

        var boostValues = new float[] { 1f, 4 / 3f, 5 / 3f, 2f, 7 / 3f, 8 / 3f, 3f };

        if (accuracy > 0)
            moveAccuracy *= boostValues[accuracy];
        else
            moveAccuracy /= boostValues[-accuracy];

        if (evasion > 0)
            moveAccuracy /= boostValues[-evasion];
        else
            moveAccuracy *= boostValues[evasion];

        return UnityEngine.Random.Range(1, 101) <= moveAccuracy;
    }

    /// <summary>
    /// 狀態出隊 顯示在在對話框框上
    /// </summary>
    /// <param name="pokemon"></param>
    /// <returns></returns>
    IEnumerator ShowStatusChanges(Pokemon pokemon)
    {
        while (pokemon.StatusChanges.Count > 0)
        {
            var message = pokemon.StatusChanges.Dequeue();
            yield return dialogBox.TypeDialog(message);
        }
    }



    /// <summary>
    /// 處理Pokemon  GG後的行爲
    /// </summary>
    /// <param name="faintedUnit"></param>
    /// <returns></returns>
    IEnumerator HandlePokemonFainted(BattleUnit faintedUnit)
    {

        yield return dialogBox.TypeDialog($"{faintedUnit.Pokemon.Base.Name}GG了!");
        faintedUnit.PlayFaintAnimation();
        yield return new WaitForSeconds(2f);

        //戰鬥勝利 播放音樂
        if (!faintedUnit.IsPlayerUnit)
        {
            bool battleWon = true;
            if (isTrainerBattle)
                //爲空説明沒有健康的Pokemon,全部GG了
                battleWon = trainerParty.GetHealthyPokemon() == null;
            if (battleWon)
                AudioManager.i.PlayMusic(battleVictoryMusic);


            //獲得經驗
            int expYield = faintedUnit.Pokemon.Base.ExpYield;
            int enemyLevel = faintedUnit.Pokemon.Level;

            //是戰鬥員獲得多
            float trainerBonus = (isTrainerBattle) ? 1.5f : 1f;

            int expGain = Mathf.FloorToInt((expYield * enemyLevel * trainerBonus) / 7);
            playerUnit.Pokemon.Exp += expYield;
            yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name}獲得了{expGain}經驗值");

            //Set Exp 條 BAR
            yield return playerUnit.Hud.SetExpSmooth();
            //Check有冇升級
            while (playerUnit.Pokemon.CheckForLevelUp())
            {
                //重新Set個Level
                playerUnit.Hud.SetLevel();
                yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name}等級提升了{playerUnit.Pokemon.Level}級");

                //學習技能
                var learnMove = playerUnit.Pokemon.GetLearnableMoveAtCurrLevel();

                if (learnMove != null)
                {
                    if (playerUnit.Pokemon.Moves.Count < PokemonBase.MaxNumOfMoves)
                    {
                        playerUnit.Pokemon.LearnMove(learnMove.MoveBase);
                        yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name}學會了{learnMove.MoveBase.Name}");
                        //重新Set技能UI的顯示
                        dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);
                    }
                    else
                    {
                        yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name}想要學習{learnMove.MoveBase.Name}");
                        yield return dialogBox.TypeDialog($"但是技能欄已滿");
                        //選擇忘記一個技能，學習新技能
                        yield return ChooseMoveToForget(playerUnit.Pokemon, learnMove.MoveBase);

                        //直到狀態不爲 技能忘記狀態 結束才執行 下一行的CODE
                        yield return new WaitUntil(() => state != BattleState.MoveForget);

                        yield return new WaitForSeconds(2f);
                    }
                }

                yield return playerUnit.Hud.SetExpSmooth(true);
            }
        }


        CheckForBattleOver(faintedUnit);
    }



    /// <summary>
    /// 當Pokemon GG時的行爲 檢查戰鬥結束
    /// </summary>
    /// <param name="faintedUnit">GG的學生</param>
    void CheckForBattleOver(BattleUnit faintedUnit)
    {
        if (faintedUnit.IsPlayerUnit)
        {
            var nextPokemon = playerParty.GetHealthyPokemon();
            if (nextPokemon != null)
                OpenPartyScreen();
            else
                BattleOver(false);
        }
        else
        {
            if (!isTrainerBattle)
            {
                BattleOver(true);
            }
            else
            {
                var nextPokemon = trainerParty.GetHealthyPokemon();
                if (nextPokemon != null)
                    StartCoroutine(AboutToUse(nextPokemon));
                else
                    BattleOver(true);
            }
        }
    }


    IEnumerator ShowDamgeDetails(DamageDetails damageDetails)
    {
        if (damageDetails.Critical > 1f)
        {
            yield return dialogBox.TypeDialog("擊破防禦!!!");
        }
        if (damageDetails.TypeEffectiveness > 1f)
        {
            yield return dialogBox.TypeDialog("效果拔群!!!");
        }
        else if (damageDetails.TypeEffectiveness < 1f)
        {
            yield return dialogBox.TypeDialog("效果不佳...");
        }
    }


    /// <summary>
    ///戰鬥中的行爲邏輯 公式/約束
    /// </summary>
    public void HandleUpdate()
    {
        if (state == BattleState.ActionSelection)
        {
            HandleActionSelection();
        }
        else if (state == BattleState.MoveSelection)
        {
            HandleMoveSelection();
        }
        else if (state == BattleState.PartyScreen)
        {
            HandlePartySelection();
        }
        else if (state == BattleState.Bag)
        {
            Action onBack = () =>
            {
                inventoryUI.gameObject.SetActive(false);
                state = BattleState.ActionSelection;
            };
            //事件onItemUsed 有參來判定行爲，是否使用了道具？
            Action<ItemBase> onItemUsed = (ItemBase usedItem) =>
            {
                StartCoroutine(OnItemUsed(usedItem));
            };

            inventoryUI.HandleUpdate(onBack, onItemUsed);
        }
        else if (state == BattleState.AboutToUse)
        {
            HandleAboutToUse();
        }
        else if (state == BattleState.MoveForget)
        {
            Action<int> onMoveSelected = (moveIndex) =>
            {
                moveSelectonUI.gameObject.SetActive(false);
                //選擇放棄學習新的技能
                if (moveIndex == PokemonBase.MaxNumOfMoves)
                {
                    //不學習新技能
                    StartCoroutine(dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name}沒有學習{moveToLearn.Name}"));
                }
                else
                {
                    //忘記又有技能，學習新技能
                    var selectedMove = playerUnit.Pokemon.Moves[moveIndex];
                    StartCoroutine(dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name}遺忘{selectedMove.Base.Name}學習了{moveToLearn.Name}"));

                    //技能列表替换 技能 忘記 學習
                    playerUnit.Pokemon.Moves[moveIndex] = new Move(moveToLearn);
                }
                moveToLearn = null;
                state = BattleState.RunningTurn;
            };


            moveSelectonUI.HandleMoveSelection(onMoveSelected);
        }

    }

    /// <summary>
    /// 根據用戶選擇選中 行爲
    /// </summary>
    void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            ++currentAction;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            --currentAction;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            currentAction += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            currentAction -= 2;
        //限定currenAction的範圍
        currentAction = Mathf.Clamp(currentAction, 0, 3);

        //BattleDialogBox Class下 UpdateActionSelection（玩家行爲被選中時）方法
        dialogBox.UpdateActionSelection(currentAction);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            //選中戰鬥
            if (currentAction == 0)
            {
                MoveSelection();
            }
            else if (currentAction == 1)
            {
                //背包
                //Daam一個精靈球
                //StartCoroutine(RunTurns(BattleAction.UseItem));
                OpenBag();
            }
            else if (currentAction == 2)
            {
                //Pokemon
                //記錄之前的狀態
                //prevState = state; 已寫進方法裏了
                OpenPartyScreen();
            }
            else if (currentAction == 3)
            {
                StartCoroutine(RunTurns(BattleAction.Run));
            }
        }
    }

    //根據用戶選擇 選中技能
    void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            ++currentMove;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            --currentMove;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            currentMove += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            currentMove -= 2;

        //初始化掙扎和休息
        Move useStruggle = new Move(defaultMoveBases[0]);
        Move useRest = new Move(defaultMoveBases[1]);

        //檢查是否使用掙扎/休息
        if (currentMove < playerUnit.Pokemon.Moves.Count)
        {
            currentMove = Mathf.Clamp(currentMove, 0, playerUnit.Pokemon.Moves.Count - 1);
            dialogBox.UpdateMoveSelection(currentMove, playerUnit.Pokemon.Moves[currentMove], playerUnit.Pokemon);
        }
        else
        {
            //currentMove唔可以超過5
            currentMove = Mathf.Clamp(currentMove, 0, 5);

            if (currentMove == 4)
                dialogBox.UpdateMoveSelection(currentMove, useStruggle, playerUnit.Pokemon);
            else
                dialogBox.UpdateMoveSelection(currentMove, useRest, playerUnit.Pokemon);
        }
        //確定技能 釋放選擇的技能
        if (Input.GetKeyDown(KeyCode.Z))
        {
            int pokemonEnergy = playerUnit.Pokemon.ENERGY;
            var move = useRest;

            //Check是否使用了掙扎/休息
            if (currentMove < playerUnit.Pokemon.Moves.Count)
                move = playerUnit.Pokemon.Moves[currentMove];
            else if (currentMove == 4)
                move = useStruggle;
            else
                move = useRest;

            //PP為0則不能使用該技能。
            if (move.PP == 0)
            {
                return;
            }

            //Poekmon剩餘能量不足技能需求的一半，則不能使用該技能。
            if (pokemonEnergy < (move.Base.Energy / 2))
            {
                return;
            }

            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(RunTurns(BattleAction.Move));
        }
        //重新選擇Return 玩家行爲
        else if (Input.GetKeyDown(KeyCode.X))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            ActionSelection();
        }
    }

    void HandlePartySelection()
    {

        Action onSelected = () =>
        {
            //獲得隊伍中當前選中的Pokemon
            var selectMember = partyScreen.SelectedMember;
            if (selectMember.HP <= 0)
            {
                partyScreen.SetMessageText("學生無法戰鬥");
                return;
            }
            if (selectMember == playerUnit.Pokemon)
            {
                partyScreen.SetMessageText("當前出場為選中的學生");
                return;
            }

            partyScreen.gameObject.SetActive(false);

            //如果玩家在戰鬥回合中更換Pokemon 則執行RunTurns
            if (partyScreen.CalledFrom == BattleState.ActionSelection)
            {
                //prevState = null;
                StartCoroutine(RunTurns(BattleAction.SwitchPokemon));
            }
            else//Pokemon 暈倒 選擇更換
            {
                state = BattleState.Busy;
                bool isTrainerAboutToUse = partyScreen.CalledFrom == BattleState.AboutToUse;
                StartCoroutine(SwitchPokemon(selectMember, isTrainerAboutToUse));
            }
            partyScreen.CalledFrom = null;
        };


        Action onBack = () =>
        {
            if (playerUnit.Pokemon.HP < 0)
            {
                partyScreen.SetMessageText("請選擇學生");
                return;
            }

            partyScreen.gameObject.SetActive(false);

            //如果是發生戰鬥的一方GG 按X的狀態为null
            if (partyScreen.CalledFrom == BattleState.AboutToUse)
            {
                //prevState = null;
                StartCoroutine(SendNextTrainerPokemon());
            }
            else
                ActionSelection();

            partyScreen.CalledFrom = null;
        };


        partyScreen.HandleUpdate(onSelected, onBack);



    }

    /// <summary>
    /// 是否切換學生？
    /// </summary>
    void HandleAboutToUse()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
            aboutToUseChoice = !aboutToUseChoice;

        dialogBox.UpdaChoiceBox(aboutToUseChoice);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            dialogBox.EnableChoiceBox(false);
            if (aboutToUseChoice == true)
            {
                //當玩家打開隊伍切換Pokemon結束時
                //對手派出Pokemon
                //prevState = BattleState.AboutToUse; 已寫進方法裏
                OpenPartyScreen();
            }
            else
            {
                //沒有 則打開
                StartCoroutine(SendNextTrainerPokemon());
            }
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            dialogBox.EnableChoiceBox(false);
            StartCoroutine(SendNextTrainerPokemon());
        }
    }


    /// <summary>
    /// 切換Pokemon
    /// </summary>
    /// <param name="newPokemon">要切換的學生</param>
    /// <returns></returns>
    IEnumerator SwitchPokemon(Pokemon newPokemon, bool isTrainerAboutToUse = false)
    {
        if (playerUnit.Pokemon.HP > 0)
        {
            yield return dialogBox.TypeDialog($"返來啦！{playerUnit.Pokemon.Base.Name} ，休息下準備打宿儺！");
            //切換動畫
            playerUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);
        }

        playerUnit.Setup(newPokemon);

        dialogBox.SetMoveNames(newPokemon.Moves);
        yield return dialogBox.TypeDialog("輪到你打宿儺啦！ " + newPokemon.Base.Name + "！");


        //當玩家在敵方Pokemon歸零？ 切換Pokemon的時機 Yes/No?
        if (isTrainerAboutToUse)
            StartCoroutine(SendNextTrainerPokemon());
        else
            state = BattleState.RunningTurn;


    }

    /// <summary>
    /// 派出下一個健康的Pokemon
    /// </summary>
    /// <returns></returns>
    IEnumerator SendNextTrainerPokemon()
    {
        state = BattleState.Busy;

        var nextPokemon = trainerParty.GetHealthyPokemon();
        enemyUnit.Setup(nextPokemon);
        yield return dialogBox.TypeDialog($"{trainer.Name}派出{nextPokemon.Base.Name}");

        state = BattleState.RunningTurn;
    }

    /// <summary>
    /// 執行判定物品使用或者扔精靈球行爲 等待該行爲完成。
    /// </summary>
    /// <param name="usedItem"></param>
    /// <returns></returns>
    IEnumerator OnItemUsed(ItemBase usedItem)
    {
        state = BattleState.Busy;
        inventoryUI.gameObject.SetActive(false);

        if (usedItem is PokeballItem)
        {
            yield return ThrowPokeball((PokeballItem)usedItem);
        }
        StartCoroutine(RunTurns(BattleAction.UseItem));
    }

    /// <summary>
    /// 扔精靈球行爲
    /// </summary>
    /// <returns></returns>
    IEnumerator ThrowPokeball(PokeballItem pokeballItem)
    {
        state = BattleState.Busy;
        //不能捕獲他人的Pokemon
        if (isTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"你不能收服他人的\"夥伴/學生/下屬/etc.\"");
            state = BattleState.RunningTurn;
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

            playerParty.AddPokemon(enemyUnit.Pokemon);
            yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name}加入了你的小隊中");

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

            state = BattleState.RunningTurn;

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



    /// <summary>
    /// 逃跑！
    /// </summary>
    /// <returns></returns>
    IEnumerator TryToEscape()
    {
        if (isTrainerBattle)
        {
            yield return dialogBox.TypeDialog("你唔可以在PK時走佬！");
            state = BattleState.RunningTurn;
            yield break;
        }
        ++escapeAttempts;
        int playerSpeed = playerUnit.Pokemon.Speed;
        int enemySpeed = enemyUnit.Pokemon.Speed;

        if (enemySpeed < playerSpeed)
        {
            yield return dialogBox.TypeDialog($"成功著草！");
            BattleOver(true);
        }
        else
        {
            float f = (playerSpeed * 128) / enemySpeed + 30 * escapeAttempts;
            f = f % 256;

            if (UnityEngine.Random.Range(0, 256) < f)
            {
                yield return dialogBox.TypeDialog($"成功著草！");
                BattleOver(true);
            }
            else
            {
                yield return dialogBox.TypeDialog($"emmmm....走唔到,點算好...");
                //回到行動狀態中
                state = BattleState.RunningTurn;
            }
        }
    }
}
