using GDE.GenericSelectionUI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SummaryScreenUI : SelectionUI<TextSlot>
{
    [Header("頁面")]
    [SerializeField] Text pageNameText;
    [SerializeField] GameObject skillsPage;
    [SerializeField] GameObject movesPage;
    [SerializeField] GameObject movesDetail;

    [Header("基本信息")]
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] Image image;

    [Header("能力值")]
    [SerializeField] Text hpText;
    [SerializeField] Text energyText;
    [SerializeField] Text attackText;
    [SerializeField] Text defenseText;
    [SerializeField] Text spAttackText;
    [SerializeField] Text spDefenseText;
    [SerializeField] Text speedText;
    [SerializeField] Text expPointsText;
    [SerializeField] Text nextLevelExpText;
    [SerializeField] Transform expBar;

    [Header("技能一覽")]
    [SerializeField] List<Text> moveTypes;
    [SerializeField] List<Text> moveCategorys;
    [SerializeField] List<Text> moveNames;
    [SerializeField] List<Text> movePPs;
    [SerializeField] Text movePower;
    [SerializeField] Text moveAccuracy;
    [SerializeField] Text moveEnergy;
    [SerializeField] Text moveDescription;

    List<TextSlot> moveSlots;

    private void Start()
    {
        moveSlots = moveNames.Select(m => m.GetComponent<TextSlot>()).ToList();
        movesDetail.SetActive(false);
        moveDescription.text = "";
    }

    bool inMoveSelection;

    public bool InMoveSelection 
    {
        get => inMoveSelection;
        set
        {
            inMoveSelection = value;

            if(inMoveSelection)
            {
                movesDetail.SetActive(true);
                SetItems(moveSlots.Take(pokemon.Moves.Count).ToList());
            }
            else
            {
                movesDetail.SetActive(false);
                moveDescription.text = "";
                ClearItems();
            }
        }
    }

    Pokemon pokemon;
    public void SetBasicDetails(Pokemon pokemon)
    {
        this.pokemon = pokemon;

        nameText.text = pokemon.Base.Name;
        levelText.text = "Lv." + pokemon.Level;
        image.sprite = pokemon.Base.FrontSprite;
    }

    public void ShowPage(int pageNum)
    {
        if(pageNum == 0)
        {
            pageNameText.text = "能力一覽";
            //顯示能力
            skillsPage.SetActive(true);
            movesPage.SetActive(false);
            //movesDetail.SetActive(false);

            SetSkills();
        }
        else if(pageNum == 1)
        {
            pageNameText.text = "技能一覽";
            //顯示技能
            skillsPage.SetActive(false);
            movesPage.SetActive(true);
            //movesDetail.SetActive(true);

            SetMoves();
        }
        Debug.Log($"你在PageNum{pageNum}");
    }

    public void SetSkills()
    {
        hpText.text = $"{pokemon.HP}/{pokemon.MaxHp}";
        energyText.text = $"{pokemon.ENERGY}/{pokemon.MaxEnergy}";
        attackText.text = "" + pokemon.Attack;
        defenseText.text = "" + pokemon.Defense;
        spAttackText.text = "" + pokemon.SpAttack;
        spDefenseText.text = "" + pokemon.SpDefense;
        speedText.text = "" + pokemon.Speed;

        expPointsText.text = "" + pokemon.Exp;
        nextLevelExpText.text = "" + (pokemon.Base.GetExpForLevel(pokemon.Level + 1) - pokemon.Exp);
        expBar.localScale = new Vector2(pokemon.GetNormalizedExp(), 1);
    }

    public void SetMoves()
    {
        for(int i =0; i < moveNames.Count; i++)
        {
            if(i < pokemon.Moves.Count)
            {
                var move = pokemon.Moves[i];
                string movecategorycheck = move.Base.Category.ToString();

                if (movecategorycheck.Equals("Physical"))
                    movecategorycheck = "物";
                else if (movecategorycheck.Equals("Special"))
                    movecategorycheck = "魔";
                else
                    movecategorycheck = "變";

                moveTypes[i].text = move.Base.Type.ToString();
                moveCategorys[i].text = movecategorycheck;
                moveNames[i].text = move.Base.Name;
                movePPs[i].text = $"{move.PP}/{move.Base.PP}";
            }
            else
            {
                moveTypes[i].text = "-";
                moveCategorys[i].text = "-";
                moveNames[i].text = "-";
                movePPs[i].text = "-";
                movePower.text = "-";
                moveAccuracy.text = "-";
                moveDescription.text = "-";
                moveEnergy.text = "-";
            }
        }
    }

    public override void HandleUpdate()
    {
        if (InMoveSelection)
            base.HandleUpdate();
    }

    public override void UpdateSelectionInUI()
    {
        for (int i = 0; i < items.Count; i++)
        {
            items[i].OnSelectionCustomChanged(i == selectedItem, GlobalSettings.i.HighlightedColor, FontStyle.Bold, (moveNames[0].fontSize+2));
        }

        var move = pokemon.Moves[selectedItem];

        movePower.text = $"{move.Base.Power}";
        moveAccuracy.text = $"{move.Base.Accuracy}";
        moveDescription.text = move.Base.Description;
        //能量需要數值顯示
        var EnergyUse = move.Base.Energy;
        if (EnergyUse < 0)
            moveEnergy.text = $"+{-move.Base.Energy}";
        else
            moveEnergy.text = $"-{move.Base.Energy}";
    }
}
