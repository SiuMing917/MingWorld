using GDEUtils.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMenuState : State<GameControlller>
{
    [SerializeField] MenuController menuController;
    public static GameMenuState i { get; private set; }

    private void Awake()
    {
        i = this;
    }

    //GameController以前State做的東西全部放這裏
    GameControlller gc;
    public override void Enter(GameControlller owner)
    {
        gc = owner;
        menuController.gameObject.SetActive(true);
        menuController.OnSelected += OnMenuItemSelected;
        menuController.OnBack += OnBack;
    }

    public override void Execute()
    {
        menuController.HandleUpdate();

    }

    public override void Exit()
    {
        menuController.gameObject.SetActive(false);
        menuController.OnSelected -= OnMenuItemSelected;
        menuController.OnBack -= OnBack;
    }

    void OnMenuItemSelected(int selection)
    {
        Debug.Log($"選擇MENU選項 {selection}");

        if (selection == 0)// Pokemon
            gc.StateMachine.Push(PartyState.i);
        else if (selection == 1)// Bag
            gc.StateMachine.Push(InventoryState.i);
        else if (selection == 2)// Save
        {
            SavingSystem.i.Save("save01");
            gc.StateMachine.Pop();
            StartCoroutine(ShowSaveLoadDialog(true));
        }
        else if (selection == 3)// Load
        {
            SavingSystem.i.Load("save01");
            gc.StateMachine.Pop();
            StartCoroutine(ShowSaveLoadDialog(false));
        }
    }

    void OnBack()
    {
        gc.StateMachine.Pop();
    }

    public IEnumerator ShowSaveLoadDialog(bool isSave)
    {
        if (isSave)
            yield return DialogManager.Instance.ShowDialogText("已保存檔案...");
        else
            yield return DialogManager.Instance.ShowDialogText("已載入檔案...");
    }
}
