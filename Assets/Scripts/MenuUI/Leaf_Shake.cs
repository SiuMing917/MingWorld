using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leaf_Shake : MonoBehaviour
{
    private float Leaf_Timer = 0;//��ʱ��
    public float LeafShakeTime = 5f;//�ȴ�һ��ʱ�����Ҷ�ζ�
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
    /// ��Ҷ�ζ���Э��
    /// </summary>
    /// <returns></returns>
    IEnumerator LeafShake()
    {
        this.GetComponent<Animator>().enabled = true;
        yield return new WaitForSeconds(1f);
        this.GetComponent<Animator>().enabled = false;
    }
}
