using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/�Ыح��n���~")]
public class ImportantItem : ItemBase
{
    public override bool Use(Pokemon pokemon)
    {
        return true;
    }
}
