using GDEUtils.StateMachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameControlller : MonoBehaviour
{

    [SerializeField] PlayerController playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;

    [SerializeField] PartyScreen partyScreen;
    [SerializeField] InventoryUI inventoryUI;

    //控制狀態的Machine
    public StateMachine<GameControlller> StateMachine { get; private set; }

    public SceneDetails CurrentScene { get; private set; }

    public SceneDetails PrevScene { get; private set; }


    public static GameControlller Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        //menuController = GetComponent<MenuController>();

        PokemonDB.Init();
        MoveDB.Init();
        ConditionDB.Init();
        ItemDB.Init();
        QuestDB.Init();
    }

    private void Start()
    {
        //初始化State Machine
        StateMachine = new StateMachine<GameControlller>(this);
        StateMachine.ChangeState(FreeRoamState.i);

        //OnBattleOver返回 bool 類型 判斷戰鬥是否勝利（相對於玩家而言）
        battleSystem.OnBattleOver += EndBattle;
        //初始化隊伍介面
        partyScreen.Init();

        DialogManager.Instance.OnShowDialog += () =>
        {
            StateMachine.Push(DialogueState.i);
        };
        DialogManager.Instance.OnDialogFinished += () =>
        {
            StateMachine.Pop();
        };
    }


    /// <summary>
    /// 暫停 玩家的行為
    /// </summary>
    /// <param name="pause"></param>
    public void PauseGame(bool pause)
    {
        if (pause)
        {
            StateMachine.Push(PauseState.i);
        }
        else
        {
            StateMachine.Pop();
        }
    }

    public void StartBattle(BattleTrigger trigger)
    {
        BattleState.i.trigger = trigger;
        StateMachine.Push(BattleState.i);
    }

    TrainerController trainer;
    /// <summary>
    ///訓練家戰鬥
    /// </summary>
    /// <param name="trainer"></param>
    public void StartTrainerBattle(TrainerController trainer)
    {
        BattleState.i.trainer = trainer;
        StateMachine.Push(BattleState.i);
    }


    /// <summary>
    /// 進入訓練家的視角 觸發戰鬥
    /// </summary>
    /// <param name="trainer"></param>
    public void OnEnterTrainerView(TrainerController trainer)
    {
        StartCoroutine(trainer.TriggerTrainerBattle(playerController));
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

        battleSystem.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);

        var playerParty = playerController.GetComponent<PokemonParty>();


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
        //有了State Machine,可以簡化Sate的Code
        StateMachine.Execute();
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

    private void OnGUI()
    {
        var style = new GUIStyle();
        style.fontSize = 24;

        //GUILayout.Label("STATE STACK",style);
        var state = StateMachine.CurrentState;
        GUILayout.Label(state.GetType().ToString(), style);
        //foreach (var state in StateMachine.StateStack)
        //{
        //    GUILayout.Label(state.GetType().ToString(), style);
        //}
    }
    public PlayerController PlayerController => playerController;
    public Camera WorldCamera => worldCamera;

    public PartyScreen PartyScreen => partyScreen;
}
