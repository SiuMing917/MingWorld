using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// 將玩家傳送到不同的位置，而不切換場景。
public class LocationPortal : MonoBehaviour, IPlayerTriggerable
{
    //生成座標
    [SerializeField] DestinationIdentifier destinationPortal;
    [SerializeField] Transform spawnPoint;
    [SerializeField] bool isOneWay = false;
    [SerializeField] Vector3 MoveToPosition;
    PlayerController player;

    Fader fader;

    /// <summary>
    /// 交互介面的實現 進入傳送門
    /// </summary>
    /// <param name="player"></param>
    public void OnPlayerTriggered(PlayerController player)
    {
        player.Character.Animator.IsMoving = false;
        this.player = player;
        StartCoroutine(Teleport());
    }

    public bool TriggerRepeatedly => false;

    public void Start()
    {
        fader = FindObjectOfType<Fader>();
    }

    IEnumerator Teleport()
    {
        //解決指定的傳送點因為場景切換被刪除的情況
        GameControlller.Instance.PauseGame(true);
        yield return fader.FadeIn(0.5f);

        //設置玩家的座標

        if (isOneWay)
        {
            //單程票
            //yield return fader.FadeOut(0.5f);
            //GameControlller.Instance.PauseGame(false);
            player.Character.SetPositionAndSnapToTile(MoveToPosition);
        }
        else
        {
            //在當前場景中查找類型為Portal的物件，並返回第一個滿足條件的物件。條件是該物件不等於當前物件，並且其目標門（destinationPortal）與當前物件的目標門相同
            var destPortal = FindObjectsOfType<LocationPortal>().First(x => x != this && x.destinationPortal == this.destinationPortal);
            player.Character.SetPositionAndSnapToTile(destPortal.SpawnPoint.position);
        }

        yield return fader.FadeOut(0.5f);
        GameControlller.Instance.PauseGame(false);

    }

    public Transform SpawnPoint => spawnPoint;

    public enum DestinationIdentifier
    {
        A, B, C, D, E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z
    }
}
