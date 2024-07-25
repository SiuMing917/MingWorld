using GDEUtils.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryState : State<GameControlller>
{
    [SerializeField] InventoryUI inventoryUI;

    //Output
    public ItemBase SelectedItem { get; private set; }

    public static InventoryState i { get; private set; }

    private void Awake()
    {
        i = this;
    }

    Inventory inventory;
    private void Start()
    {
        inventory = Inventory.GetInventory();    
    }

    GameControlller gc;

    public override void Enter(GameControlller owner)
    {
        gc = owner;

        SelectedItem = null;

        inventoryUI.gameObject.SetActive(true);
        inventoryUI.OnSelected += OnItemSelected;
        inventoryUI.OnBack += OnBack;
    }

    public override void Execute()
    {
        inventoryUI.HandleUpdate();
    }

    public override void Exit()
    {
        inventoryUI.gameObject.SetActive(false);
        inventoryUI.OnSelected -= OnItemSelected;
        inventoryUI.OnBack -= OnBack;
    }

    void OnItemSelected(int selection)
    {
        SelectedItem = inventoryUI.SelectedItem;

        if (gc.StateMachine.GetPrevState() != ShopSellingState.i)
            StartCoroutine(SelectPokemonAndUseTtem());
        else
            gc.StateMachine.Pop();
    }

    void OnBack()
    {
        SelectedItem = null;
        gc.StateMachine.Pop();
    }

    IEnumerator SelectPokemonAndUseTtem()
    {
        var prevState = gc.StateMachine.GetPrevState();
        if (prevState == BattleState.i)
        {
            //在戰鬥中
            if(!SelectedItem.CanUseInBattle)
            {
                yield return DialogManager.Instance.ShowDialogText("這個物品不能在戰鬥中使用！");
                yield break;
            }
        }
        else
        {
            //戰鬥外的情況
            if(!SelectedItem.CanUseOutsideBattle)
            {
                yield return DialogManager.Instance.ShowDialogText("這個物品只在戰鬥中使用！");
                yield break;
            }
        }

        if(SelectedItem is PokeballItem)
        {
            inventory.UseItem(SelectedItem, null);
            gc.StateMachine.Pop();
            yield break;
        }
        yield return gc.StateMachine.PushAndWait(PartyState.i);

        
        if(prevState == BattleState.i)
        {
            if (UseItemState.i.ItemUsed)
                gc.StateMachine.Pop();
        }
    }
}
