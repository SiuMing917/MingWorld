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

/*�w��SelcetionUI �N���A���A�ݭn
public class MenuController : MonoBehaviour
{
    [SerializeField] GameObject menu;

    public event Action<int> onMenuSelected;
    public event Action onBack;

    List<Text> menuItems;
    //�O���Ĥ@�Ӧ�m(�ﶵ �w�]��0
    int selectedItem = 0;

    private void Awake()
    {
        //�ϥ�linq ��tolist��k��o�\����Ҧ��l����Text�ݩ�
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
    /// �\����ܦ欰
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
    /// ��s�襤���C��
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