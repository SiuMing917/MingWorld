using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LongGrass : MonoBehaviour, IPlayerTriggerable
{

    /// <summary>
    /// ���ͯ�a�԰��o��
    /// </summary>
    /// <param name="player"></param>
    public void OnPlayerTriggered(PlayerController player)
    {
        if (UnityEngine.Random.Range(1, 101) <= 10)
        {
            {
                player.Character.Animator.IsMoving = false;
                GameControlller.Instance.StartBattle(BattleTrigger.LongGrass);
            }
        }
    }
    public bool TriggerRepeatedly => true;

}
