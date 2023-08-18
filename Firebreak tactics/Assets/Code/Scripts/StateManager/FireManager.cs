using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FireManager : MonoBehaviour
{
    [SerializeField] private WindDirection wind = WindDirection.S; // south default to establish game
    [SerializeField] private TileManager tileManager;
    [SerializeField] private TMP_Text Heat;
    [SerializeField] private TMP_Text Turn,Turn2;
    [SerializeField] private TMP_Text Fire;
    [SerializeField] private TMP_Text UIN,UIE,UIS,UIW;
    private int globaltime, time, Ntime = 0;

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
        List<GameObject> spreadTiles = tileManager.getDownBreezeTiles((TileManager.WindDirection)wind);
        List<WindDirection> choices = choices = new List<WindDirection>{
            WindDirection.N, WindDirection.E, WindDirection.S, WindDirection.W};

        if (globaltime < 7 || Ntime < 5){
        // wind direction can't change to north until turn 7 or if it's been 5 turns since north
            choices.Remove(WindDirection.N);
        }

        if (spreadTiles.Count <= tileManager.GetSpreadRate() || spreadTiles.Count < 3){
        // if the current direction will result in filling or underwhelming enemy turn, force a swap
        // this will prevent map being 100% full but prevent the fire cornering itself 
            choices.Remove(wind);
            int newDirection = UnityEngine.Random.Range(0, choices.Count);
            wind = choices[newDirection];
            time = 0;
        }

        // add a biase for the direction to change with respect to the user and objectives

        if (wind == WindDirection.N && time >= 1){
            // only let it stay north for 1 turn and put north on cooldown
            choices.Remove(wind);
            int newDirection = UnityEngine.Random.Range(0, choices.Count);
            wind = choices[newDirection];
            time ++;
            Ntime = 0; // north cooldown
        }
        else{
            if (wind != WindDirection.S && time >= 2){
            // change at least every 2 turns if it isn't southernly 
                choices.Remove(wind);
                int newDirection = UnityEngine.Random.Range(0, choices.Count);
                wind = choices[newDirection];
                time ++;
            }
            else{
            // sometimes change sooner 1/3 of the time
                if (UnityEngine.Random.Range(0, 3) == 0){
                    int newDirection = UnityEngine.Random.Range(0, choices.Count);
                    wind = choices[newDirection];
                    time = 0;
                }           
            }  
        }   
        Ntime++;
        globaltime++;
        colourCompass();
    }

    private void colourCompass(){
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

            if (tileManager.hasTurnsRemaining())
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
            else // the fire has no more options  
            {
                GameManager.Instance.UpdateGameState(GameManager.GameState.Victory);
            }
        }

        if (_state == GameManager.GameState.PreTurn)
        {
            ChangeWindDirection();
            tileManager.DecayFire();
            Turn.text = "TURN " + globaltime;
            Turn2.text = "TURN " + globaltime;
            Heat.text = "HEAT " + tileManager.GetSpreadRate();
            Fire.text = "FIRE " + tileManager.GetFireTiles().Count;
            GameManager.Instance.UpdateGameState(GameManager.GameState.PlayerTurn);
        }
    }
}