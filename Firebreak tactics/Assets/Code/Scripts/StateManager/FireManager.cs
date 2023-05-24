using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireManager : MonoBehaviour
{
    [SerializeField] TileManager tileManager;
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
        if (_state == GameManager.GameState.EnemyTurn){
        	Debug.Log("FireManager listening");
        	if (tileManager.GetFireTiles().Count > 0){
	            tileManager.SpreadFire();
	            GameManager.Instance.UpdateGameState(GameManager.GameState.PreTurn);
        	}
        	else{
        		GameManager.Instance.UpdateGameState(GameManager.GameState.Victory); // victory by 0 fire tiles
        	}

        }

        if (_state == GameManager.GameState.PreTurn)
        {
        	// tell TileManager to change decay of every fire tile  
        	tileManager.DecayFire();
        	tileManager.SetNextFireTiles();
        	// organize ember tile actions 
        	Debug.Log("Ending state: PreTurn");
        	GameManager.Instance.UpdateGameState(GameManager.GameState.PlayerTurn);
        }
    }
}
