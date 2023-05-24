using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTurnUI : MonoBehaviour
{
	private bool PlayerTurn = false;
    void Awake()
    {
        GameManager.OnGameStateChanged += GameStateChanged;
    }

    void OnDestroy()
    {
        GameManager.OnGameStateChanged -= GameStateChanged;
    }

    private void GameStateChanged(GameManager.GameState _state){
        PlayerTurn = (_state == GameManager.GameState.PlayerTurn);
        if (PlayerTurn){
        	Debug.Log("PlayerTurnUI listening");
        }
    }

    public void EndTurn(){
    	if (PlayerTurn)
    	{
    		Debug.Log("Ending state: PlayerTurn");
    		GameManager.Instance.UpdateGameState(GameManager.GameState.EnemyTurn);
    	}
    }
}
