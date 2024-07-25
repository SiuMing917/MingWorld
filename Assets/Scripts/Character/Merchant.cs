using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Merchant : MonoBehaviour
{

    [SerializeField] List<ItemBase> availableItems;

    public IEnumerator Trade()
    {
        ShopMenuState.i.AvailableItems = availableItems;
        yield return GameControlller.Instance.StateMachine.PushAndWait(ShopMenuState.i);
    }

    /// <summary>
    /// �ӤH�����~
    /// </summary>
    public List<ItemBase> AvailableItems => availableItems;
}
