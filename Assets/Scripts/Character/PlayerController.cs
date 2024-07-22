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

    //��s�边�Q����ɽե� �����c�y���
    private void Awake()
    {
        i = this;
        character = GetComponent<Character>();
    }

    public void HandleUpdate()
    {
        //���b����
        if (!character.IsMoving)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            //�]�m���a����﨤����   ��x�b�����s�� y�b���s
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
    /// ���ʵ�����P�_�O�_����L�欰 ��� Ĳ�o�԰�����
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
        //��e���I���������0 �B
        //�}�l�m ��
        if (colliders.Count() == 0 && triggerable != currentlyInTrigger)
            currentlyInTrigger = null;

    }

    /// <summary>
    /// �O�s/SAVE
    /// </summary>
    /// <returns></returns>
    public object CaptureState()
    {
        var saveData = new PlayerSaveData()
        {
            position = new float[] { transform.position.x, transform.position.y },
            //�O�s Pokemon���󪺸�� �W�r ��eHP����
            pokemons = GetComponent<PokemonParty>().Pokemons.Select(p => p.GetSaveData()).ToList(),
        };

        return saveData;
    }

    /// <summary>
    /// �[��/LOAD
    /// </summary>
    /// <param name="state"></param>
    public void RestoreState(object state)
    {
        var saveData = (PlayerSaveData)state;
        //��_/���J ���a���y��
        var pos = saveData.position;
        //�N�O�s���y��Set Value�����a���y��
        transform.position = new Vector3(pos[0], pos[1]);

        //LOAD Pokemon���ƾ�
        //�q�L�c�y��� �N��Ƹ��J
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
