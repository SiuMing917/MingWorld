using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/創建重要物品")]
public class ImportantItem : ItemBase
{
    public override bool Use(Pokemon pokemon)
    {
        return true;
    }
}
