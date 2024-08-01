using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leaf_Shake : MonoBehaviour
{
    private float Leaf_Timer = 0;//计时器
    public float LeafShakeTime = 5f;//等待一定时间后树叶晃动
    private void Update()
    {
        Leaf_Timer+=Time.deltaTime;
        if (Leaf_Timer >= LeafShakeTime)
        {
            Leaf_Timer -= LeafShakeTime;
            StartCoroutine(LeafShake());
        }
    }
    /// <summary>
    /// 树叶晃动的协程
    /// </summary>
    /// <returns></returns>
    IEnumerator LeafShake()
    {
        this.GetComponent<Animator>().enabled = true;
        yield return new WaitForSeconds(1f);
        this.GetComponent<Animator>().enabled = false;
    }
}
