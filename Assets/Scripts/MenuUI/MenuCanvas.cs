using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class MenuCanvas : MonoBehaviour
{
    public GameObject FishButton;//鱼的按钮
    public GameObject CatButton;//猫的按钮
    public Animator Animator_StartUI;//StartUI的动画控制器
    /// <summary>
    /// 按下fishbutton
    /// </summary>
    /// <returns></returns>
    IEnumerator PressFishButton()
    {
        FishButton.GetComponent<Image>().DOFade(0, 1);
        yield return new WaitForSeconds(1);
        CatButton.gameObject.SetActive(true);
    }
    public void OnFishButton()
    {
        StartCoroutine(PressFishButton());
    }
    /// <summary>
    /// 按下猫头
    /// </summary>
    public void OnCatButton()
    {
        CatButton.GetComponent<Animator>().SetBool("Press", true);
        CatButton.transform.GetChild(0).gameObject.GetComponent<Animator>().enabled = false;
        EventHandler.CallTransformPanel();
    }
    /// <summary>
    /// 退出按钮
    /// </summary>
    public void OnExitButton()
    {
        UnityEditor.EditorApplication.isPlaying = false;
        /*Application.Quit();*/
    }
    /// <summary>
    /// 开始按钮
    /// </summary>
    public void OnStartButton()
    {
        Animator_StartUI.SetTrigger("StartUIExit");
        EventHandler.CallStartNewGameAnimation();
    }
}
