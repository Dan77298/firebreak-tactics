using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FireManager : MonoBehaviour
{
    [SerializeField] private TileManager tileManager;
    [SerializeField] private TMP_Text  Heat;
    [SerializeField] private TMP_Text  Fire;
    private bool hasControl = false; // player can interact with the game  

    private void Awake()
    {
        GameManager.OnGameStateChanged += GameStateChanged;
    }

    private void OnDestroy()
    {
        GameManager.OnGameStateChanged -= GameStateChanged;
    }

    private void GameStateChanged(GameManager.GameState _state)
    {
        if (_state == GameManager.GameState.EnemyTurn)
        {
            Debug.Log("FireManager listening");
            if (tileManager.GetNextFireTiles().Count > 0) // Added missing parentheses ()
            {
                if (tileManager.GetFireTiles().Count > 0)
                {
                    tileManager.SpreadFire();
                    GameManager.Instance.UpdateGameState(GameManager.GameState.PreTurn);
                }
                else
                {
                    GameManager.Instance.UpdateGameState(GameManager.GameState.Victory); // victory by 0 fire tiles
                }
            }
            else
            {
                GameManager.Instance.UpdateGameState(GameManager.GameState.Lose);
            }
        }

        if (_state == GameManager.GameState.PreTurn)
        {
            tileManager.DecayFire();
            tileManager.GetTileObjects();
            tileManager.SetNextFireTiles();
            Heat.text = "HEAT " + tileManager.GetSpreadRate();
            Fire.text = "FIRE " + tileManager.GetFireTiles().Count;
            // organize ember tile actions 
            GameManager.Instance.UpdateGameState(GameManager.GameState.PlayerTurn);
        }
    }
}