using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdFly : MonoBehaviour
{
    /*private float Timer;//计时器*/

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
    /// 5s后销毁bird
    /// </summary>
    /// <returns></returns>
    IEnumerator DestroyBird()
    {
        yield return new WaitForSeconds(5);
        Destroy(gameObject);
    }
}
