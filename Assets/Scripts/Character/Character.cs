using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{

    public float moveSpeed;


    public bool IsMoving { get; private set; }

    public float OffsetY { get; private set; } = 0.3f;

    CharacterAnimator animator;

    private void Awake()
    {
        //獲取當前游戲對象Element的方法，可以通過直接調用它來訪問游戲對象的Element和進行Value調整
        animator = GetComponent<CharacterAnimator>();

        SetPositionAndSnapToTile(transform.position);
    }

    /// <summary>
    /// 設置玩家的位置改變時 重新調整
    /// </summary>
    /// <param name="pos">改變的位置</param>
    public void SetPositionAndSnapToTile(Vector2 pos)
    {
        pos.x = Mathf.Floor(pos.x) + 0.5f;
        pos.y = Mathf.Floor(pos.y) + 0.5f + OffsetY;

        //調整後的位置
        transform.position = pos;
    }

    /// <summary>
    /// 移動行爲
    /// </summary>
    /// <param name="moveVec">移動位置</param>
    /// <param name="OnMoveOver">移動結束觸發事件</param>
    /// <returns></returns>
    public IEnumerator Move(Vector2 moveVec, Action OnMoveOver = null, bool checkCollisions = true)
    {
        animator.MoveX = Mathf.Clamp(moveVec.x, -1f, 1f);
        animator.MoveY = Mathf.Clamp(moveVec.y, -1f, 1f);

        var targetPos = transform.position;
        targetPos.x += moveVec.x;
        targetPos.y += moveVec.y;

        //臺階 跳躍
        var ledge = CheckForLedge(targetPos);
        if (ledge != null)
        {
            if (ledge.TryToJump(this, moveVec))
                yield break;
        }

        if (checkCollisions && !IsPathClear(targetPos))
            yield break;

        //離開水，衝浪狀態Set False
        if (animator.IsSurfing && Physics2D.OverlapCircle(targetPos, 0.3f, GameLayers.I.WaterLayer) == null)
            animator.IsSurfing = false;

        IsMoving = true;

        //檢測玩家和移動坐標之間的差異是否是一個非常小的Value
        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            //停止協程 並在下一個更新繼續
            yield return null;
        }
        transform.position = targetPos;

        IsMoving = false;

        OnMoveOver?.Invoke();
    }


    public void HandleUpdate()
    {
        animator.IsMoving = IsMoving;
    }


    /// <summary>
    /// 判定是否存在障礙物
    /// </summary>
    /// <param name="targetPos"></param>
    /// <returns></returns>
    private bool IsPathClear(Vector3 targetPos)
    {
        var diff = targetPos - transform.position;
        var dir = diff.normalized;

        var collisionLayer = GameLayers.I.SolidObjectsLayer | GameLayers.I.InteractableLayer | GameLayers.I.PlayerLayer;
        if (!animator.IsSurfing)
            collisionLayer = collisionLayer | GameLayers.I.WaterLayer;
        if (Physics2D.BoxCast(transform.position + dir, new Vector2(0.2f, 0.2f), 0f, dir, diff.magnitude - 1, collisionLayer) == true)
        {
            return false;
        }
        else
        {
            return true;
        }

    }


    private bool IsWalkable(Vector3 targetPos)
    {
        //當玩家的目標位置檢查圓形範圍内沒有solidObjectsLayer  則不能行走
        if (Physics2D.OverlapCircle(targetPos, 0.2f, GameLayers.I.SolidObjectsLayer | GameLayers.I.InteractableLayer) != null)
        {
            return false;
        }
        return true;
    }


    /// <summary>
    /// 檢測目標移動位置是不是臺階 Return臺階Class Element
    /// </summary>
    /// <param name="targetPos">移动目标位置</param>
    /// <returns></returns>
    Ledge CheckForLedge(Vector3 targetPos)
    {
        var collider = Physics2D.OverlapCircle(targetPos, 0.15f, GameLayers.I.LedgeLayer);

        return collider?.GetComponent<Ledge>();
    }



    /// <summary>
    /// 朝向目標位置
    /// </summary>
    /// <param name="targertPos">目標位置</param>
    public void LookTowards(Vector3 targertPos)
    {
        var xdiff = Mathf.Floor(targertPos.x) - Mathf.Floor(transform.position.x);
        var ydiff = Mathf.Floor(targertPos.y) - Mathf.Floor(transform.position.y);
        //只能在x或y軸上見到玩家
        if (xdiff == 0 || ydiff == 0)
        {
            animator.MoveX = Mathf.Clamp(xdiff, -1f, 1f);
            animator.MoveY = Mathf.Clamp(ydiff, -1f, 1f);
        }
        else
            Debug.LogError("看不到玩家");
    }

    public CharacterAnimator Animator
    {
        get => animator;
    }

}
