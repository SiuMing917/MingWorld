using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class Portal : MonoBehaviour, IPlayerTriggerable
{
    [SerializeField] int sceneToLoad = -1;
    //生成座標
    [SerializeField] DestinationIdentifier destinationPortal;
    [SerializeField] Transform spawnPoint;
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
        StartCoroutine(SwitchScense());
    }

    public bool TriggerRepeatedly => false;

    public void Start()
    {
        fader = FindObjectOfType<Fader>();
    }

    IEnumerator SwitchScense()
    {
        //解決指定的傳送點因為場景切換被刪除的情況
        DontDestroyOnLoad(gameObject);

        GameControlller.Instance.PauseGame(true);
        yield return fader.FadeIn(0.5f);

        yield return SceneManager.LoadSceneAsync(sceneToLoad);

        //在當前場景中查找類型為Portal的物件，並返回第一個滿足條件的物件。條件是該物件不等於當前物件，並且其目標門（destinationPortal）與當前物件的目標門相同
        var destPortal = FindObjectsOfType<Portal>().First(x => x != this && x.destinationPortal == this.destinationPortal);

        //設置玩家的座標
        player.Character.SetPositionAndSnapToTile(destPortal.SpawnPoint.position);


        yield return fader.FadeOut(0.5f);
        GameControlller.Instance.PauseGame(false);
        //傳送完 要刪除傳送點  //如果不刪除 下次調用會重新生成嗎？
        Destroy(gameObject);

    }

    public Transform SpawnPoint => spawnPoint;

    public enum DestinationIdentifier
    {
        A, B, C, D, E
    }
}
