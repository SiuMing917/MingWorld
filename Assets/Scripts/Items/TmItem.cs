using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/創建學習技能TM或HM")]
public class TmItem : ItemBase
{
    [SerializeField] MoveBase move;
    [SerializeField] bool isHM;
    public override bool Use(Pokemon pokemon)
    {
        //學習技能是從庫存UI中處理的，如果它被學習了，那麼返回true
        return pokemon.HasMove(move);
    }

    /// <summary>
    /// Pokemon能否學習該技能
    /// </summary>
    /// <param name="pokemon"></param>
    /// <returns></returns>
    public bool CanBeTaught(Pokemon pokemon)
    {
        return pokemon.Base.LearnableByItems.Contains(move);
    }

    public override string Name => base.Name + $":{move.Name}";

    public override bool CanUseInBattle => false;

    public override bool IsReusable => isHM;

    public MoveBase Move => move;
    public bool IsHM => isHM;
}
