using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FireManager : MonoBehaviour
{
    [SerializeField] private WindDirection wind = WindDirection.S; // south default to establish game
    [SerializeField] private TileManager tileManager;
    [SerializeField] private TMP_Text Heat;
    [SerializeField] private TMP_Text Fire;
    [SerializeField] private TMP_Text UIN,UIE,UIS,UIW;

    private void Awake()
    {
        GameManager.OnGameStateChanged += GameStateChanged;
    }

    private void OnDestroy()
    {
        GameManager.OnGameStateChanged -= GameStateChanged;
    }

    public enum WindDirection{
        N, E, S, W
    }

    public void ChangeWindDirection()
    {
        if (UnityEngine.Random.Range(0, 8) == 0)
        {
            List<WindDirection> choices = new List<WindDirection>{
                WindDirection.N, WindDirection.E, WindDirection.S, WindDirection.W
            };
            choices.Remove(wind);

            int newDirection = UnityEngine.Random.Range(0, choices.Count);
            wind = choices[newDirection];
        }

        UIN.color = new Color32(171,171,171,255);
        UIE.color = new Color32(171,171,171,255);
        UIS.color = new Color32(171,171,171,255);
        UIW.color = new Color32(171,171,171,255);

         // colour changer 
        switch (wind){
            case WindDirection.N:
                UIN.color = new Color32(255,0,0,255);
                break;
            case WindDirection.W:
                UIW.color = new Color32(255,0,0,255);
                break;
            case WindDirection.E:
                UIE.color = new Color32(255,0,0,255);
                break;
            default: // S
                UIS.color = new Color32(255,0,0,255);
                break;
        }
    }

    private void GameStateChanged(GameManager.GameState _state)
    {

        if (_state == GameManager.GameState.EnemyTurn)
        {
            // upon player ending turn, check for win/loss conditions
            Debug.Log("FireManager listening");

            if (tileManager.hasIgnitableTiles())
            {
                if (tileManager.GetFireTiles().Count > 0) // game goes on
                {
                    tileManager.SpreadFire((TileManager.WindDirection)wind);
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
            ChangeWindDirection();
            tileManager.DecayFire();
            Heat.text = "HEAT " + tileManager.GetSpreadRate();
            Fire.text = "FIRE " + tileManager.GetFireTiles().Count;
            GameManager.Instance.UpdateGameState(GameManager.GameState.PlayerTurn);
        }
    }
}