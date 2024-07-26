using GDE.GenericSelectionUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PartyScreen : SelectionUI<TextSlot>
{
    [SerializeField] Text messageText;
    PartyMemberUI[] memberSlots;

    List<Pokemon> pokemons;
    PokemonParty party;

    public bool isUsingTm = false;

    //已用SelectionUI,不再需要
    //int selection = 0;

    /// <summary>
    /// 獲得選中的Pokemon 的Instance
    /// </summary>
    public Pokemon SelectedMember => pokemons[selectedItem];

    /// <summary>
    /// 可以從不同的狀態調用Pokemon隊伍屏幕，如 ActionSelection、RunningTurn、AboutTousel
    /// </summary>
    /// //已用SelectionUI,不再需要
    //public BattleState? CalledFrom { get; set; }

    //memberSlots Array中就存了所有subobject中的 PartyMemberUI element，可以在後續的Code中使用
    public void Init()
    {
        //獲得所有children components
        memberSlots = GetComponentsInChildren<PartyMemberUI>(true);
        SetSelectionSettings(SelectionType.Grid, 2);

        party = PokemonParty.GetPlayerParty();

        SetPartyDate();

        party.OnUpdated += SetPartyDate;
    }

    //將pokemons 加入到pokomon隊伍UI Array中
    public void SetPartyDate()
    {
        pokemons = party.Pokemons;
        ClearItems();

        for (int i = 0; i < memberSlots.Length; i++)
        {
            if (i < pokemons.Count)
            {
                memberSlots[i].gameObject.SetActive(true);
                memberSlots[i].Init(pokemons[i]);
            }
            else
                memberSlots[i].gameObject.SetActive(false);
        }

        var textSlots = memberSlots.Select(m => m.GetComponent<TextSlot>());
        SetItems(textSlots.Take(pokemons.Count).ToList());
        //UpdateMemberSelection(selection);

        messageText.text = "五條老師，請選擇學生";
    }

    /// <summary>
    /// 更新隊伍選擇行爲
    /// </summary>
    /// <param name="onSelected"></param>
    /// <param name="onBack"></param>
   //已用SelectionUI,不再需要

    /*
    public void HandleUpdate(Action onSelected, Action onBack)
    {
        var prevSelection = selection;

        if (Input.GetKeyDown(KeyCode.RightArrow))
            ++selection;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            --selection;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            selection += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            selection -= 2;

        selection = Mathf.Clamp(selection, 0, pokemons.Count - 1);

        if (selection != prevSelection)
            UpdateMemberSelection(selection);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            onSelected?.Invoke();
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            onBack?.Invoke();
        }
    }
    */




    //當玩家選中隊伍中的Pokemon時
    //已用SelectionUI,不再需要
    /*
    public void UpdateMemberSelection(int selectedMember)
    {
        for (int i = 0; i < pokemons.Count; i++)
        {
            if (i == selectedMember)
                memberSlots[i].SetSelected(true);
            else
                memberSlots[i].SetSelected(false);
        }
    }
    */

    /// <summary>
    /// 控制顯示Pokemon 能否學習技能文本
    /// </summary>
    /// <param name="tmItem"></param>
    public void ShowIfTmIsUsable(TmItem tmItem)
    {
        for (int i = 0; i < pokemons.Count; i++)
        {
            string message = tmItem.CanBeTaught(pokemons[i])?"能學習":"不能學習";
            memberSlots[i].SetMessage(message);
        }
    }

    /// <summary>
    /// 清除文本
    /// </summary>
    public void ClaerMemberSlotMessage()
    {
        for (int i = 0; i < pokemons.Count; i++)
        {
            memberSlots[i].SetMessage("");
        }
    }


    public void SetMessageText(string message)
    {
        messageText.text = message;
    }

}
