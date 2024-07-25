using GDE.GenericSelectionUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MoveToForgetSelectionUI : SelectionUI<TextSlot>
{
    [SerializeField] List<Text> moveTexts;


    /// <summary>
    /// Initialize選擇遺忘 更換技能 UI
    /// </summary>
    /// <param name="currentMoves">當前有的技能</param>
    /// <param name="newMove">可學習的技能</param>
    public void SetMoveDate(List<MoveBase> currentMoves, MoveBase newMove)
    {
        for (int i = 0; i < currentMoves.Count; i++)
        {
            moveTexts[i].text = currentMoves[i].Name;
        }

        moveTexts[currentMoves.Count].text = newMove.Name;

        SetItems(moveTexts.Select(m => m.GetComponent<TextSlot>()).ToList());
    }

    #region
    /*
    /// <summary>
    /// 技能更換交互
    /// </summary>
    public void HandleMoveSelection(Action<int> onSelected)
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
            ++currentSelection;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            --currentSelection;

        UpdateMoveSelection(currentSelection);

        if (Input.GetKeyDown(KeyCode.Z))
            onSelected?.Invoke(currentSelection);


    }


    /// <summary>
    /// 選中變色
    /// </summary>
    /// <param name="selecton"></param>
    public void UpdateMoveSelection(int selecton)
    {
        for (int i = 0; i < PokemonBase.MaxNumOfMoves + 1; i++)
        {
            if (i == selecton)
            {
                moveSelectionText[i].color = highlightedColor;
            }
            else
                moveSelectionText[i].color = Color.black;
        }
    }
    */
    #endregion

}
