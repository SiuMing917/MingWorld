using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BattleUnit : MonoBehaviour
{
    //是否是玩家的Unit Pokemon
    [SerializeField] bool isPlayerUnit;

    [SerializeField] BattleHud hud;

    [SerializeField] GameObject expBar;

    public bool IsPlayerUnit
    {
        get { return isPlayerUnit; }
    }
    /// <summary>
    /// 戰鬥時HP Bar的變動
    /// </summary>
    public BattleHud Hud { get { return hud; } }

    public Pokemon Pokemon { get; set; }


    Image image;
    Vector3 orginalPos;
    Color orginalColor;
    private void Awake()
    {
        image = GetComponent<Image>();
        orginalPos = image.transform.localPosition;
        orginalColor = image.color;
    }

    public void Setup(Pokemon pokemon)
    {
        Pokemon = pokemon;
        //如果是玩家的Pokemon用背面圖，否則用正面圖
        if (isPlayerUnit)
        {
            image.sprite = Pokemon.Base.BackSprite;
        }
        else
        {
            image.sprite = Pokemon.Base.FrontSprite;
        }

        hud.gameObject.SetActive(true);
        hud.SetData(pokemon);

        //還原大小
        transform.localScale = new Vector3(1, 1, 1);
        //戰鬥後面恢復Pokemon的顔色
        image.color = orginalColor;
        PlayEnterAnimation();

    }
    public void Clear()
    {
        hud.gameObject.SetActive(false);
    }

    //入場動畫
    public void PlayEnterAnimation()
    {
        if (isPlayerUnit)
            image.transform.localPosition = new Vector3(-500f, orginalPos.y);
        else
            image.transform.localPosition = new Vector3(500f, orginalPos.y);

        image.transform.DOLocalMoveX(orginalPos.x, 1f);
    }

    /// <summary>
    /// 攻擊動畫
    /// </summary>
    public void PlayAttackAnimation()
    {
        var sequence = DOTween.Sequence();
        if (isPlayerUnit)
            sequence.Append(image.transform.DOLocalMoveX(orginalPos.x + 50f, 0.25f));
        else
            sequence.Append(image.transform.DOLocalMoveX(orginalPos.x - 50f, 0.25f));

        sequence.Append(image.transform.DOLocalMoveX(orginalPos.x, 0.2f));
    }

    public void PlayHitAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.DOColor(Color.gray, 0.1f));
        sequence.Append(image.DOColor(orginalColor, 0.1f));
    }

    public void PlayFaintAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.transform.DOLocalMoveY(orginalPos.y - 150, 0.5f));
        sequence.Join(image.DOFade(0f, 0.5f));
    }

    /// <summary>
    /// 進入精靈球動畫
    /// </summary>
    /// <returns></returns>
    public IEnumerator PlayCaptureAnimation()
    {
        var sequence = DOTween.Sequence();
        //透明
        sequence.Append(image.DOFade(0, 0.5f));
        //移動Y
        sequence.Join(image.transform.DOLocalMoveY(orginalPos.y + 50f, 0.5f));
        //縮小
        sequence.Join(image.transform.DOScale(new Vector3(0.3f, 0.3f, 1f), 0.5f));

        yield return sequence.WaitForCompletion();
    }

    /// <summary>
    /// 跳出精靈球
    /// </summary>
    /// <returns></returns>
    public IEnumerator PlayBreakOutAnimation()
    {
        var sequence = DOTween.Sequence();
        //透明
        sequence.Append(image.DOFade(1, 0.5f));
        //移動Y
        sequence.Join(image.transform.DOLocalMoveY(orginalPos.y, 0.5f));
        //縮小
        sequence.Join(image.transform.DOScale(new Vector3(1f, 1f, 1f), 0.5f));

        yield return sequence.WaitForCompletion();
    }

}
