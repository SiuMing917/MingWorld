using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class StartGameManager : MonoBehaviour
{
    public float Canvas_FadeTime;
    public GameObject NewGameAnimation;
    public GameObject StartAnimationCanvas;
    public GameObject EssentialObjects;
    public GameControlller gameControlller;
    public GameObject StartUI;

    private PlayableDirector StartDirector;
    private CanvasGroup fadeCanvasGroup;
    private bool IsFade;
    private GameObject StartPanel;
    private GameObject MenuCanvas;
    private void OnEnable()
    {
        EventHandler.TransformPanel += OnTransformPanel;
        EventHandler.StartNewGameAnimation += OnStartNewGameAnimation;
    }

   

    private void OnDisable()
    {
        EventHandler.TransformPanel -= OnTransformPanel;
        EventHandler.StartNewGameAnimation -= OnStartNewGameAnimation;
    }
    private void Start()
    {
        StartDirector=NewGameAnimation.GetComponent<PlayableDirector>();
        fadeCanvasGroup=FindObjectOfType<CanvasGroup>();
        //StartPanel = GameObject.FindGameObjectWithTag("StartPanel");
        //MenuCanvas = GameObject.FindGameObjectWithTag("MenuCanvas");
        /*StartCoroutine(Fade(1));*/
    }
    private void Update()
    {
        if (StartDirector.time >= 49)//10秒后
        {
            StartAnimationCanvas.SetActive(false);
            EssentialObjects.SetActive(true);
            StartUI.SetActive(false);
        }
    }
    /// <summary>
    /// Fade
    /// </summary>
    /// <param name="targetAlpha">醴梓alpha</param>
    /// <returns></returns>
    private IEnumerator Fade(float targetAlpha)
    {
        IsFade=true;
        fadeCanvasGroup.blocksRaycasts=true;
        float speed = Mathf.Abs(fadeCanvasGroup.alpha - targetAlpha) / Canvas_FadeTime;
        while (!Mathf.Approximately(fadeCanvasGroup.alpha, targetAlpha))
        {
            fadeCanvasGroup.alpha = Mathf.MoveTowards(fadeCanvasGroup.alpha, targetAlpha, speed * Time.deltaTime);
            yield return null;
        }
        fadeCanvasGroup.blocksRaycasts = false;
        IsFade = false;
    }
    /// <summary>
    /// Panel轉場
    /// </summary>
    /// <returns></returns>
    private IEnumerator TransformPanel()
    {
        if (!IsFade) yield return Fade(1);
        yield return new WaitForSeconds(5);
        StartPanel.SetActive(false);
        MenuCanvas.transform.GetChild(0).gameObject.SetActive(true);
        if (!IsFade) yield return Fade(0);
    }
    /// <summary>
    /// 開始動畫
    /// </summary>
    /// <returns></returns>
    private IEnumerator StartNewGameAnimation()
    {
        if (!IsFade) yield return Fade(1);
        yield return new WaitForSeconds(1);
        MenuCanvas.SetActive(false);
        
        
        NewGameAnimation.SetActive(true);
        if (!IsFade) yield return Fade(0);
    }
    /// <summary>
    /// 開始Panel
    /// </summary>
    /// <exception cref="System.NotImplementedException"></exception>
    private void OnTransformPanel()
    {
        StartCoroutine(TransformPanel());
    }
    /// <summary>
    /// 開始動畫
    /// </summary>
    /// <exception cref="System.NotImplementedException"></exception>
    private void OnStartNewGameAnimation()
    {
        StartCoroutine(StartNewGameAnimation());
    }
}
