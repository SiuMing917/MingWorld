using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ledge : MonoBehaviour
{
    [SerializeField] int xDir;
    [SerializeField] int yDir;


    private void Awake()
    {
        GetComponent<SpriteRenderer>().enabled = false;
    }

    /// <summary>
    /// 判定角色跳躍  能執行跳躍
    /// </summary>
    /// <param name="character"></param>
    /// <param name="moveDir"></param>
    /// <returns></returns>
    public bool TryToJump(Character character, Vector2 moveDir)
    {
        //當方向正確時
        if (moveDir.x == xDir && moveDir.y == yDir)
        {
            StartCoroutine(Jump(character));
            return true;
        }
        return false;
    }
    /// <summary>
    /// 角色跳躍
    /// </summary>
    /// <param name="character"></param>
    /// <returns></returns>
    IEnumerator Jump(Character character)
    {
        GameControlller.Instance.PauseGame(true);
        character.Animator.IsJumping = true;

        //獲得跳躍的坐標
        var jumpDest = character.transform.position + new Vector3(xDir, yDir) * 2;
        //執行跳躍動畫
        yield return character.transform.transform.DOJump(jumpDest, 0.3f, 1, 0.5f).WaitForCompletion();

        character.Animator.IsJumping = false;
        GameControlller.Instance.PauseGame(false);
    }
}
