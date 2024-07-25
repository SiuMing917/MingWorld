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
    /// Initialize��ܿ�� �󴫧ޯ� UI
    /// </summary>
    /// <param name="currentMoves">��e�����ޯ�</param>
    /// <param name="newMove">�i�ǲߪ��ޯ�</param>
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
    /// �ޯ�󴫥椬
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
    /// �襤�ܦ�
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
