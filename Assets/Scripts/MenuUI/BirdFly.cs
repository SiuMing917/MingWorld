using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdFly : MonoBehaviour
{
    /*private float Timer;//��ʱ��*/

    private void Start()
    {
        StartCoroutine(DestroyBird());
    }
    // Update is called once per frame
    void Update()
    {
        transform.Translate(-0.5f, 0, 0);
        /*Timer+=Time.deltaTime;
        Debug.Log(Timer);*/
    }
    /// <summary>
    /// 5s������bird
    /// </summary>
    /// <returns></returns>
    IEnumerator DestroyBird()
    {
        yield return new WaitForSeconds(5);
        Destroy(gameObject);
    }
}
