using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleDialogBox : MonoBehaviour
{

    [SerializeField] int lettersPerSecond;

    [SerializeField] Text dialogText;
    [SerializeField] GameObject actionSelector;
    [SerializeField] GameObject moveSelector;
    [SerializeField] GameObject moveDetails;
    [SerializeField] GameObject choiceBox;

    [SerializeField] List<Text> actionTexts;
    [SerializeField] List<Text> moveTexts;

    [SerializeField] Text ppText;
    [SerializeField] Text typeText;
    [SerializeField] Text EnergyText;
    [SerializeField] Text UserEnergyText;

    [SerializeField] Text YesText;
    [SerializeField] Text NoText;

    Color highlightedColor;

    private void Start()
    {
        highlightedColor = GlobalSettings.i.HighlightedColor;
    }

    public void SetDialog(string dialog)
    {
        dialogText.text = dialog;
    }


    //携程 令對話的文字逐個字顯示出來，像動畫一樣。
    /// <summary>
    /// 顯示文字  一字一字
    /// </summary>
    /// <param name="dialog">要顯示的文字内容/param>
    /// <returns></returns>
    public IEnumerator TypeDialog(string dialog)
    {
        dialogText.text = "";
        //ToCharArray()   把String轉換成Array
        foreach (var letter in dialog.ToCharArray())
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f / lettersPerSecond);
        }
        yield return new WaitForSeconds(1f);
    }

    //控制DialogTexUI顯示
    public void EnableDialogText(bool enabled)
    {
        dialogText.enabled = enabled;
    }
    //控制Player行爲 UI的顯示
    public void EnableActonSelector(bool enabled)
    {
        actionSelector.SetActive(enabled);
    }
    //控制顯示内容 Pokemon技能的顯示 和 技能描述
    public void EnableMoveSelector(bool enabled)
    {
        moveSelector.SetActive(enabled);
        moveDetails.SetActive(enabled);
    }

    /// <summary>
    /// 控制戰鬥結束時切换的顯示
    /// </summary>
    /// <param name="enabled"></param>
    public void EnableChoiceBox(bool enabled)
    {
        choiceBox.SetActive(enabled);
    }

    public bool IsChoiceBoxEnabled => choiceBox.activeSelf;

    //當actionTexts Player行爲被選中時
    public void UpdateActionSelection(int selectedAction)
    {
        for (int i = 0; i < actionTexts.Count; i++)
        {
            if (i == selectedAction)
            {
                actionTexts[i].color = highlightedColor;
            }
            else
            {
                actionTexts[i].color = Color.white;
            }
        }
    }
    //當Player選擇技能時
    public void UpdateMoveSelection(int selectedAction, Move move, Pokemon pokemon)
    {
        for (int i = 0; i < moveTexts.Count; i++)
        {
            if (i == selectedAction)
            {
                moveTexts[i].color = highlightedColor;
            }
            else
            {
                moveTexts[i].color = Color.white;
            }
        }
        //PP數值顯示
        ppText.text = $"PP {move.PP}/{move.Base.PP}";

        //技能屬性顯示
        int movepowercheck = move.Base.Power;
        string movecategorycheck = move.Base.Category.ToString();

        if (movecategorycheck.Equals("Physical"))
            movecategorycheck = "物理";
        else if (movecategorycheck.Equals("Special"))
            movecategorycheck = "魔法";
        else
            movecategorycheck = "變化";

        if (movepowercheck <= 0)
            typeText.text = $"【{move.Base.Type.ToString()}】<{movecategorycheck}>  威力 -";
        else
            typeText.text = $"【{move.Base.Type.ToString()}】<{movecategorycheck}>  威力 {move.Base.Power}";

        //能量需要數值顯示
        int EnergyUse = move.Base.Energy;
        if(EnergyUse < 0)
            EnergyText.text = $"能量增加 +{-move.Base.Energy}";
        else
            EnergyText.text = $"能量消耗 -{move.Base.Energy}";

        //玩家Pokemon剩餘能量
        UserEnergyText.text = $"當前能量{pokemon.ENERGY}/{pokemon.MaxEnergy}";

        //PP變色
        if (move.PP == 0)
            ppText.color = Color.red;
        else
            ppText.color = Color.black;

        //能量變色
        if (pokemon.ENERGY < (move.Base.Energy/2))
            EnergyText.color = Color.red;
        else
            EnergyText.color = Color.black;
    }

    /// <summary>
    /// 將Pokemon的技能顯示在UI上
    /// </summary>
    /// <param name="moves"></param>
    public void SetMoveNames(List<Move> moves)
    {
        for (int i = 0; i < moveTexts.Count; i++)
        {
            if (i < moves.Count)
            {
                moveTexts[i].text = moves[i].Base.Name;
            }
            else
            {
                moveTexts[i].text = " - ";
            }
        }

        moveTexts[4].text = "掙扎";
        moveTexts[5].text = "休息";
    }

    public void UpdaChoiceBox(bool yesSelected)
    {
        if (yesSelected)
        {
            YesText.color = highlightedColor;
            NoText.color = Color.black;
        }
        else
        {
            YesText.color = Color.black;
            NoText.color = highlightedColor;
        }
    }

}
