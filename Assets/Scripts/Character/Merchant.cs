using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Merchant : MonoBehaviour
{

    [SerializeField] List<ItemBase> availableItems;

    public IEnumerator Trade()
    {
        yield return ShopController.i.StarTrading(this);
    }

    /// <summary>
    /// 商人的物品
    /// </summary>
    public List<ItemBase> AvailableItems => availableItems;
}
