using GDEUtils.StateMachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UseItemState : State<GameControlller>
{
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] InventoryUI inventoryUI;

    // Output
    public bool ItemUsed { get; private set; }

    public static UseItemState i { get; private set; }
    Inventory inventory;

    private void Awake()
    {
        i = this;

        ItemUsed = false;

        inventory = Inventory.GetInventory();
    }

    GameControlller gc;

    public override void Enter(GameControlller owner)
    {
        gc = owner;

        StartCoroutine(UseItem());
    }

    /// <summary>
    /// 使用物品
    /// </summary>
    /// <returns></returns>
    IEnumerator UseItem()
    {

        var item = inventoryUI.SelectedItem;
        var pokemon = partyScreen.SelectedMember;

        if(item is TmItem)
        {
            yield return HandleTmItems();
        }
        else
        {
            //處理進化物品
            if (item is EvolutionItem)
            {
                //檢測寶可夢進化 可用的進化物品
                var evolution = pokemon.CheckForEvolution(item);
                if (evolution != null)
                {
                    yield return EvolutionState.i.Evolve(pokemon, evolution);
                }
                else
                {
                    yield return DialogManager.Instance.ShowDialogText("物品沒有效果");
                    gc.StateMachine.Pop();
                    yield break;
                }
            }

        }

        var usedItem = inventory.UseItem(item, partyScreen.SelectedMember);
        if (usedItem != null)
        {
            ItemUsed = true;

            if ((usedItem is RecoverItem))
                yield return DialogManager.Instance.ShowDialogText($"玩家使用了{usedItem.Name}");
        }
        else
        {
            if (inventoryUI.SelectedCategory == (int)ItemCategory.Items)
                yield return DialogManager.Instance.ShowDialogText("物品沒有效果");
        }

        //關閉畫面
        gc.StateMachine.Pop();
    }

    /// <summary>
    /// 技能學習處理函數
    /// </summary>
    /// <returns></returns>
    IEnumerator HandleTmItems()
    {
        var tmItem = inventoryUI.SelectedItem as TmItem;
        if (tmItem == null)
            yield break;

        var pokemon = partyScreen.SelectedMember;

        //技能擁有判定
        if (pokemon.HasMove(tmItem.Move))
        {
            yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name}已經學會了{tmItem.Move.Name}");
            yield break;
        }
        //不能學習的技能判定
        if (!tmItem.CanBeTaught(pokemon))
        {
            yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name}不能學習{tmItem.Move.Name}");
            yield break;
        }

        if (pokemon.Moves.Count < PokemonBase.MaxNumOfMoves)
        {
            pokemon.LearnMove(tmItem.Move);
            yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name}學會了{tmItem.Move.Name}");
        }
        else
        {
            yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name}嘗試學習{tmItem.Move.Name}");
            yield return DialogManager.Instance.ShowDialogText($"但是技能數量超過了{PokemonBase.MaxNumOfMoves}");

            yield return DialogManager.Instance.ShowDialogText($"請選擇要遺忘的技能", true, false);

            MoveToForgetState.i.NewMove = tmItem.Move;
            MoveToForgetState.i.CurrentMoves = pokemon.Moves.Select(m => m.Base).ToList();
            yield return gc.StateMachine.PushAndWait(MoveToForgetState.i);

            var moveIndex = MoveToForgetState.i.Selection;
            if (moveIndex == PokemonBase.MaxNumOfMoves || moveIndex == -1)
            {
                //不學習新技能
                yield return (DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name}沒有學習{tmItem.Move.Name}"));
            }
            else
            {
                //忘記已有的技能，學習新的技能
                var selectedMove = pokemon.Moves[moveIndex];
                yield return (DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name}遺忘了{selectedMove.Base.Name}，學習了{tmItem.Move.Name}"));

                //賦值替換 技能 忘記 學習
                pokemon.Moves[moveIndex] = new Move(tmItem.Move);
            }
        }

    }
}
