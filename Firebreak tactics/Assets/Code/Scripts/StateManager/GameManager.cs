using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
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

        switch (_state)
        {
            case GameState.PlayerTurn:
                break;
            case GameState.EnemyTurn:
                break;
            case GameState.PreTurn:
                PreTurn();
                break;
            case GameState.Victory:
                break;
            case GameState.Lose:
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

    public void EndTurn(){
        if (State == GameState.EnemyTurn)
        {
            State = GameState.EnemyTurn; // change this to player's turn or pre turn later
            OnGameStateChanged?.Invoke(State); // FireManager is listening for active
        }
    }

    public void PreTurn(){
        OnGameStateChanged?.Invoke(State); // FireManager is listening for active 
        // change wind direction 
    }
}
