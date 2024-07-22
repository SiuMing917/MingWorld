using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLayers : MonoBehaviour
{

    [SerializeField] LayerMask solidObjectsLayer;
    [SerializeField] LayerMask interactableLayer;
    [SerializeField] LayerMask longGrassLayer;
    [SerializeField] LayerMask playerLayer;
    [SerializeField] LayerMask fovLayer;
    [SerializeField] LayerMask portalLayer;
    [SerializeField] LayerMask tirggersLayer;
    [SerializeField] LayerMask ledgeLayer;
    [SerializeField] LayerMask waterLayer;

    public static GameLayers I { get; set; }

    public void Awake()
    {
        I = this;
    }

    public LayerMask SolidObjectsLayer
    {
        get => solidObjectsLayer;
    }
    public LayerMask InteractableLayer
    {
        get => interactableLayer;
    }
    public LayerMask LongGrassLayer
    {
        get => longGrassLayer;
    }
    public LayerMask PlayerLayer
    {
        get => playerLayer;
    }
    public LayerMask FovLayer
    {
        get => fovLayer;
    }
    public LayerMask PortalLayer
    {
        get => portalLayer;
    }

    public LayerMask TriggersLayer
    {
        get => tirggersLayer;
    }
    public LayerMask LedgeLayer
    {
        get => ledgeLayer;
    }

    public LayerMask WaterLayer
    {
        get => waterLayer;
    }

    public LayerMask TriggerableLayers
    {
        get => longGrassLayer | fovLayer | portalLayer | tirggersLayer | waterLayer;
    }
}

