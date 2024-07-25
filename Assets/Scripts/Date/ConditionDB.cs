using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionDB : MonoBehaviour
{
    /// <summary>
    /// 獲得在UI中顯示的狀態名稱 key 為condition的ConditionID 賦值
    /// 讓condition能調用出 ConditionDB下的ConditionID名稱
    /// </summary>
    public static void Init()
    {
        foreach (var kvp in Conditions)
        {
            var conditionId = kvp.Key;
            var condition = kvp.Value;

            condition.Id = conditionId;
        }
    }


    //Dictionary 存儲 異常狀態
    public static Dictionary<ConditionID, Condition> Conditions { get; set; } = new Dictionary<ConditionID, Condition>()
    {
        {
        ConditionID.中毒,
            new Condition()
            {
                Name ="中毒",
                StartMessage="中毒了",
                OnAfterTurn=(Pokemon pokemon)=>
                {
                    pokemon.DecreaseHP(pokemon.MaxHp/8);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}因爲中毒受到了傷害");
                }
            }

        },
        {
        ConditionID.灼傷,
            new Condition()
            {
                Name ="灼傷",
                StartMessage="灼傷了",
                OnAfterTurn=(Pokemon pokemon)=>
                {
                    pokemon.DecreaseHP(pokemon.MaxHp/8);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}因爲灼傷受到了傷害");
                }
            }

        },
        {
        ConditionID.麻痹,
            new Condition()
            {
                Name ="麻痹",
                StartMessage="麻痹了",
                OnBeforeMove=(Pokemon pokemon)=>
                {
                   if(Random.Range(0,5) == 1)
                    {
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}麻痹了而無法行動");
                        return false;
                    }
                    return true;
                }
            }

        },
        {
             ConditionID.封印,
            new Condition()
            {
                Name ="封印",
                StartMessage="被封印了",
                OnBeforeMove=(Pokemon pokemon)=>
                {
                    //自愈概率
                   if(Random.Range(1,5)==1)
                   {
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}的封印狀態解除了");
                        return true;
                   }
                   return false;
                }
            }
        },
        {
             ConditionID.眼訓,
            new Condition()
            {
                Name ="睡眠",
                StartMessage="訓著咗",
                OnStart=(Pokemon pokemon)=>
                {
                     //1-3回合
                     pokemon.StatusTime=Random.Range(1,4);
                    Debug.Log($"將會訓{pokemon.StatusTime}個回合");
                },
                OnBeforeMove=(Pokemon pokemon)=>
                {
                    //睡醒 
                    if(pokemon.StatusTime<=0)
                    {
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}醒啦");
                        return true;
                    }
                    //每睡一回合减少一回合
                    pokemon.StatusTime--;
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}訓著咗");
                    return false;
                }
            }
        },

        {
             ConditionID.發癲,
            new Condition()
            {
                Name ="傻咗",
                StartMessage="傻咗",
                OnStart=(Pokemon pokemon)=>
                {
                     //1-4回合
                     pokemon.VolatileStatusTime=Random.Range(1,5);
                     Debug.Log($"將會發癲{pokemon.VolatileStatusTime}個回合");
                },
                OnBeforeMove=(Pokemon pokemon)=>
                {
                    if(pokemon.VolatileStatusTime<=0)
                    {
                        pokemon.CureVolatileStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}傻仔狀態解除了");
                        return true;
                    }
                    //每一回合减少一回合
                   pokemon.VolatileStatusTime--;

                    if(Random.Range(0,3) == 2)
                        return true;

                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}傻咗");
                    pokemon.DecreaseHP(pokemon.MaxHp/8);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}發神經攻擊了自己");
                    return false;
                }
            }
        },

    };


    /// <summary>
    /// 狀態異常捕獲率
    /// </summary>
    /// <param name="condition"></param>
    /// <returns></returns>
    public static float GetStatusBonus(Condition condition)
    {
        if (condition == null)
            return 1f;
        else if (condition.Id == ConditionID.眼訓 || condition.Id == ConditionID.封印)
            return 2f;
        else if (condition.Id == ConditionID.麻痹 || condition.Id == ConditionID.中毒 || condition.Id == ConditionID.灼傷)
            return 1.5f;

        return 1f;
    }
}

public enum ConditionID
{
    無, 中毒, 灼傷, 眼訓, 麻痹, 封印, 發癲
}
