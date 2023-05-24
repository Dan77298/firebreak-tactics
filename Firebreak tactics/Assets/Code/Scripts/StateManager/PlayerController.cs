using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour{
    private bool hasControl = false; // player can interact with the game  

    void Awake(){
        GameManager.OnGameStateChanged += GameStateChanged;
    }

    void OnDestroy(){
        GameManager.OnGameStateChanged -= GameStateChanged;
    }

    private void GameStateChanged(GameManager.GameState _state){
        if (_state == GameManager.GameState.PlayerTurn){
            //GameManager.Instance.UpdateGameState(GameManager.GameState.EnemyTurn);
        }
    }
}
