using GDE.GenericSelectionUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : SelectionUI<TextSlot>
{
    private void Start()
    {
        SetItems(GetComponentsInChildren<TextSlot>().ToList());
    }
}

/*已用SelcetionUI 代替，不再需要
public class MenuController : MonoBehaviour
{
    [SerializeField] GameObject menu;

    public event Action<int> onMenuSelected;
    public event Action onBack;

    List<Text> menuItems;
    //記錄第一個位置(選項 預設為0
    int selectedItem = 0;

    private void Awake()
    {
        //使用linq 的tolist方法獲得功能表的所有子物件的Text屬性
        menuItems = menu.GetComponentsInChildren<Text>().ToList();
    }

    public void OpenMenu()
    {
        menu.SetActive(true);
        UpdateItemSelection();
    }

    public void CloseMenu()
    {
        menu.SetActive(false);
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
        selectedItem = Mathf.Clamp(selectedItem, 0, menuItems.Count - 1);

        if (prevSelecton != selectedItem)
            UpdateItemSelection();

        if (Input.GetKeyDown(KeyCode.Z))
        {
            onMenuSelected?.Invoke(selectedItem);
            CloseMenu();
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            onBack?.Invoke();
            CloseMenu();
        }

    }

    /// <summary>
    /// 更新選中的顏色
    /// </summary>
    public void UpdateItemSelection()
    {
        for (int i = 0; i < menuItems.Count; i++)
        {
            if (selectedItem == i)
                menuItems[i].color = GlobalSettings.i.HighlightedColor;
            else
                menuItems[i].color = GlobalSettings.i.InHighlightedColor;
            //menuItems[i].color = Color.black;
        }
    }
}
*/