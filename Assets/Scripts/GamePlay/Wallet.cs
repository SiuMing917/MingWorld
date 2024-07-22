using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wallet : MonoBehaviour, ISavable
{
    [SerializeField] float money;

    public static Wallet i { get; private set; }

    public event Action OnMoneyChanged;

    private void Awake()
    {
        i = this;
    }
    /// <summary>
    /// 收入
    /// </summary>
    /// <param name="amount"></param>
    public void AddMoney(float amount)
    {
        money += amount;
        OnMoneyChanged?.Invoke();
    }
    /// <summary>
    /// 支出
    /// </summary>
    /// <param name="amount"></param>
    public void takeMoney(float amount)
    {
        money -= amount;
        OnMoneyChanged?.Invoke();
    }
    /// <summary>
    /// 是否有錢 買得起野？
    /// </summary>
    /// <param name="amount"></param>
    /// <returns></returns>
    public bool HasMoney(float amount)
    {
        return amount <= money;
    }

    public object CaptureState()
    {
        return money;
    }

    public void RestoreState(object state)
    {
        money = (float)state;
    }

    public float Money => money;
}
