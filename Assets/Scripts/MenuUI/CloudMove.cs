using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudMove : MonoBehaviour
{
    public int Start_x;//云朵出现时的坐标
    public int End_x;//云朵消失时的坐标
    private void Update()
    {
        /*Debug.Log(this.GetComponent<RectTransform>().position.x);*/
        transform.Translate(-0.1f, 0, 0);
        if (this.GetComponent<RectTransform>().position.x <= End_x)
        {
            this.GetComponent<RectTransform>().position = new Vector3(Start_x, this.GetComponent<RectTransform>().position.y, 0);
        }
    }
}
