using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleHud : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] Text StatusText;
    [SerializeField] HPBar hpBar;

    [SerializeField] GameObject expBar;

    [SerializeField] Color psnColor;
    [SerializeField] Color brnColor;
    [SerializeField] Color slpColor;
    [SerializeField] Color parColor;
    [SerializeField] Color frzColor;

    Pokemon _pokemon;
    Dictionary<ConditionID, Color> statusColor;

    public void SetData(Pokemon pokemon)
    {
        if (_pokemon != null)
        {
            //更換Pokeomon 取消Subscribe事件
            _pokemon.OnStatusChanged -= SetStatusText;
            _pokemon.OnHPChanged -= UpdateHP;
        }

        _pokemon = pokemon;
        nameText.text = pokemon.Base.Name;
        levelText.text = "Lvl " + pokemon.Level;
        hpBar.SetHP((float)pokemon.HP / pokemon.MaxHp);
        SetExp();

        statusColor = new Dictionary<ConditionID, Color>()
        {
            { ConditionID.中毒,psnColor },
            { ConditionID.眼訓,slpColor },
            { ConditionID.封印,frzColor },
            { ConditionID.灼傷,brnColor },
            { ConditionID.麻痹,parColor },
        };

        SetStatusText();

        //當狀態改變時 同時執行 SetStatusText文本狀態
        _pokemon.OnStatusChanged += SetStatusText;
        //當Pokemon的HP變化時 重新更新 UI Bar 的顯示
        _pokemon.OnHPChanged += UpdateHP;


    }

    /// <summary>
    /// UI中顯示狀態的名稱，Set顔色
    /// </summary>
    void SetStatusText()
    {
        if (_pokemon.Status == null)
        {
            StatusText.text = "";
        }
        else
        {
            StatusText.text = _pokemon.Status.Id.ToString();
            StatusText.color = statusColor[_pokemon.Status.Id];
        }
    }


    /// <summary>
    /// Initialize個Level Setting
    /// </summary>
    public void SetLevel()
    {
        levelText.text = "Lvl " + _pokemon.Level;
    }



    /// <summary>
    /// Initialize Exp條Bar
    /// </summary>
    public void SetExp()
    {
        //只有玩家才有exp
        if (expBar == null) return;

        float normalizedExp = _pokemon.GetNormalizedExp();
        expBar.transform.localScale = new Vector3(normalizedExp, 1, 1);

    }



    /// <summary>
    /// Smooth 升級Bar
    /// </summary>
    /// <param name="reset">升级重置</param>
    /// <returns></returns>
    public IEnumerator SetExpSmooth(bool reset = false)
    {
        //只有Player現有Exp
        if (expBar == null) yield break;

        //升級時Reset條Bar
        if (reset)
            expBar.transform.localScale = new Vector3(0, 1, 1);

        float normalizedExp = _pokemon.GetNormalizedExp();
        yield return expBar.transform.DOScaleX(normalizedExp, 1.5f).WaitForCompletion();


    }

    /// <summary>
    /// 封裝 携程後的更新HP公式
    /// </summary>
    public void UpdateHP()
    {
        StartCoroutine(UpdateHPAsync());
    }

    //血量變動時 重新為Pokemon的HP Bar做調整
    public IEnumerator UpdateHPAsync()
    {
        yield return hpBar.SetHPSmooth((float)_pokemon.HP / _pokemon.MaxHp);
    }

    /// <summary>
    /// 判定 HP更新  
    /// </summary>
    /// <returns></returns>
    public IEnumerator WaitForHPUpdate()
    {
        yield return new WaitUntil(() => hpBar.IsUpdating == false);
    }

    /// <summary>
    /// 清空戰鬥中的Subscribe 的事件
    /// </summary>
    public void ClearData()
    {
        if (_pokemon != null)
        {
            //更换Pokemon 取消Subscribe事件
            _pokemon.OnStatusChanged -= SetStatusText;
            _pokemon.OnHPChanged -= UpdateHP;
        }
    }

}
