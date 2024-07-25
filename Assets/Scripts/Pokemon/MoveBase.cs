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
    //類別
    [SerializeField] MoveCategory category;
    //影响
    [SerializeField] MoveEffects effects;
    [SerializeField] List<SecondaryEffects> secondaries;
    [SerializeField] MoveTarget target;


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


    //改爲 MoveCategory
    //public bool IsSpecial
    //{
    //    get
    //    {
    //        if (type == PokemonType.火 || type == PokemonType.水 || type == PokemonType.氷 || type == PokemonType.Electric电
    //          || type == PokemonType.草 || type == PokemonType.龙)
    //        {
    //            return true;
    //        }
    //        else
    //        { return false; }
    //    }

    //}


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