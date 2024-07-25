using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Cutscene : MonoBehaviour, IPlayerTriggerable
{
    [SerializeReference]
    [SerializeField] List<CutsceneAction> actions;

    public bool TriggerRepeatedly => false;

    public IEnumerator Play()
    {
        //GameController�}�lCut Scene State
        GameControlller.Instance.StateMachine.Push(CutsceneState.i);

        foreach(var action in actions)
        {
            if (action.WaitForCompletion)
                yield return action.Play();
            else
                StartCoroutine(action.Play());
        }

        //GameController�}�lFree Roam State
        GameControlller.Instance.StateMachine.Pop();
    }

    public void AddAction(CutsceneAction action)
    {
#if UNITY_EDITOR
        Undo.RegisterCompleteObjectUndo(this, "�K�[�ʧ@��L���ʵe");
#endif
        action.Name = action.GetType().ToString();
        actions.Add(action);
    }

    public void OnPlayerTriggered(PlayerController player)
    {
        player.Character.Animator.IsMoving = false;
        StartCoroutine(Play());
    }
}
