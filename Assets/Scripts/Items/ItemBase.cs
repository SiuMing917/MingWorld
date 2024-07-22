using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBase : ScriptableObject
{
    [SerializeField] string name;
    [SerializeField] string description;
    [SerializeField] Sprite icon;
    [SerializeField] float price;
    [SerializeField] bool isSellable;


    public virtual string Name => name;

    public string Description => description;

    public Sprite Icon => icon;

    public float Price => price;
    public bool IsSellable => isSellable;


    /// <summary>
    /// Parent Class Vitual Bool 使用物品
    /// </summary>
    /// <returns></returns>
    public virtual bool Use(Pokemon pokemon)
    {
        return false;
    }
    /// <summary>
    /// 重複使用
    /// </summary>
    public virtual bool IsReusable => false;

    /// <summary>
    /// 戰鬥中使用
    /// </summary>
    public virtual bool CanUseInBattle => true;
    /// <summary>
    /// 戰鬥后使用
    /// </summary>
    public virtual bool CanUseOutsideBattle => true;
}
