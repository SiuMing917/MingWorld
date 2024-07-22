using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CutableTree : MonoBehaviour, Interactable
{
    public IEnumerator Interact(Transform initiator)
    {
        yield return DialogManager.Instance.ShowDialogText("前面的樹好似可以砍伐。");

        var pokemonWithCut = initiator.GetComponent<PokemonParty>().Pokemons.FirstOrDefault(p => p.Moves.Any(m => m.Base.Name == "斬擊"));

        if (pokemonWithCut != null)
        {
            int selectedChoice = 0;

            yield return DialogManager.Instance.ShowDialogText($"{pokemonWithCut.Base.Name}應該能斬棵樹落來",
                choices: new List<string> { "是", "否" },
                onChoiceSelected: (selecttion) => selectedChoice = selecttion);

            if (selectedChoice == 0)
            {
                yield return DialogManager.Instance.ShowDialogText($"{pokemonWithCut.Base.Name}斬掉樹了");
                gameObject.SetActive(false);
            }
        }

    }
}
