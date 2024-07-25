using GDEUtils.StateMachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RunTurnState : State<BattleSystem>
{
    public static RunTurnState i { get; private set; }
    private void Awake()
    {
        i = this;
    }

    BattleUnit playerUnit;
    BattleUnit enemyUnit;
    BattleDialogBox dialogBox;
    PartyScreen partyScreen;
    bool isTrainerBattle;
    PokemonParty playerParty;
    PokemonParty trainerParty;

    BattleSystem bs;
    public override void Enter(BattleSystem owner)
    {
        bs = owner;

        playerUnit = bs.PlayerUnit;
        enemyUnit = bs.EnemyUnit;
        dialogBox = bs.DialogBox;
        partyScreen = bs.PartyScreen;
        isTrainerBattle = bs.IsTrainerBattle;
        playerParty = bs.PlayerParty;
        trainerParty = bs.TrainerParty;

        StartCoroutine(RunTurns(bs.SelectedAction));
    }

    /// <summary>
    /// 回合制戰鬥
    /// </summary>
    /// <param name="playerAction">玩家動作</param>
    /// <returns></returns>
    IEnumerator RunTurns(BattleAction playerAction)
    {

        //初始化掙扎&休息
        Move useStruggle = new Move(bs.DefaultMoveBases[0]);
        Move useRest = new Move(bs.DefaultMoveBases[1]);

        int currentMove = bs.SelectedMove;
        Debug.Log($"你的學生有{playerUnit.Pokemon.Moves.Count}個技能,現在使用的是技能{currentMove+1}");
        //選擇戰鬥
        if (playerAction == BattleAction.Move)
        {
            if (playerUnit.Pokemon.Moves.Count > currentMove)
                playerUnit.Pokemon.CurrentMove = playerUnit.Pokemon.Moves[currentMove];
            else if (currentMove == 4)
                playerUnit.Pokemon.CurrentMove = useStruggle;
            else if (currentMove == 5)
                playerUnit.Pokemon.CurrentMove = useRest;
            else
                playerUnit.Pokemon.CurrentMove = useStruggle;

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
            if (bs.IsBattleOver) yield break;

            //Hp大於0 會和繼續
            if (secondPokemon.HP > 0)
            {
                //第二回合
                yield return RunMove(secondUnit, firstUnit, secondUnit.Pokemon.CurrentMove);
                yield return RunAfterTurn(secondUnit);
                if (bs.IsBattleOver) yield break;
            }
        }
        else
        {
            //選擇交換Pokemon
            if (playerAction == BattleAction.SwitchPokemon)
            {
                yield return bs.SwitchPokemon(bs.SelectedPekomon);
            }

            //選擇道具
            else if (playerAction == BattleAction.UseItem)
            {
                if(bs.SelectedItem is PokeballItem)
                {
                    //如果用的是精靈球
                    yield return bs.ThrowPokeball(bs.SelectedItem as PokeballItem);

                    //如果戰鬥結束
                    if (bs.IsBattleOver) 
                        yield break;
                }
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
            if (bs.IsBattleOver) yield break;
        }
        //戰鬥未結束 回到選擇行動Code
        if (!bs.IsBattleOver)
            bs.StateMachine.ChangeState(ActionSelectionState.i);
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
        if(!bs.DefaultMoveBases.Contains(move.Base))
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
            if (effects.SelfEffect)
                source.SetStatus(effects.Status);
            else
                target.SetStatus(effects.Status);
        }

        //不穩定狀態 混亂/異常類 等
        if (effects.VolatileStatus != ConditionID.無)
        {
            if (effects.SelfEffect)
                source.SetVolatileStatus(effects.VolatileStatus);
            else
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
        if (bs.IsBattleOver) yield break;

        //燒傷或中毒在回合結束後結算。
        sourceUnit.Pokemon.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.Pokemon);

        yield return sourceUnit.Hud.WaitForHPUpdate();

        if (sourceUnit.Pokemon.HP <= 0)
        {
            yield return HandlePokemonFainted(sourceUnit);
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
                AudioManager.i.PlayMusic(bs.BattleVictoryMusic);


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
                        yield return dialogBox.TypeDialog($"請選擇一個技能忘記");

                        //選擇忘記一個技能，學習新技能
                        MoveToForgetState.i.NewMove = learnMove.MoveBase;
                        MoveToForgetState.i.CurrentMoves = playerUnit.Pokemon.Moves.Select(m => m.Base).ToList();
                        yield return GameControlller.Instance.StateMachine.PushAndWait(MoveToForgetState.i);

                        var moveIndex = MoveToForgetState.i.Selection;
                        if (moveIndex == PokemonBase.MaxNumOfMoves || moveIndex == -1)
                        {
                            //不學習新技能
                            yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name}沒有學習{learnMove.MoveBase.Name}");
                        }
                        else
                        {
                            //忘記已有的技能，學習新的技能
                            var selectedMove = playerUnit.Pokemon.Moves[moveIndex];
                            yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name}遺忘了{selectedMove.Base.Name}，學習了{learnMove.MoveBase.Name}");

                            //賦值替換 技能 忘記 學習
                            playerUnit.Pokemon.Moves[moveIndex] = new Move(learnMove.MoveBase);
                        }
                    }
                }

                yield return playerUnit.Hud.SetExpSmooth(true);
            }
        }


        yield return CheckForBattleOver(faintedUnit);
    }



    /// <summary>
    /// 當Pokemon GG時的行爲 Check戰鬥結束
    /// </summary>
    /// <param name="faintedUnit">GG的學生</param>
    IEnumerator CheckForBattleOver(BattleUnit faintedUnit)
    {
        if (faintedUnit.IsPlayerUnit)
        {
            var nextPokemon = playerParty.GetHealthyPokemon();
            if (nextPokemon != null)
            {
                yield return GameControlller.Instance.StateMachine.PushAndWait(PartyState.i);
                yield return bs.SwitchPokemon(PartyState.i.SelectedPokemon);
            }
            else
                bs.BattleOver(false);
        }
        else
        {
            if (!isTrainerBattle)
            {
                bs.BattleOver(true);
            }
            else
            {
                var nextPokemon = trainerParty.GetHealthyPokemon();
                if (nextPokemon != null)
                {
                    AboutToUseState.i.NewPokemon = nextPokemon;
                    yield return bs.StateMachine.PushAndWait(AboutToUseState.i);
                }
                else
                    bs.BattleOver(true);
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
    /// 逃跑
    /// </summary>
    /// <returns></returns>
    IEnumerator TryToEscape()
    {
        if (isTrainerBattle)
        {
            yield return dialogBox.TypeDialog("你唔可以在PK時走佬！");
            yield break;
        }
        ++bs.escapeAttempts;
        int playerSpeed = playerUnit.Pokemon.Speed;
        int enemySpeed = enemyUnit.Pokemon.Speed;

        if (enemySpeed < playerSpeed)
        {
            yield return dialogBox.TypeDialog($"成功著草！");
            bs.BattleOver(true);
        }
        else
        {
            float f = (playerSpeed * 128) / enemySpeed + 30 * bs.escapeAttempts;
            f = f % 256;

            if (UnityEngine.Random.Range(0, 256) < f)
            {
                yield return dialogBox.TypeDialog($"成功著草！");
                bs.BattleOver(true);
            }
            else
            {
                yield return dialogBox.TypeDialog($"emmmm....走唔到,點算好...");
                //回到行動狀態中
            }
        }
    }
}
