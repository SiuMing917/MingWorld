using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HPBar hpBar;
    [SerializeField] Text messageText;
    [SerializeField] Image pokemonImage;

    Pokemon _pokemon;

    //public Text MessageText => messageText;

    public void Init(Pokemon pokemon)
    {
        _pokemon = pokemon;
        UpdateData();
        //Initialize為Empty
        SetMessage("");
        _pokemon.OnHPChanged += UpdateData;
    }

    void UpdateData()
    {
        nameText.text = _pokemon.Base.Name;
        levelText.text = "Lvl " + _pokemon.Level;
        hpBar.SetHP((float)_pokemon.HP / _pokemon.MaxHp);
        pokemonImage.sprite = _pokemon.Base.FrontSprite;
    }




    //改變顔色
    public void SetSelected(bool seleted)
    {
        if (seleted)
            nameText.color = GlobalSettings.i.HighlightedColor;
        else
            nameText.color = Color.black;
    }

    /// <summary>
    /// 顯示是否能學習技能
    /// </summary>
    /// <param name="message"></param>
    public void SetMessage(string message)
    {
        messageText.text = message;
    }
}
