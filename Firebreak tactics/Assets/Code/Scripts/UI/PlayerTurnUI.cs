using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTurnUI : MonoBehaviour
{
    private bool PlayerTurn = false;
    [SerializeField] private GameObject tutorial;
    private string check;

    void Awake()
    {
        GameManager.OnGameStateChanged += GameStateChanged;
    }

    void OnDestroy()
    {
        GameManager.OnGameStateChanged -= GameStateChanged;
    }

    private void GameStateChanged(GameManager.GameState _state)
    {

        PlayerTurn = (_state == GameManager.GameState.PlayerTurn);
        check = _state.ToString(); // Convert _state to a string

        if (PlayerTurn)
        {
            // You can perform actions when it's the player's turn here.
        }
    }

    public void EndTurn()
    {
        if (PlayerTurn && !tutorial.activeSelf)
        {
            GameManager.Instance.UpdateGameState(GameManager.GameState.EnemyTurn, null);
        }
    }
}
