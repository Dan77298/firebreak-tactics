using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenManager : MonoBehaviour
{
    [SerializeField] private GameObject PlayerUI;
    [SerializeField] private GameObject Tutorial;

    [SerializeField] private GameObject current;
    [SerializeField] private GameObject next;
    [SerializeField] private GameObject prev;

    private GameManager.GameState currentState;

    void Start()
    {
        GameManager.OnGameStateChanged += GameStateChanged;
    }

    private void OnDestroy()
    {
        GameManager.OnGameStateChanged -= GameStateChanged;
    }

    private void GameStateChanged(GameManager.GameState newState)
    {
    }

    public void NextScreen()
    {
        current.SetActive(false);
        next.SetActive(true);
    }

    public void PrevScreen()
    {
        current.SetActive(false);
        prev.SetActive(true);
    }

    public void EndTutorial()
    {
        PlayerUI.SetActive(true);
        Tutorial.SetActive(false);
        GameManager.Instance.UpdateGameState(GameManager.GameState.PreTurn, null);
    }
}
