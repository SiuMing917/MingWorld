using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move      //技能類
{
    public MoveBase Base { get; set; }
    public int PP { get; set; }
    public int Energy { get; set; }

    public Move(MoveBase pBase)
    {
        Base = pBase;
        PP = pBase.PP;
        Energy = pBase.Energy;

    }

    /// <summary>
    /// LOAD技能數據
    /// </summary>
    /// <param name="saveData"></param>
    public Move(MoveSaveData saveData)
    {
        //some Bug Base not initialize
        Base = MoveDB.GetObjectByName(saveData.name);
        PP = saveData.pp;
        Energy = saveData.energy;
    }

    /// <summary>
    /// 保存的技能數據
    /// </summary>
    /// <returns></returns>
    public MoveSaveData GetSaveData()
    {
        var saveData = new MoveSaveData
        {
            name = Base.name,
            pp = PP,
            energy = Energy
        };
        return saveData;
    }

    /// <summary>
    /// 增加PP
    /// </summary>
    /// <param name="amount"></param>
    public void IncreasePP(int amount)
    {
        PP = Mathf.Clamp(PP + amount, 0, Base.PP);
    }

    public void IncreaseEnergy(int amount)
    {
        PP = Mathf.Clamp(Energy + amount, 0, Base.Energy);
    }

    public void PPHeal()
    {
        PP = Base.PP;
    }
}
[System.Serializable]
public class MoveSaveData
{
    public string name;
    public int pp;
    public int energy;
}
