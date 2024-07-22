using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

public class SurfableWater : MonoBehaviour, Interactable, IPlayerTriggerable
{
    //有冇跳落水
    bool isJumpingToWater = false;

    public bool TriggerRepeatedly => true;

    public IEnumerator Interact(Transform initiator)
    {
        //水中不能再次衝浪
        var animator = initiator.GetComponent<CharacterAnimator>();
        if (animator.IsSurfing || isJumpingToWater)
            yield break;


        yield return DialogManager.Instance.ShowDialogText("依個池塘好靚!");

        var pokemonWithSurf = initiator.GetComponent<PokemonParty>().Pokemons.FirstOrDefault(p => p.Moves.Any(m => m.Base.Name == "衝浪"));

        if (pokemonWithSurf != null)
        {
            int selectedChoice = 0;
            yield return DialogManager.Instance.ShowDialogText($"{pokemonWithSurf.Base.Name}可以助你在水上移動",
                choices: new List<string> { "是", "否" },
                onChoiceSelected: (selecttion) => selectedChoice = selecttion);

            if (selectedChoice == 0)
            {
                yield return DialogManager.Instance.ShowDialogText($"你拒絕了{pokemonWithSurf.Base.Name}的幫助，自己發動了【無下限】在水上行走");

                //var animator = initiator.GetComponent<CharacterAnimator>();
                var dir = new Vector3(animator.MoveX, animator.MoveY);
                var targetPos = initiator.position + dir;

                isJumpingToWater = true;
                yield return initiator.DOJump(targetPos, 0.3f, 1, 0.5f).WaitForCompletion();
                isJumpingToWater = false;

                animator.IsSurfing = true;
            }
        }

    }

    //觸發水上戰鬥
    public void OnPlayerTriggered(PlayerController player)
    {
        if (UnityEngine.Random.Range(1, 101) <= 10)
        {
            {
                GameControlller.Instance.StartBattle(BattleTrigger.Water);
            }
        }
    }
}
