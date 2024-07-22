using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Healer : MonoBehaviour
{
    public IEnumerator Heal(Transform player, Dialog dialog)
    {
        int selectedChoice = 0;
        yield return DialogManager.Instance.ShowDialogText("您想要治療學生嗎？",
           choices: new List<string>() { "是", "否" },
            onChoiceSelected: (choiceIndex) => selectedChoice = choiceIndex);
        //(choiceIndex) => selectedChoice = choiceIndex：
        //一個Lambda運算式作為回呼函數，在使用者選擇某個選項後將選項的索引賦值給selectedChoice變數。

        if (selectedChoice == 0)
        {
            //是
            yield return Fader.i.FadeIn(0.5f);

            var playerParty = player.GetComponent<PokemonParty>();
            playerParty.Pokemons.ForEach(p => p.Heal());

            playerParty.PartyUpdated();

            yield return Fader.i.FadeOut(0.5f);
            yield return DialogManager.Instance.ShowDialogText("靚仔，記得下次再來幫襯喔~~~");
        }
        else if (selectedChoice == 1)
        {
            //否
            yield return DialogManager.Instance.ShowDialogText("靚仔，今次拒絕唔緊要，記得下次要幫襯喔~~~");
        }

    }
}
