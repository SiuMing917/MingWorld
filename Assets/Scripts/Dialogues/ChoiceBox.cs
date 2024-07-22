using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChoiceBox : MonoBehaviour
{
    [SerializeField] ChoiceText choiceTextPrefab;

    //判斷等待 可以選擇
    bool choiceSelected = false;

    /// <summary>
    /// 存放 生成的ChoiceText 集合
    /// </summary>
    List<ChoiceText> choiceTexts;

    int currentChoice;

    /// <summary>
    /// 顯示選擇項 清理之前那的選項重新載入
    /// </summary>
    /// <param name="choices"></param>
    /// <returns></returns>
    public IEnumerator ShowChoices(List<string> choices, Action<int> onChoicesSelected)
    {
        choiceSelected = false;
        currentChoice = 0;
        gameObject.SetActive(true);

        //刪除存在的子物件  之前的選項
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        choiceTexts = new List<ChoiceText>();



        //為生成的每一項賦值文字
        foreach (var choice in choices)
        {
            var choiceTextObj = Instantiate(choiceTextPrefab, transform);
            choiceTextObj.TextField.text = choice;
            choiceTexts.Add(choiceTextObj);
        }
        //等待 為 true 才繼續執行下一行
        yield return new WaitUntil(() => choiceSelected == true);

        onChoicesSelected?.Invoke(currentChoice);
        gameObject.SetActive(false);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
            ++currentChoice;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            --currentChoice;

        currentChoice = Mathf.Clamp(currentChoice, 0, choiceTexts.Count - 1);

        for (int i = 0; i < choiceTexts.Count; i++)
        {
            //當當前下標選中下標時 效果變化
            choiceTexts[i].SetSelected(i == currentChoice);
        }

        if (Input.GetKeyDown(KeyCode.Z))
            choiceSelected = true;
    }
}
