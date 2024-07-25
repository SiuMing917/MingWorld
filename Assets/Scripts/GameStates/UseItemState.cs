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
    /// �ϥΪ��~
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
            //�B�z�i�ƪ��~
            if (item is EvolutionItem)
            {
                //�˴��_�i�ڶi�� �i�Ϊ��i�ƪ��~
                var evolution = pokemon.CheckForEvolution(item);
                if (evolution != null)
                {
                    yield return EvolutionState.i.Evolve(pokemon, evolution);
                }
                else
                {
                    yield return DialogManager.Instance.ShowDialogText("���~�S���ĪG");
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
                yield return DialogManager.Instance.ShowDialogText($"���a�ϥΤF{usedItem.Name}");
        }
        else
        {
            if (inventoryUI.SelectedCategory == (int)ItemCategory.Items)
                yield return DialogManager.Instance.ShowDialogText("���~�S���ĪG");
        }

        //�����e��
        gc.StateMachine.Pop();
    }

    /// <summary>
    /// �ޯ�ǲ߳B�z���
    /// </summary>
    /// <returns></returns>
    IEnumerator HandleTmItems()
    {
        var tmItem = inventoryUI.SelectedItem as TmItem;
        if (tmItem == null)
            yield break;

        var pokemon = partyScreen.SelectedMember;

        //�ޯ�֦��P�w
        if (pokemon.HasMove(tmItem.Move))
        {
            yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name}�w�g�Ƿ|�F{tmItem.Move.Name}");
            yield break;
        }
        //����ǲߪ��ޯ�P�w
        if (!tmItem.CanBeTaught(pokemon))
        {
            yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name}����ǲ�{tmItem.Move.Name}");
            yield break;
        }

        if (pokemon.Moves.Count < PokemonBase.MaxNumOfMoves)
        {
            pokemon.LearnMove(tmItem.Move);
            yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name}�Ƿ|�F{tmItem.Move.Name}");
        }
        else
        {
            yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name}���վǲ�{tmItem.Move.Name}");
            yield return DialogManager.Instance.ShowDialogText($"���O�ޯ�ƶq�W�L�F{PokemonBase.MaxNumOfMoves}");

            yield return DialogManager.Instance.ShowDialogText($"�п�ܭn��Ѫ��ޯ�", true, false);

            MoveToForgetState.i.NewMove = tmItem.Move;
            MoveToForgetState.i.CurrentMoves = pokemon.Moves.Select(m => m.Base).ToList();
            yield return gc.StateMachine.PushAndWait(MoveToForgetState.i);

            var moveIndex = MoveToForgetState.i.Selection;
            if (moveIndex == PokemonBase.MaxNumOfMoves || moveIndex == -1)
            {
                //���ǲ߷s�ޯ�
                yield return (DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name}�S���ǲ�{tmItem.Move.Name}"));
            }
            else
            {
                //�ѰO�w�����ޯ�A�ǲ߷s���ޯ�
                var selectedMove = pokemon.Moves[moveIndex];
                yield return (DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name}��ѤF{selectedMove.Base.Name}�A�ǲߤF{tmItem.Move.Name}"));

                //��ȴ��� �ޯ� �ѰO �ǲ�
                pokemon.Moves[moveIndex] = new Move(tmItem.Move);
            }
        }

    }
}
