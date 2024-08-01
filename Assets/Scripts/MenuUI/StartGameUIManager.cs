using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartGameUIManager : MonoBehaviour
{
    public GameObject StartGame;
    public GameObject EssentialObjects;
    public GameControlller GameControlller;

    public void NewGameStart()
    {
        StartGame.SetActive(true);
        GameControlller.isNewGame = true;
    }

    public void LoadGameStart()
    {
        this.gameObject.SetActive(false);
        GameControlller.isNewGame = false;
        EssentialObjects.SetActive(true);
    }
}
