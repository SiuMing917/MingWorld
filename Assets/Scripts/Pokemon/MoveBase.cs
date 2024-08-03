using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Move", menuName = "Pokemon/Create new move")]
public class MoveBase : ScriptableObject
{
    [SerializeField] string name;

    [TextArea]
    [SerializeField] string description;

    [SerializeField] PokemonType type;
    //威力
    [SerializeField] int power;
    //命中率
    [SerializeField] int accuracy;
    //一定命中
    [SerializeField] bool alwaysHit;
    //PP
    [SerializeField] int pp;
    //有限度
    [SerializeField] int priority;
    //能量消耗
    [SerializeField] int energy;
    //暴擊率
    [SerializeField] int criticalrate;
    //類別
    [SerializeField] MoveCategory category;
    //影响
    [SerializeField] MoveEffects effects;
    [SerializeField] List<SecondaryEffects> secondaries;
    [SerializeField] MoveTarget target;

    //隨機相關
    [SerializeField] MoveSpecial movespecial;


    [SerializeField] AudioClip sound;

    public string Name
    {
        get
        {
            return name;
        }
        set
        {
            name = value;
        }
    }

    public string Description
    {
        get
        {
            return description;
        }

        set
        {
            description = value;
        }
    }

    public PokemonType Type
    {
        get
        {
            return type;
        }

        set
        {
            type = value;
        }
    }

    public int Power
    {
        get
        {
            return power;
        }

        set
        {
            power = value;
        }
    }

    public int Accuracy
    {
        get
        {
            return accuracy;
        }

        set
        {
            accuracy = value;
        }
    }

    public int PP
    {
        get
        {
            return pp;
        }

        set
        {
            pp = value;
        }
    }

    public int Energy
    {
        get
        {
            return energy;
        }

        set
        {
            energy = value;
        }
    }

    public int CriticalRate
    {
        get
        {
            return criticalrate;
        }

        set
        {
            criticalrate = value;
        }
    }

    /// <summary>
    /// 攻擊技能類別 物理/魔法
    /// </summary>
    public MoveCategory Category
    { get { return category; } }

    /// <summary>
    /// 效果
    /// </summary>
    public MoveEffects Effects
    {
        get
        {
            return effects;
        }
    }

    public MoveTarget Target
    {
        get
        {
            return target;
        }
    }
    /// <summary>
    /// 必定命中
    /// </summary>
    public bool AlwaysHit
    {
        get
        {
            return alwaysHit;
        }


    }

    /// <summary>
    /// 第二種狀態
    /// </summary>
    public List<SecondaryEffects> Secondaries
    {
        get
        {
            return secondaries;
        }
    }

    public int Priority
    {
        get
        {
            return priority;
        }
    }

    public AudioClip Sound => sound;

    /// <summary>
    /// 隨機
    /// </summary>
    public MoveSpecial MoveSpecial
    {
        get
        {
            return movespecial;
        }
    }

}


/// <summary>
/// 技能效果
/// </summary>
[System.Serializable]
public class MoveEffects
{

    [SerializeField] List<StatBoost> boosts;
    [SerializeField] ConditionID status;
    [SerializeField] ConditionID volatileStatus;
    [SerializeField] bool selfeffect = false;


    /// <summary>
    /// BUFF/DEBUFF
    /// </summary>
    public List<StatBoost> Boosts
    {
        get { return boosts; }
    }

    /// <summary>
    /// 異常狀態
    /// </summary>
    public ConditionID Status
    { get { return status; } }

    public ConditionID VolatileStatus
    {
        get
        {
            return volatileStatus;
        }
    }
    public bool SelfEffect
    { get { return selfeffect; } }
}

[System.Serializable]
/// <summary>
/// 第二種狀態類
/// </summary>
public class SecondaryEffects : MoveEffects
{
    [SerializeField] int change;
    [SerializeField] MoveTarget target;

    /// <summary>
    /// 生效次數
    /// </summary>
    public int Change
    {
        get
        {
            return change;
        }
    }
    /// <summary>
    /// 作用目標
    /// </summary>
    public MoveTarget Target
    {
        get
        {
            return target;
        }
    }
}



/// <summary>
/// BUFF/DEBUFF 類型和等級
/// </summary>
[System.Serializable]
public class StatBoost
{
    /// <summary>
    /// 類型
    /// </summary>
    public Stat stat;
    /// <summary>
    /// 等級
    /// </summary>
    public int boost;
}

public enum MoveCategory
{
    Physical, Special, Status
}
public enum MoveTarget
{
    Foe, Self
}

[System.Serializable]
public class MoveSpecial
{
    [SerializeField] bool withLevel = false;
    [SerializeField] bool withEnergy = false;
    [SerializeField] bool withBoosts = false;
    [SerializeField] bool withStats = false;
    [SerializeField] bool withHp = false;
    [SerializeField] bool withPp = false;
    [SerializeField] bool withMoveType = false;
    [SerializeField] bool withUserType = false;
    [SerializeField] bool withTargetType = false;
    [SerializeField] bool withDamage = false;

    [SerializeField] bool clearBoosts = false;
    [SerializeField] bool copyStatus = false;
    [SerializeField] bool increaseHp = false;
    [SerializeField] bool decreaseHp = false;
    [SerializeField] bool increaseEnergy = false;
    [SerializeField] bool decreaseEnergy = false;
    [SerializeField] bool increasePp = false;
    [SerializeField] bool decreasePp = false;
    [SerializeField] bool copyMove = false;
    [SerializeField] bool makeDeath = false;
    [SerializeField] bool makeProtect = false;



    [SerializeField] int moveValue1 = 0;
    [SerializeField] int moveValue2 = 0;
    [SerializeField] int moveValue3 = 0;

    public bool WithLevel
    {
        get
        {
            return withLevel;
        }
    }
    public bool WithEnergy
    {
        get
        {
            return withEnergy;
        }
    }
    public bool WithBoosts
    {
        get
        {
            return withBoosts;
        }
    }
    public bool WithStats
    {
        get
        {
            return withStats;
        }
    }

    public bool WithHp
    {
        get
        {
            return withHp;
        }
    }
    public bool WithPp
    {
        get
        {
            return withPp;
        }
    }
    public bool WithMoveType
    {
        get
        {
            return withMoveType;
        }
    }
    public bool WithUserType
    {
        get
        {
            return withUserType;
        }
    }
    public bool WithTargetType
    {
        get
        {
            return withTargetType;
        }
    }
    public bool WithDamage
    {
        get
        {
            return withDamage;
        }
    }

    public bool ClearBoosts
    {
        get
        {
            return clearBoosts;
        }
    }

    public bool CopyStatus
    {
        get
        {
            return copyStatus;
        }
    }

    public bool IncreaseHp
    {
        get
        {
            return increaseHp;
        }
    }

    public bool DecreaseHp
    {
        get
        {
            return decreaseHp;
        }
    }

    public bool IncreaseEnergy
    {
        get
        {
            return increaseEnergy;
        }
    }

    public bool DecreaseEnergy
    {
        get
        {
            return decreaseEnergy;
        }
    }

    public bool IncreasePp
    {
        get
        {
            return increasePp;
        }
    }

    public bool DecreasePp
    {
        get
        {
            return decreasePp;
        }
    }

    public bool CopyMove
    {
        get
        {
            return copyMove;
        }
    }

    public bool MakeDeath
    {
        get
        {
            return makeDeath;
        }
    }

    public bool MakeProtect
    {
        get
        {
            return makeProtect;
        }
    }

    public int MoveValue1
    {
        get
        {
            return moveValue1;
        }
    }

    public int MoveValue2
    {
        get
        {
            return moveValue2;
        }
    }

    public int MoveValue3
    {
        get
        {
            return moveValue3;
        }
    }

}
