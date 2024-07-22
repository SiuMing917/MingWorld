using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TarinerFove : MonoBehaviour, IPlayerTriggerable
{
    /// <summary>
    /// 進入 對戰人員可見範圍 觸發戰鬥
    /// </summary>
    /// <param name="player"></param>
    public void OnPlayerTriggered(PlayerController player)
    {
        player.Character.Animator.IsMoving = false;
        GameControlller.Instance.OnEnterTrainerView(GetComponentInParent<TrainerController>());
    }
    public bool TriggerRepeatedly => false;
}
