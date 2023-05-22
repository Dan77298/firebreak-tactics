using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireManager : MonoBehaviour
{
    private bool hasControl = false; // player can interact with the game  

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
        hasControl = (_state == GameManager.GameState.EnemyTurn);
        if (hasControl){
            TakeTurn();
        }
    }

    private void TakeTurn(){
        
    }

    
}
