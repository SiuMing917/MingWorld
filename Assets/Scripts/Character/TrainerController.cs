﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainerController : MonoBehaviour, Interactable, ISavable
{
    [SerializeField] string name;
    [SerializeField] Sprite sprite;
    [SerializeField] Dialog dialog;
    [SerializeField] Dialog dialogAfterBattle;
    [SerializeField] GameObject exclamation;
    [SerializeField] GameObject fov;
    [SerializeField] AudioClip trainerAppearsClip;


    bool battleLosst = false;

    Character character;

    public string Name
    {
        get
        {
            return name;
        }

    }

    public Sprite Sprite
    {
        get
        {
            return sprite;
        }

    }


    private void Awake()
    {
        character = GetComponent<Character>();
    }

    private void Start()
    {
        SetFovRotation(character.Animator.DefaultDirection);
    }

    private void Update()
    {
        character.HandleUpdate();
    }

    public IEnumerator Interact(Transform initiator)
    {
        //朝向玩家
        character.LookTowards(initiator.position);

        if (!battleLosst)
        {

            AudioManager.i.PlayMusic(trainerAppearsClip, loop: false);

            //進入戰鬥的行爲
            yield return DialogManager.Instance.ShowDialog(dialog);
            GameControlller.Instance.StartTrainerBattle(this);

        }
        else
        {
            //戰鬥結束對話
            yield return DialogManager.Instance.ShowDialog(dialogAfterBattle);
        }
    }

    /// <summary>
    /// 觸發戰鬥
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public IEnumerator TriggerTrainerBattle(PlayerController player)
    {
        GameControlller.Instance.StateMachine.Push(CutsceneState.i);

        AudioManager.i.PlayMusic(trainerAppearsClip, loop: false);

        //顯示對話框
        exclamation.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        exclamation.SetActive(false);

        //走向玩家
        var diff = player.transform.position - transform.position;
        var moveVec = diff - diff.normalized;
        //Mathf.Round  取整數
        moveVec = new Vector2(Mathf.Round(moveVec.x), Mathf.Round(moveVec.y));

        yield return character.Move(moveVec);

        //進入戰鬥的行爲 顯示對話
        yield return DialogManager.Instance.ShowDialog(dialog);

        GameControlller.Instance.StateMachine.Pop();

        GameControlller.Instance.StartTrainerBattle(this);

    }

    /// <summary>
    /// 戰敗
    /// </summary>
    public void BattleLost()
    {
        battleLosst = true;
        fov.gameObject.SetActive(false);
    }

    public void SetFovRotation(FacingDirection dir)
    {
        float angele = 0f;
        if (dir == FacingDirection.Right)
            angele = 90f;
        else if (dir == FacingDirection.Up)
            angele = 180f;
        else if (dir == FacingDirection.Left)
            angele = 270;

        fov.transform.eulerAngles = new Vector3(0f, 0f, angele);
    }

    public object CaptureState()
    {
        return battleLosst;
    }

    public void RestoreState(object state)
    {
        battleLosst = (bool)state;
        //當戰鬥失敗時 對手的事業要Set Inactive.
        if (battleLosst)
            fov.gameObject.SetActive(false);
    }
}
