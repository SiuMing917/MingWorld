using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

public class SurfableWater : MonoBehaviour, Interactable, IPlayerTriggerable
{
    //���N������
    bool isJumpingToWater = false;

    public bool TriggerRepeatedly => true;

    public IEnumerator Interact(Transform initiator)
    {
        //��������A���Į�
        var animator = initiator.GetComponent<CharacterAnimator>();
        if (animator.IsSurfing || isJumpingToWater)
            yield break;


        yield return DialogManager.Instance.ShowDialogText("�̭Ӧ���n�!");

        var pokemonWithSurf = initiator.GetComponent<PokemonParty>().Pokemons.FirstOrDefault(p => p.Moves.Any(m => m.Base.Name == "�Į�"));

        if (pokemonWithSurf != null)
        {
            int selectedChoice = 0;
            yield return DialogManager.Instance.ShowDialogText($"{pokemonWithSurf.Base.Name}�i�H�U�A�b���W����",
                choices: new List<string> { "�O", "�_" },
                onChoiceSelected: (selecttion) => selectedChoice = selecttion);

            if (selectedChoice == 0)
            {
                yield return DialogManager.Instance.ShowDialogText($"�A�ڵ��F{pokemonWithSurf.Base.Name}�����U�A�ۤv�o�ʤF�i�L�U���j�b���W�樫");

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

    //Ĳ�o���W�԰�
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
