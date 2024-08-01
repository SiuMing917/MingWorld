using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class MenuCanvas : MonoBehaviour
{
    public GameObject FishButton;//��İ�ť
    public GameObject CatButton;//è�İ�ť
    public Animator Animator_StartUI;//StartUI�Ķ���������
    /// <summary>
    /// ����fishbutton
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
    /// ����èͷ
    /// </summary>
    public void OnCatButton()
    {
        CatButton.GetComponent<Animator>().SetBool("Press", true);
        CatButton.transform.GetChild(0).gameObject.GetComponent<Animator>().enabled = false;
        EventHandler.CallTransformPanel();
    }
    /// <summary>
    /// �˳���ť
    /// </summary>
    public void OnExitButton()
    {
        UnityEditor.EditorApplication.isPlaying = false;
        /*Application.Quit();*/
    }
    /// <summary>
    /// ��ʼ��ť
    /// </summary>
    public void OnStartButton()
    {
        Animator_StartUI.SetTrigger("StartUIExit");
        EventHandler.CallStartNewGameAnimation();
    }
}
