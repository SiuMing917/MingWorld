using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerController : MonoBehaviour, ISavable
{

    [SerializeField] string name;
    [SerializeField] Sprite sprite;

    const float offsetY = 0.3f;



    private Vector2 input;

    private Character character;

    public string Name
    {
        get
        {
            return name;
        }

    }

    public Sprite Sprite
    {
        get
        {
            return sprite;
        }

    }

    public static PlayerController i { get; private set; }

    //當編輯器被喚醒時調用 類似構造函數
    private void Awake()
    {
        i = this;
        character = GetComponent<Character>();
    }

    public void HandleUpdate()
    {
        //不在移動
        if (!character.IsMoving)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            //設置玩家不能對角移動   當x軸不為零時 y軸為零
            if (input.x != 0) { input.y = 0; }
            //else if (input.y != 0) { input.x = 0; }


            if (input != Vector2.zero)
            {
                StartCoroutine(character.Move(input, OnMoveOver));
            }
        }

        character.HandleUpdate();

        if (Input.GetKeyDown(KeyCode.Z))
            StartCoroutine(Interact());
    }


    IEnumerator Interact()
    {

        var facingDir = new Vector3(character.Animator.MoveX, character.Animator.MoveY);
        var interactPos = transform.position + facingDir;

        // Debug.DrawLine(transform.position, interactPos, Color.green, 0.5f);

        var collider = Physics2D.OverlapCircle(interactPos, 0.3f, GameLayers.I.InteractableLayer | GameLayers.I.WaterLayer);
        if (collider != null)
        {
            yield return collider.GetComponent<Interactable>()?.Interact(transform);
        }
    }

    IPlayerTriggerable currentlyInTrigger;
    /// <summary>
    /// 移動結束後判斷是否有其他行為 對視 觸發戰鬥等等
    /// </summary>
    private void OnMoveOver()
    {
        var colliders = Physics2D.OverlapCircleAll(transform.position - new Vector3(0, character.OffsetY), 0.2f, GameLayers.I.TriggerableLayers);

        IPlayerTriggerable triggerable = null;
        foreach (var collider in colliders)
        {
            triggerable = collider.GetComponent<IPlayerTriggerable>();
            if (triggerable != null)
            {
                if (triggerable == currentlyInTrigger && !triggerable.TriggerRepeatedly)
                    break;

                triggerable.OnPlayerTriggered(this);
                currentlyInTrigger = triggerable;
                break;
            }
        }
        //當前的碰撞器物件數0 且
        //開始置 空
        if (colliders.Count() == 0 && triggerable != currentlyInTrigger)
            currentlyInTrigger = null;

    }

    /// <summary>
    /// 保存/SAVE
    /// </summary>
    /// <returns></returns>
    public object CaptureState()
    {
        var saveData = new PlayerSaveData()
        {
            position = new float[] { transform.position.x, transform.position.y },
            //保存 Pokemon物件的資料 名字 當前HP等等
            pokemons = GetComponent<PokemonParty>().Pokemons.Select(p => p.GetSaveData()).ToList(),
        };

        return saveData;
    }

    /// <summary>
    /// 加載/LOAD
    /// </summary>
    /// <param name="state"></param>
    public void RestoreState(object state)
    {
        var saveData = (PlayerSaveData)state;
        //恢復/載入 玩家的座標
        var pos = saveData.position;
        //將保存的座標Set Value給玩家的座標
        transform.position = new Vector3(pos[0], pos[1]);

        //LOAD Pokemon的數據
        //通過構造函數 將資料載入
        GetComponent<PokemonParty>().Pokemons = saveData.pokemons.Select(s => new Pokemon(s)).ToList();
    }

    public Character Character => character;
}

[Serializable]
public class PlayerSaveData
{
    public float[] position;
    public List<PokemonSaveData> pokemons;
}
