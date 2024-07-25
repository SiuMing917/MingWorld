using GDE.GenericSelectionUI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MoveSelectionUI : SelectionUI<TextSlot>
{
    [SerializeField] List<TextSlot> moveTexts;

    [SerializeField] Text ppText;
    [SerializeField] Text typeText;
    [SerializeField] Text EnergyText;
    [SerializeField] Text UserEnergyText;

    private void Start()
    {
        SetSelectionSettings(SelectionType.Grid, 2);
    }

    List<Move> _moves;
    Pokemon _pokemon;

    //可能會用到Battyle System的東西
    Move _usestruggle;
    Move _userest;
    Move _nonemove;

    public void SetMoves(List<Move> moves, Pokemon pokemon, Move struggle, Move rest, Move nonemove)
    {
        _moves = moves;
        _pokemon = pokemon;
        _usestruggle = struggle;
        _userest = rest;
        _nonemove = nonemove;

        selectedItem = 0;

        //SetItems(moveTexts.Take(moves.Count).ToList());
        SetItems(moveTexts.Take(6).ToList());

        for (int i = 0; i < moveTexts.Count; i++)
        {
            if (i < moves.Count)
            {
                moveTexts[i].SetText(moves[i].Base.Name);
            }
            else
            {
                moveTexts[i].SetText(" - ");
            }
        }

        moveTexts[4].SetText("掙扎");
        moveTexts[5].SetText("休息");
    }

    public override void UpdateSelectionInUI()
    {
        base.UpdateSelectionInUI();
        var pokemon = _pokemon;
        var movelist = _moves;
        var move = _nonemove;
        if (selectedItem < pokemon.Moves.Count)
        {
            move = movelist[selectedItem];
        }
        else if (selectedItem == 4)
        {
            move = _usestruggle;
        }
        else if (selectedItem == 5)
        {
            move = _userest;
        }
        else
        {
            move = _nonemove;
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
        if (EnergyUse < 0)
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
        if (pokemon.ENERGY < (move.Base.Energy / 2))
            EnergyText.color = Color.red;
        else
            EnergyText.color = Color.black;
    }
}
