using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CutsceneAction
{
    [SerializeField] string name;

    //用於是否等到一個Action完後再執行下一個Action
    [SerializeField] bool waitForCompletion = true;
    public virtual IEnumerator Play()
    {
        yield break;
    }

    public string Name
    {
        get => name;
        set => name = value;
    }

    public bool WaitForCompletion => waitForCompletion;
}
