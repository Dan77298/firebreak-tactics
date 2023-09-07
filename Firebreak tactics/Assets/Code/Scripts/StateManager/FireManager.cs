using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FireManager : MonoBehaviour
{
    [SerializeField] private WindDirection wind = WindDirection.S; // south default to establish game
    [SerializeField] private TileManager tileManager;
    [SerializeField] private TMP_Text Heat;
    [SerializeField] private TMP_Text Fire;
    [SerializeField] private TMP_Text UIN,UIE,UIS,UIW;
    [SerializeField] public Image compassNeedle;
    private bool isWindSame = false;

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
        if (UnityEngine.Random.Range(0, 4) == 0)
        {
            List<WindDirection> choices = new List<WindDirection>{
                WindDirection.N, WindDirection.E, WindDirection.S, WindDirection.W
            };
            choices.Remove(wind);

            int newDirection = UnityEngine.Random.Range(0, choices.Count);
            wind = choices[newDirection];
        }

         // compass needle changer
        switch (wind){
            case WindDirection.N:
                compassNeedle.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                break;
            case WindDirection.W:
                compassNeedle.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                break;
            case WindDirection.E:
                compassNeedle.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 270f);
                break;
            default: // S
                compassNeedle.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 180f);
                break;
        }
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
    
    public WindDirection getWind() {
        return wind;
    }
}