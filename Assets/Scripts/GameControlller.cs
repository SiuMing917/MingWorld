using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum GameState { FreeRoam, Battle, Dialog, Memu, PartyScreen, Bag, Cutscene, Pause, Evolution, Shop }
public class GameControlller : MonoBehaviour
{

    [SerializeField] PlayerController PlayerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;

    [SerializeField] PartyScreen partyScreen;
    [SerializeField] InventoryUI inventoryUI;
    GameState state;
    GameState prevState;
    GameState stateBeforeEvolution;

    public SceneDetails CurrentScene { get; private set; }

    public SceneDetails PrevScene { get; private set; }

    MemuController memuController;


    public static GameControlller Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        memuController = GetComponent<MemuController>();

        PokemonDB.Init();
        MoveDB.Init();
        ConditionDB.Init();
        ItemDB.Init();
        QuestDB.Init();
    }

    private void Start()
    {
        //OnBattleOver返回 bool 類型 判斷戰鬥是否勝利（相對於玩家而言）
        battleSystem.OnBattleOver += EndBattle;
        //初始化隊伍介面
        partyScreen.Init();

        DialogManager.Instance.OnShowDialog += () =>
        {
            prevState = state;
            state = GameState.Dialog;
        };
        DialogManager.Instance.OnDialogFinished += () =>
        {
            if (state == GameState.Dialog)
                state = prevState;
        };

        //在功能表中選擇關閉時  回到自由狀態
        memuController.onBack += () =>
        {
            state = GameState.FreeRoam;
        };
        //功能表選擇
        memuController.onMenuSelected += OnMenuSelected;

        EvolutionManger.i.OnStartEvolution += () =>
        {
            stateBeforeEvolution = state;
            state = GameState.Evolution;
        };
        EvolutionManger.i.OnCompleteEvolution += () =>
        {
            partyScreen.SetPartyDate();
            state = stateBeforeEvolution;

            //進化結束 播放
            AudioManager.i.PlayMusic(CurrentScene.SceneMusic, fade: true);

        };

        ShopController.i.OnStart += () => state = GameState.Shop;
        ShopController.i.OnFinish += () => state = GameState.FreeRoam;
    }


    /// <summary>
    /// 暫停 玩家的行為
    /// </summary>
    /// <param name="pause"></param>
    public void PauseGame(bool pause)
    {
        if (pause)
        {
            prevState = state;
            state = GameState.Pause;
        }
        else
        {
            state = prevState;
        }
    }

    //開始Cut Scene
    public void StartCutsceneState()
    {
        state = GameState.Cutscene;
    }
    public void StartFreeRoamState()
    {
        state = GameState.FreeRoam;
    }

    public void StartBattle(BattleTrigger trigger)
    {
        state = GameState.Battle;
        //啟動battleSystem
        battleSystem.gameObject.SetActive(true);
        //關閉 世界地圖攝像機
        worldCamera.gameObject.SetActive(false);

        //將PokemonParty元件附加到PlayerController物件上
        var playerPokemon = PlayerController.GetComponent<PokemonParty>();

        //從當前場景中  獲得MapArea元件（腳本  調用生成野生寶可夢的方法
        var wildPokemon = CurrentScene.GetComponent<MapArea>().GetWildPokemon(trigger);

        //複製出新的寶可夢 創建一個物件
        var wildPokemonCopy = new Pokemon(wildPokemon.Base, wildPokemon.Level);

        battleSystem.StartBattle(playerPokemon, wildPokemonCopy, trigger);
    }

    TrainerController trainer;
    /// <summary>
    ///訓練家戰鬥
    /// </summary>
    /// <param name="trainer"></param>
    public void StartTrainerBattle(TrainerController trainer)
    {
        state = GameState.Battle;
        //啓動battleSystem 
        battleSystem.gameObject.SetActive(true);
        //關閉 世界地图摄像机
        worldCamera.gameObject.SetActive(false);
        this.trainer = trainer;
        //將PokemonParty元件附加到PlayerController物件上
        var playerParty = PlayerController.GetComponent<PokemonParty>();
        var trainerParty = trainer.GetComponent<PokemonParty>();

        battleSystem.StartTrainerBattle(playerParty, trainerParty);
    }


    /// <summary>
    /// 進入訓練家的視角 觸發戰鬥
    /// </summary>
    /// <param name="trainer"></param>
    public void OnEnterTrainerView(TrainerController trainer)
    {
        state = GameState.Cutscene;
        StartCoroutine(trainer.TriggerTrainerBattle(PlayerController));
    }


    /// <summary>
    /// 戰鬥結束 遊戲行為
    /// </summary>
    /// <param name="won"></param>
    void EndBattle(bool won)
    {
        if (trainer != null && won == true)
        {
            trainer.BattleLost();
            trainer = null;
        }

        partyScreen.SetPartyDate();

        state = GameState.FreeRoam;
        battleSystem.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);

        var playerParty = PlayerController.GetComponent<PokemonParty>();


        //存在進化 ？
        bool hasEvolution = playerParty.CheckForEvolution();
        //執行進化  
        if (hasEvolution)
            StartCoroutine(playerParty.RunEvolution());
        else
            AudioManager.i.PlayMusic(CurrentScene.SceneMusic, fade: true);

    }

    /// <summary>
    /// 玩家輸入 行為  控制器權管理
    /// </summary>
    private void Update()
    {
        if (state == GameState.FreeRoam)
        {
            PlayerController.HandleUpdate();
            //打開背包
            if (Input.GetKeyDown(KeyCode.Return))
            {
                memuController.OpenMemu();
                state = GameState.Memu;
            }

        }
        else if(state == GameState.Cutscene)
        {
            PlayerController.Character.HandleUpdate();
        }
        //戰鬥
        else if (state == GameState.Battle)
        {
            battleSystem.HandleUpdate();
        }
        //對話
        else if (state == GameState.Dialog)
        {
            DialogManager.Instance.HandleUpdate();
        }
        //清單畫面
        else if (state == GameState.Memu)
        {
            memuController.HandleUpdate();
        }
        //隊伍畫面
        else if (state == GameState.PartyScreen)
        {
            Action onSelected = () =>
            {
                //顯示信息之類的
            };
            Action onBack = () =>
            {
                //關閉畫面
                partyScreen.gameObject.SetActive(false);
                state = GameState.FreeRoam;
            };

            partyScreen.HandleUpdate(onSelected, onBack);
        }
        //背包畫面
        else if (state == GameState.Bag)
        {
            Action onBack = () =>
            {
                //關閉畫面
                inventoryUI.gameObject.SetActive(false);
                state = GameState.FreeRoam;
            };

            inventoryUI.HandleUpdate(onBack);
        }
        //商店畫面
        else if (state == GameState.Shop)
        {
            ShopController.i.HandleUpdate();
        }



    }

    /// <summary>
    /// 設置當前場景和之前的場景
    /// </summary>
    /// <param name="currScene">當前的場景</param>
    public void SetCurrentScene(SceneDetails currScene)
    {
        PrevScene = CurrentScene;
        CurrentScene = currScene;
    }

    /// <summary>
    /// 功能表選擇執行
    /// </summary>
    /// <param name="selectedItem"></param>
    void OnMenuSelected(int selectedItem)
    {
        if (selectedItem == 0)
        {
            //Pokemon隊伍
            partyScreen.gameObject.SetActive(true);
            state = GameState.PartyScreen;
        }
        else if (selectedItem == 1)
        {
            //背包
            inventoryUI.gameObject.SetActive(true);
            state = GameState.Bag;
        }
        else if (selectedItem == 2)
        {
            //保存/SAVE
            SavingSystem.i.Save("save01");
            state = GameState.FreeRoam;
        }
        else if (selectedItem == 3)
        {
            //載入/LOAD
            SavingSystem.i.Load("save01");
            state = GameState.FreeRoam;
        }
        //回到自由行動模式
    }


    /// <summary>
    /// 調整相機 移動 淡化淡出
    /// </summary>
    /// <param name="moveOffset"></param>
    public IEnumerator MoveCamera(Vector2 moveOffset, bool waitForFadeOut = false)
    {
        yield return Fader.i.FadeIn(0.5f);


        worldCamera.transform.position += new Vector3(moveOffset.x, moveOffset.y);

        //這兩種方式的區別就是先執行後續代碼或者等待淡出效果完成再執行後續代碼。
        if (waitForFadeOut)
            yield return Fader.i.FadeOut(0.5f);
        else
            StartCoroutine(Fader.i.FadeOut(0.5f));
    }


    public GameState State => state;
}
