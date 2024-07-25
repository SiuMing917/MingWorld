using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Pokemon
{
    [SerializeField] PokemonBase _base;
    [SerializeField] int _level;
    [SerializeField] List<MoveBase> CustomMoveBase;

    public Pokemon(PokemonBase pBase, int pLevel)
    {
        _base = pBase;
        _level = pLevel;

        Init();
    }

    //自定義技能
    //public List<MoveBase> CustomMoves { get; set; }

    //PokemonBase 類型的屬性
    public PokemonBase Base { get { return _base; } }
    public int Level
    {
        get { return _level; }
        set { _level = value; }
    }

    /// <summary>
    /// 獲得的經驗值
    /// </summary>
    public int Exp { get; set; }

    //控制HP
    public int HP { get; set; }

    //控制ENERGY
    public int ENERGY { get; set; }


    //聲明技能類 的集合屬性
    public List<Move> Moves { get; set; }

    public Move CurrentMove { get; set; }

    /// <summary>
    /// DICT對集合屬性  存儲數值變化
    /// </summary>
    public Dictionary<Stat, int> Stats { get; private set; }
    /// <summary>
    /// DICT對集合的屬性 存儲提升降低/等級
    /// </summary>
    public Dictionary<Stat, int> StatBoosts { get; private set; }
    /// <summary>
    /// QUEUE屬性存儲 各種狀態訊息
    /// </summary>
    public Queue<string> StatusChanges { get; private set; }
    /// <summary>
    /// 狀態類型 屬性
    /// </summary>
    public Condition Status { get; private set; }
    public int StatusTime { get; set; }

    /// <summary>
    /// 不穩定狀態/戰鬥結束消失
    /// </summary>
    public Condition VolatileStatus { get; private set; }
    /// <summary>
    /// 戰鬥結束狀態消失 時間/回合
    /// </summary>
    public int VolatileStatusTime { get; set; }

    /// <summary>
    /// 委託返回事件  控制的狀態變化
    /// </summary>
    /// 
    public event System.Action OnStatusChanged;
    /// <summary>
    /// 委託返回事件  控制的血量變化
    /// </summary>
    public event System.Action OnHPChanged;

    /// <summary>
    /// 委託返回事件  控制的能量變化
    /// </summary>
    public event System.Action OnENERGYChanged;

    public void Init()
    {

        //創建一個集合用來存放寶可夢的初始技能
        Moves = new List<Move>();
        //寶可夢升級時添加技能
        int LearnableMovesCount = Base.LearnableMoves.Count;

        /*
        foreach (var move in Base.LearnableMoves)
        {
            if (move.Level <= Level)
            {
                Moves.Add(new Move(move.MoveBase));
            }
            //最大技能數限制
            if (Moves.Count >= PokemonBase.MaxNumOfMoves)
            {
                break;
            }
        }
        */

       

        //如果自定義技能不爲NULL
        if(CustomMoveBase != null && CustomMoveBase.Count > 0)
        {
            //取最新技能
            for (int i = 0; i <= CustomMoveBase.Count - 1; i++)
            {
                //只取頭四個自定義技能
                if (i < 4)
                {
                    Moves.Add(new Move(CustomMoveBase[i]));
                }
                //最大技能數限制
                if (Moves.Count >= PokemonBase.MaxNumOfMoves)
                {
                    break;
                }
            }
        }
        else 
        {
            //取最新技能
            for (int i = LearnableMovesCount - 1; i >= 0; i--)
            {
                var move = Base.LearnableMoves[i];
                if (move.Level <= Level)
                {
                    Moves.Add(new Move(move.MoveBase));
                }
                //最大技能數限制
                if (Moves.Count >= PokemonBase.MaxNumOfMoves)
                {
                    break;
                }
            }
        }

        Exp = Base.GetExpForLevel(Level);

        CalculateStats();
        HP = MaxHp;
        ENERGY = MaxEnergy;



        StatusChanges = new Queue<string>();
        //初始提升等級為0
        ResetStatBoost();
        Status = null;
        VolatileStatus = null;

    }

    /// <summary>
    /// 用構造函數載入 資料
    /// </summary>
    /// <param name="saveData"></param>
    public Pokemon(PokemonSaveData saveData)
    {
        //基礎資料
        _base = PokemonDB.GetObjectByName(saveData.name);

        HP = saveData.hp;
        Level = saveData.level;
        Exp = saveData.exp;
        ENERGY = saveData.energy;

        if (saveData.statusId != null)
            Status = ConditionDB.Conditions[saveData.statusId.Value];
        else
            Status = null;

        //通過構造函數載入資料
        Moves = saveData.moves.Select(s => new Move(s)).ToList();


        CalculateStats();

        StatusChanges = new Queue<string>();
        //初始提升等級為0
        ResetStatBoost();
        VolatileStatus = null;
    }

    /// <summary>
    /// 獲得要保存的Pokemon的資料
    /// </summary>
    /// <returns></returns>
    public PokemonSaveData GetSaveData()
    {
        var saveData = new PokemonSaveData()
        {
            name = Base.name,
            hp = HP,
            energy = ENERGY,

            level = Level,
            exp = Exp,
            statusId = Status?.Id,
            moves = Moves.Select(m => m.GetSaveData()).ToList(),

        };
        return saveData;
    }



    /// <summary>
    /// 計算統計
    /// </summary>
    void CalculateStats()
    {
        Stats = new Dictionary<Stat, int>();
        Stats.Add(Stat.攻擊, Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5);
        Stats.Add(Stat.防禦, Mathf.FloorToInt((Base.Defence * Level) / 100f) + 5);
        Stats.Add(Stat.魔法, Mathf.FloorToInt((Base.SpAttack * Level) / 100f) + 5);
        Stats.Add(Stat.魔防, Mathf.FloorToInt((Base.SpDefence * Level) / 100f) + 5);
        Stats.Add(Stat.速度, Mathf.FloorToInt((Base.Speed * Level) / 100f) + 5);

        int oldMaxHp = MaxHp;

        MaxHp = Mathf.FloorToInt((Base.MaxHp * Level) / 100f) + 10 + Level;
        if (oldMaxHp != 0)
            HP += MaxHp - oldMaxHp;

        int oldMaxEnergy = MaxEnergy;
        MaxEnergy = Mathf.FloorToInt((Base.MaxEnergy * Level) / 100f) + 10 + Level;
        if (oldMaxEnergy != 0)
            ENERGY += MaxEnergy - oldMaxEnergy;
    }

    /// <summary>
    /// 狀態歸零
    /// </summary>
    void ResetStatBoost()
    {
        StatBoosts = new Dictionary<Stat, int>()
        {
            {Stat.攻擊,0},
            {Stat.防禦,0},
            {Stat.魔法,0 },
            {Stat.魔防,0 },
            {Stat.速度,0 },
            {Stat.命中率,0 },
            {Stat.閃避率,0 }
        };
    }

    /// <summary>
    /// 獲得改變後的數值
    /// </summary>
    /// <param name="stat">類型</param>
    /// <returns></returns>
    int GetStat(Stat stat)
    {
        //數值變化值
        int statVal = Stats[stat];

        //等級變化值
        int boost = StatBoosts[stat];
        //等級帶來的倍率變化
        var boostValues = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f };

        if (boost >= 0)
            statVal = Mathf.FloorToInt(statVal * boostValues[boost]);
        else
            statVal = Mathf.FloorToInt(statVal / boostValues[-boost]);


        return statVal;
    }


    /// <summary>
    /// 把List的值存到 字典集合中(賦予效果等級和類型)
    /// </summary>
    /// <param name="statBoosts">list 類型和等級</param>
    public void ApplyBoosts(List<StatBoost> statBoosts)
    {
        foreach (var statBoost in statBoosts)
        {
            var stat = statBoost.stat;
            var boost = statBoost.boost;

            //狀態入隊
            if (boost > 0)
                StatusChanges.Enqueue($"{Base.Name}的{stat}提升了");
            else
                StatusChanges.Enqueue($"{Base.Name}的{stat}降低了");

            StatBoosts[stat] = Mathf.Clamp(StatBoosts[stat] + boost, -6, 6);

            Debug.Log($"{stat}變化了{StatBoosts[stat]}");
        }
    }

    /// <summary>
    /// 檢查是不是能升級
    /// </summary>
    /// <returns></returns>
    public bool CheckForLevelUp()
    {
        if (Exp > Base.GetExpForLevel(_level + 1))
        {
            ++_level;
            CalculateStats();
            return true;
        }

        return false;
    }

    /// <summary>
    /// 升級時獲得可以升級的技能
    /// </summary>
    /// <returns></returns>
    public LearnableMove GetLearnableMoveAtCurrLevel()
    {
        return Base.LearnableMoves.Where(x => x.Level == _level).FirstOrDefault();
    }


    /// <summary>
    /// 學習技能
    /// </summary>
    /// <param name="moveToLearn">學習的技能</param>
    public void LearnMove(MoveBase moveToLearn)
    {
        if (Moves.Count > PokemonBase.MaxNumOfMoves)
            return;
        Moves.Add(new Move(moveToLearn));
    }

    /// <summary>
    /// 是否存在技能
    /// </summary>
    /// <param name="moveToCheck"></param>
    /// <returns></returns>
    public bool HasMove(MoveBase moveToCheck)
    {
        return Moves.Count(m => m.Base == moveToCheck) > 0;

    }
    /// <summary>
    /// 檢測寶可夢進化 等級
    /// </summary>
    /// <returns></returns>
    public Evolution CheckForEvolution()
    {
        //進化清單中找到對應等級的進化物件
        return Base.Evolutions.FirstOrDefault(e => e.ReqquiredLevel <= Level);
    }
    /// <summary>
    /// 檢測Pokemon進化 物品
    /// </summary>
    /// <param name="item">进化物品</param>
    /// <returns></returns>
    public Evolution CheckForEvolution(ItemBase item)
    {
        //進化列表中找到對應 需求物品
        return Base.Evolutions.FirstOrDefault(e => e.RequiredItem == item);
    }
    /// <summary>
    /// 進化改變
    /// </summary>
    /// <param name="evolution"></param>
    public void Evolve(Evolution evolution)
    {
        // 改變寶可夢的種類
        _base = evolution.EvolvesInto;
        //重新計算所有的資料
        CalculateStats();
    }
    /// <summary>
    /// 治療HP-異常狀態
    /// </summary>
    public void Heal()
    {
        HP = MaxHp;
        ENERGY = MaxEnergy;
        foreach (var move in Moves)
        {
            move.PPHeal();
        }
        OnHPChanged?.Invoke();
        OnENERGYChanged?.Invoke();
        CureStatus();
    }

    /// <summary>
    /// 計算顯示在 Scale Bar的Value
    /// </summary>
    /// <returns></returns>
    public float GetNormalizedExp()
    {
        int currLevelExp = Base.GetExpForLevel(Level);
        int nextLevelExp = Base.GetExpForLevel(Level + 1);

        float normalizedExp = (float)(Exp - currLevelExp) / (nextLevelExp - currLevelExp);
        return Mathf.Clamp01(normalizedExp);
    }

    public int Attack
    {
        get { return GetStat(Stat.攻擊); }
    }
    public int Defense
    {
        get { return GetStat(Stat.防禦); }
    }
    public int SpAttack
    {
        get { return GetStat(Stat.魔法); }
    }
    public int SpDefense
    {
        get { return GetStat(Stat.魔防); }
    }
    public int Speed
    {
        get { return GetStat(Stat.速度); }
    }
    public int MaxHp
    {
        get;
        private set;
    }

    public int MaxEnergy
    {
        get;
        private set;
    }

    public DamageDetails TakeDamage(Move move, Pokemon attacker)
    {
        //初始暴擊值
        float critical = 1f;
        if (Random.Range(1f,100f) <= 5f)
        {
            critical = 2f;
        }

        //Pokemon有兩個種屬性
        float type = TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type1) * TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type2);

        var damageDetails = new DamageDetails()
        {
            //這個程式碼片段則是為 damageDetails 這個變數設置了初始值。
            TypeEffectiveness = type,
            Critical = critical,
            Fainted = false
        };
        //判斷技能類型 返回攻擊類型 物/特
        float attack = (move.Base.Category == MoveCategory.Special) ? attacker.SpAttack : attacker.Attack;
        float defense = (move.Base.Category == MoveCategory.Special) ? SpDefense : Defense;

        float modifiers = Random.Range(0.85f, 1f) * type * critical;
        float a = (2 * attacker.Level + 10) / 250f;
        float d = a * move.Base.Power * ((float)attack / defense) + 2;
        int damage = Mathf.FloorToInt(d * modifiers);

        DecreaseHP(damage);

        return damageDetails;
    }

    /// <summary>
    /// 增加HP
    /// </summary>
    /// <param name="amount"></param>
    public void IncreaseHP(int amount)
    {
        HP = Mathf.Clamp(HP + amount, 0, MaxHp);
        OnHPChanged?.Invoke();
    }
    /// <summary>
    /// HP减少
    /// </summary>
    /// <param name="damage"></param>
    public void DecreaseHP(int damage)
    {
        HP = Mathf.Clamp(HP - damage, 0, MaxHp);
        OnHPChanged?.Invoke();
    }

    /// <summary>
    /// 能量增加
    /// </summary>
    /// <param name="EnergyUse"></param>
    public void IncreaseENERGY(int EnergyUse)
    {
        ENERGY = Mathf.Clamp(ENERGY + EnergyUse, 0, MaxEnergy);
        OnENERGYChanged?.Invoke();
    }

    /// <summary>
    /// 能量减少
    /// </summary>
    /// <param name="EnergyUse"></param>
    public void DecreaseENERGY(int EnergyUse)
    {
        ENERGY = Mathf.Clamp(ENERGY - EnergyUse, 0, MaxEnergy);
        OnENERGYChanged?.Invoke();
    }

    /// <summary>
    /// 獲得狀態
    /// </summary>
    /// <param name="conditionID">類型</param>
    public void SetStatus(ConditionID conditionID)
    {
        if (Status != null) return;
        Status = ConditionDB.Conditions[conditionID];
        Status?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{Base.Name}{Status.StartMessage}");
        OnStatusChanged?.Invoke();
    }

    /// <summary>
    /// 自愈 解除異常狀態
    /// </summary>
    public void CureStatus()
    {
        Status = null;
        OnStatusChanged?.Invoke();
    }

    /// <summary>
    /// 不穩定狀態 設置
    /// </summary>
    /// <param name="conditionID"></param>
    public void SetVolatileStatus(ConditionID conditionID)
    {
        if (VolatileStatus != null) return;
        VolatileStatus = ConditionDB.Conditions[conditionID];
        VolatileStatus?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{Base.Name}{VolatileStatus.StartMessage}");

    }

    /// <summary>
    /// 不穩定狀態  自愈/解除
    /// </summary>
    public void CureVolatileStatus()
    {
        VolatileStatus = null;
    }

    public Move GetRandomMove()
    {
        var movesWithpp = Moves.Where(x => x.PP > 0).ToList();

        int r = Random.Range(0, movesWithpp.Count);
        return movesWithpp[r];
    }

    /// <summary>
    /// Pokemon 回合結束 受到傷害  
    /// </summary>
    public void OnAfterTurn()
    {
        Status?.OnAfterTurn?.Invoke(this);
        VolatileStatus?.OnAfterTurn?.Invoke(this);
    }
    /// <summary>
    /// 回合開始判定 異常狀態 無法行動
    /// </summary>
    /// <returns></returns>
    public bool OnBeforeMove()
    {
        //canPerformMove 是否 可以執行移動判斷
        bool canPerformMove = true;
        if (Status?.OnBeforeMove != null)
        {
            if (!Status.OnBeforeMove(this))
                canPerformMove = false;
        }
        if (VolatileStatus?.OnBeforeMove != null)
        {
            if (!VolatileStatus.OnBeforeMove(this))
                canPerformMove = false;
        }
        return canPerformMove;
    }
    /// <summary>
    /// 戰鬥結束重置效果(增幅類)狀態等級 / 不穩定狀態(麻痹等等)
    /// </summary>
    public void OnBattleOver()
    {
        VolatileStatus = null;
        ResetStatBoost();
    }

}
//用於暴擊 克制 消息
public class DamageDetails
{
    public bool Fainted { get; set; }

    public float Critical { get; set; }
    public float TypeEffectiveness { get; set; }
}
[System.Serializable]
public class PokemonSaveData
{
    public string name;
    public int hp;
    public int energy;
    public int level;
    public int exp;
    public ConditionID? statusId;
    public List<MoveSaveData> moves;

}


