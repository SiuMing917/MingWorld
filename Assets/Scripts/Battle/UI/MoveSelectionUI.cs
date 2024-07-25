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

    //�i��|�Ψ�Battyle System���F��
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

        moveTexts[4].SetText("�ä�");
        moveTexts[5].SetText("��");
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

        //PP�ƭ����
        ppText.text = $"PP {move.PP}/{move.Base.PP}";

        //�ޯ��ݩ����
        int movepowercheck = move.Base.Power;
        string movecategorycheck = move.Base.Category.ToString();

        if (movecategorycheck.Equals("Physical"))
            movecategorycheck = "���z";
        else if (movecategorycheck.Equals("Special"))
            movecategorycheck = "�]�k";
        else
            movecategorycheck = "�ܤ�";

        if (movepowercheck <= 0)
            typeText.text = $"�i{move.Base.Type.ToString()}�j<{movecategorycheck}>  �¤O -";
        else
            typeText.text = $"�i{move.Base.Type.ToString()}�j<{movecategorycheck}>  �¤O {move.Base.Power}";

        //��q�ݭn�ƭ����
        int EnergyUse = move.Base.Energy;
        if (EnergyUse < 0)
            EnergyText.text = $"��q�W�[ +{-move.Base.Energy}";
        else
            EnergyText.text = $"��q���� -{move.Base.Energy}";

        //���aPokemon�Ѿl��q
        UserEnergyText.text = $"��e��q{pokemon.ENERGY}/{pokemon.MaxEnergy}";

        //PP�ܦ�
        if (move.PP == 0)
            ppText.color = Color.red;
        else
            ppText.color = Color.black;

        //��q�ܦ�
        if (pokemon.ENERGY < (move.Base.Energy / 2))
            EnergyText.color = Color.red;
        else
            EnergyText.color = Color.black;
    }
}
