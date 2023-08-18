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
            // code

            // we will want to grab gridXZ from GridManager for the 2D array of all tiles
            /* and use cellToWorld or get the GameObject from gridXZ and do tile.transform.position
               for the sake of placing units on cell position    
            */

        }
    }
}
