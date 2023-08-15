using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FireManager : MonoBehaviour
{
    [SerializeField] private TileManager tileManager;
    [SerializeField] private TMP_Text Heat;
    [SerializeField] private TMP_Text Fire;

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
            // upon player ending turn, check for win/loss conditions
            Debug.Log("FireManager listening");

            int nextFireTiles = tileManager.GetNextFireTiles().Count;

            if (tileManager.hasIgnitableTiles())
            {
                if (tileManager.GetFireTiles().Count > 0) // game goes on
                {
                    tileManager.SpreadFire();
                    GameManager.Instance.UpdateGameState(GameManager.GameState.PreTurn);
                }
                else // victory by 0 fire tiles
                {
                    GameManager.Instance.UpdateGameState(GameManager.GameState.Victory);
                }
            }
            else // lose by all tiles on fire 
            {
                GameManager.Instance.UpdateGameState(GameManager.GameState.Lose);
            }
        }

        if (_state == GameManager.GameState.PreTurn)
        {
            tileManager.DecayFire();
            Heat.text = "HEAT " + tileManager.GetSpreadRate();
            Fire.text = "FIRE " + tileManager.GetFireTiles().Count; // Access fireTiles here
            GameManager.Instance.UpdateGameState(GameManager.GameState.PlayerTurn);
        }
    }
}