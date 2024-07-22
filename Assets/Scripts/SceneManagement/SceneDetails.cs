using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneDetails : MonoBehaviour
{
    [SerializeField] List<SceneDetails> connectedScenes;
    [SerializeField] AudioClip sceneMusic;
    List<SavableEntity> savableEntities;

    public bool IsLoaded { get; private set; }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {

            Debug.Log($"玩家進入場景{gameObject.name}，開始載入旁邊的區域");
            //LOAd下一個Scene
            LoadScene();
            GameControlller.Instance.SetCurrentScene(this);

            //播放當前LOAD Scene的背景音樂
            AudioManager.i.PlayMusic(sceneMusic, fade: true);

            //LOAD旁邊連接的區域
            foreach (var scene in connectedScenes)
            {
                scene.LoadScene();
            }

            //清除沒有用到的Scene
            var prevScene = GameControlller.Instance.PrevScene;

            if (prevScene != null)
            {
                //獲得先前連接的Scene對象
                var previoslyLoadeScenes = prevScene.connectedScenes;
                foreach (var scene in previoslyLoadeScenes)
                {
                    //如果先前的Scene不是連接的Scene對象 且 不是當前Scene
                    if (!connectedScenes.Contains(scene) && scene != this)
                        scene.UnLoadScene();
                }
            }
            //如果先前的Scene 它不是一個連接的Scene  卸載
            if (!connectedScenes.Contains(prevScene) && prevScene != null)
                prevScene.UnLoadScene();
        }
    }

    public void LoadScene()
    {
        if (!IsLoaded)
        {
            var operation = SceneManager.LoadSceneAsync(gameObject.name, LoadSceneMode.Additive);
            //這可能是為了確保場景只被載入一次，以避免重複載入或其他不必要的操作。
            IsLoaded = true;

            //當場景載入完畢時 還原資料
            operation.completed += (AsyncOperation op) =>
            {
                savableEntities = GetSavableEntitiesInScene();
                //調用保存類中的方法  把資料還原給物件
                SavingSystem.i.RestoreEntityStates(savableEntities);
            };

        }
    }
    public void UnLoadScene()
    {
        //如果已經載入了
        if (IsLoaded)
        {
            //調用保存類中的方法 把物件保存
            SavingSystem.i.CaptureEntityStates(savableEntities);

            SceneManager.UnloadSceneAsync(gameObject.name);
            IsLoaded = false;
        }
    }

    /// <summary>
    /// 獲得要保存物件的場景資料實體
    /// </summary>
    /// <returns></returns>
    List<SavableEntity> GetSavableEntitiesInScene()
    {
        var currenScene = SceneManager.GetSceneByName(gameObject.name);
        //可保存的實體 savableEntities
        var savableEntities = FindObjectsOfType<SavableEntity>().Where(x => x.gameObject.scene == currenScene).ToList();

        return savableEntities;
    }


    public AudioClip SceneMusic => sceneMusic;
}
