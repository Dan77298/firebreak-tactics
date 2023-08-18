using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject LoseUI;
    [SerializeField] private GameObject VictoryUI;
    [SerializeField] private GameObject PlayerUI;
    public static GameManager Instance;
    public GameState State;

    public static event Action<GameState> OnGameStateChanged;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        UpdateGameState(GameState.ProduceTerrain);
    }

    public void UpdateGameState(GameState _state)
    {
        State = _state;
        Debug.Log("State change: " + _state);

        switch (_state)
        {
            case GameState.ProduceTerrain: // TileManager listens
                break;
            case GameState.PlayerTurn: // PlayerController listens
                break;
            case GameState.EnemyTurn: // FireManager listens
                break;
            case GameState.PreTurn: // FireManager listens
                break;
            case GameState.Victory:
                Victory();
                break;
            case GameState.Lose:
                Lose();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(_state), _state, null);
        }

        OnGameStateChanged?.Invoke(_state);
    }

    public enum GameState
    {
        ProduceTerrain,
        PlayerTurn,
        EnemyTurn,
        PreTurn,
        Victory,
        Lose
    }

    private void Victory()
    {
        VictoryUI.SetActive(true);
        PlayerUI.SetActive(false);
    }

    private void Lose()
    {
        LoseUI.SetActive(true);
        PlayerUI.SetActive(false);
    }
}
