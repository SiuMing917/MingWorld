using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Pokemon", menuName = "Pokemon/Create new pokemon")]
public class PokemonBase : ScriptableObject
{
    [SerializeField] string name;
    //描述
    [TextArea]
    [SerializeField] string description;

    [SerializeField] Sprite frontSprite;
    [SerializeField] Sprite backSprite;

    [SerializeField] PokemonType type1;
    [SerializeField] PokemonType type2;

    //基礎屬性
    [SerializeField] int maxHp;
    [SerializeField] int attack;
    [SerializeField] int defence;
    [SerializeField] int spAttack;
    [SerializeField] int spDefense;
    [SerializeField] int speed;

    //能量
    [SerializeField] int maxEnergy;

    //經驗值
    [SerializeField] int expYield;

    [SerializeField] GrowthRate growthRate;

    [SerializeField] int catchRate = 255;

    //集合存放寶可夢可以學習的類 包括技能和等級
    [SerializeField] List<LearnableMove> learnableMoves;
    //寶可夢可以從  物品技能 中學習的集合
    [SerializeField] List<MoveBase> learnableByItems;
    /// <summary>
    /// 進化Target List
    /// </summary>
    [SerializeField] List<Evolution> evolutions;


    public static int MaxNumOfMoves { get; set; } = 4;

    /// <summary>
    /// 計算需要升級的經驗值量
    /// </summary>
    /// <param name="level"></param>
    /// <returns></returns>
    public int GetExpForLevel(int level)
    {
        int levelrange = level * level * level;

        if (growthRate == GrowthRate.Fast)
        {
            return 4 * levelrange / 5;
        }
        else if (growthRate == GrowthRate.MediumFast)
        {
            return levelrange;
        }
        return -1;
    }


    public string Name
    {
        get { return name; }
    }
    public string Description
    {
        get { return description; }
    }
    public Sprite FrontSprite
    {
        get { return frontSprite; }
    }
    public Sprite BackSprite
    {
        get { return backSprite; }
    }
    public PokemonType Type1
    {
        get { return type1; }
    }
    public PokemonType Type2
    {
        get { return type2; }
    }
    public int MaxHp
    {
        get { return maxHp; }
    }
    public int Defence
    {
        get { return defence; }
    }
    public int Attack
    {
        get { return attack; }
    }
    public int SpAttack
    {
        get { return spAttack; }
    }
    public int SpDefence
    {
        get { return spDefense; }
    }
    public int Speed
    {
        get { return speed; }
    }

    public int MaxEnergy
    {
        get { return maxEnergy; }
    }


    public List<LearnableMove> LearnableMoves
    {
        get
        {
            return learnableMoves;
        }
    }

    public List<MoveBase> LearnableByItems => learnableByItems;

    /// <summary>
    /// 捕获率 默认255
    /// </summary>
    public int CatchRate
    {
        get
        {
            return catchRate;
        }
    }

    /// <summary>
    /// 可獲得經驗
    /// </summary>
    public int ExpYield
    {
        get
        {
            return expYield;
        }

    }

    /// <summary>
    /// 成長速率
    /// </summary>
    public GrowthRate GrowthRate
    {
        get
        {
            return growthRate;
        }
    }

    public List<Evolution> Evolutions => evolutions;

}

/// <summary>
/// 可以學習的技能
/// </summary>
[System.Serializable]
public class LearnableMove
{
    //可以學習的技能
    [SerializeField] MoveBase moveBase;
    //可以學習的等級
    [SerializeField] int level;

    public MoveBase MoveBase
    {
        get
        {
            return moveBase;
        }

        set
        {
            moveBase = value;
        }
    }

    public int Level
    {
        get
        {
            return level;
        }

        set
        {
            level = value;
        }
    }

}
[System.Serializable]
public class Evolution
{
    [SerializeField] PokemonBase evolvesInto;
    [SerializeField] int requiredLevel;
    [SerializeField] EvolutionItem requiredItem;

    public PokemonBase EvolvesInto => evolvesInto;
    public int ReqquiredLevel => requiredLevel;
    public EvolutionItem RequiredItem => requiredItem;
}

public enum PokemonType  //Pokemon屬性
{
    無,
    普,
    火,
    水,
    雷,
    風,
    冰,
    鬥,
    毒,
    土,
    木,
    飛,
    陰,
    陽,
    龙
}


public enum GrowthRate
{
    Fast, MediumFast,
}


/// <summary>
/// 寶可夢屬性 提升/降低 類型
/// </summary>
public enum Stat
{
    攻擊,
    防禦,
    魔法,
    魔防,
    速度,

    //這 2 個不是實際統計資料，它們用於提高技能精度
    命中率,
    //闪避率
    閃避率
}

//屬性剋制
public class TypeChart
{
    static float[][] chart =
    {     //                    普   火   水   雷   風   冰   鬥   毒   土   木   飛   陰   陽   龍 

         /*普*/     new float[]{1f,  1f,  1f,  1f,  1f,  1f,0.5f,  1f,  1f,  1f,  1f,  1f,  1f,  2f},
         /*火*/     new float[]{1f,0.5f,0.5f,  1f,  2f,1.5f,  1f,  1f,0.5f,  2f,  1f,  2f,0.5f,0.5f},
         /*水*/     new float[]{1f,  2f,0.5f,  1f,  1f,0.5f,  1f,  1f,  2f,  0f,  1f,0.5f,  2f,0.5f},
         /*雷*/     new float[]{1f,  1f,  2f,0.5f,0.5f,  1f,  1f,  1f,  2f,0.5f,  2f,  2f,0.5f,  1f},
         /*風*/     new float[]{1f,  0f,  1f,  2f,0.5f,  1f,  1f,  1f,  1f,  2f,  2f,  1f,  1f,0.5f},
         /*冰*/     new float[]{1f,  2f,  2f,  1f,0.5f,0.5f,0.5f,  1f,  1f,  2f,  2f,0.5f,  1f,  2f},
         /*鬥*/     new float[]{2f,  1f,  1f,  1f,  1f,  2f,  1f,0.5f,  1f,  1f,0.5f,  0f,  2f,  1f},
         /*毒*/     new float[]{1f,0.5f,  1f,  1f,  1f,  1f,  1f,  0f,  1f,  2f,  1f,  1f,  1f,  1f},
         /*土*/     new float[]{1f,  2f,  2f,  2f,  1f,  1f,  1f,  2f,  1f,  0f,  0f,  1f,  1f,  0f},
         /*木*/     new float[]{1f,0.5f,  2f,  2f,0.5f,  1f,  1f,0.5f,  1f,0.5f,0.5f,  1f,  2f,  1f},
         /*飛*/     new float[]{1f,  1f,  1f,  1f,  2f,0.5f,  2f,  1f,  1f,  2f,0.5f,  1f,  1f,  1f},
         /*陰*/     new float[]{1f,  2f,0.5f,  2f,  1f,0.5f,  1f,  1f,  1f,  1f,  1f,  0f,  2f,  1f},
         /*陽*/     new float[]{1f,0.5f,  2f,0.5f,  1f,  1f,0.5f,  1f,  1f,  2f,  2f,  2f,  0f,  1f},
         /*龍*/     new float[]{1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  2f},
    };

    /// <summary>
    /// Return剋制倍率
    /// </summary>
    /// <param name="attacType">攻擊的技能類型</param>
    /// <param name="defenseType">被攻擊的寶可夢類型</param>
    /// <returns></returns>
    public static float GetEffectiveness(PokemonType attacType, PokemonType defenseType)
    {
        if (attacType == PokemonType.無 || defenseType == PokemonType.無)
            return 1;
        int row = (int)attacType - 1;
        int col = (int)defenseType - 1;

        return chart[row][col];
    }

    // public string Gets
}



