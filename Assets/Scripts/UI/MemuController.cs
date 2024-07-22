using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MemuController : MonoBehaviour
{
    [SerializeField] GameObject memu;

    public event Action<int> onMenuSelected;
    public event Action onBack;

    List<Text> memuItems;
    //記錄第一個位置(選項 預設為0
    int selectedItem = 0;

    private void Awake()
    {
        //使用linq 的tolist方法獲得功能表的所有子物件的Text屬性
        memuItems = memu.GetComponentsInChildren<Text>().ToList();
    }

    public void OpenMemu()
    {
        memu.SetActive(true);
        UpdateItemSelection();
    }

    public void CloseMemu()
    {
        memu.SetActive(false);
    }

    /// <summary>
    /// 功能表選擇行為
    /// </summary>
    public void HandleUpdate()
    {
        int prevSelecton = selectedItem;

        if (Input.GetKeyDown(KeyCode.DownArrow))
            selectedItem++;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            selectedItem--;
        selectedItem = Mathf.Clamp(selectedItem, 0, memuItems.Count - 1);

        if (prevSelecton != selectedItem)
            UpdateItemSelection();

        if (Input.GetKeyDown(KeyCode.Z))
        {
            onMenuSelected?.Invoke(selectedItem);
            CloseMemu();
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            onBack?.Invoke();
            CloseMemu();
        }

    }

    /// <summary>
    /// 更新選中的顏色
    /// </summary>
    public void UpdateItemSelection()
    {
        for (int i = 0; i < memuItems.Count; i++)
        {
            if (selectedItem == i)
                memuItems[i].color = GlobalSettings.i.HighlightedColor;
            else
                memuItems[i].color = GlobalSettings.i.InHighlightedColor;
                //memuItems[i].color = Color.black;
        }
    }
}
