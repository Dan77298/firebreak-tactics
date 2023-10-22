using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;

public class FireManager : MonoBehaviour
{
    [SerializeField] private WindDirection wind = WindDirection.S; // south default to establish game
    [SerializeField] private TileManager tileManager;
    [SerializeField] private UnitManager unitManager;
    [SerializeField] private BaseManager baseManager; 
    [SerializeField] private TMP_Text Heat;
    [SerializeField] private TMP_Text Turn,Turn2;
    [SerializeField] private TMP_Text Fire;
    [SerializeField] private TMP_Text UIN,UIE,UIS,UIW;
    [SerializeField] private Image CompassNeedle;

    private int numberOfTurns, time, Ntime = 0;
    private List<GameObject> tileImmunities = new List<GameObject>(); // list of all prevented tiles 

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

        // if (Ntime < 6){
        // // wind direction can't change to north until it's been 5 turns since north
        //     choices.Remove(WindDirection.N);
        // }

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
            time = 0;
            Ntime = 0; // north cooldown
        }
        else if ((wind == WindDirection.S && time >= 3) || time >= 5){
        // change wind if southernly every 3, otherwise 2 for east or west
            choices.Remove(wind);
            int newDirection = UnityEngine.Random.Range(0, choices.Count);
            wind = choices[newDirection];
            time = 0;
        }   
        time++; // resets on wind change 
        Ntime++; // prevents north overselection
        numberOfTurns++; // tracks number of player turns  
        needlechanger();
    }

    private void needlechanger(){
        UIN.color = new Color32(171,171,171,255);
        UIE.color = new Color32(171,171,171,255);
        UIS.color = new Color32(171,171,171,255);
        UIW.color = new Color32(171,171,171,255);

         // colour changer 
        switch (wind){
            case WindDirection.N:
                CompassNeedle.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                break;
            case WindDirection.W:
                CompassNeedle.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                break;
            case WindDirection.E:
                CompassNeedle.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 270f);
                break;
            default: // S
                CompassNeedle.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 180f);
                break;
        }
    }

    private void handleUnitActions(){
        foreach (var action in unitManager.getUnitActions()){
        // check all actions 
            TileBehaviour targetScript = action.Value.GetComponent<TileBehaviour>();
            UnitBehaviour unitScript = action.Key.GetComponent<UnitBehaviour>();
            if (action.Value != null && action.Key != null){
                if (targetScript.GetOccupyingUnit() && unitScript.getSupport()){
                // if the action is between two units 
                    unitManager.transferWater(action.Key, unitManager.GetUnitOnTile(action.Value));
                }
                else if (unitScript.getExtinguish() && (action.Value.name == "Fire" || action.Value.name == "Ember")){
                // if the action is an extinguish 
                    tileManager.Extinguish(action.Value);
                    unitScript.useWater(1);
                }
                else if (action.Value.name == "Water"){
                // if the action is to refill unit
                    unitManager.refillUnit(action.Key, action.Value);
                }
                else if (unitScript.getPreventative()){
                // if the action is a preventative 
                    if (action.Value.name != "Fire" && action.Value.name != "Water" && action.Value.name != "Ember" && action.Value.name != "Road"){
                        TileBehaviour tileScript = action.Value.GetComponent<TileBehaviour>();
                        Debug.Log(action.Value);
                        tileImmunities.Add(action.Value);
                        tileScript.applyFoam(5);
                        unitScript.useWater(1);
                    }
                }
            }
        }
    }

    private void checkTileImmunities(){
        List<GameObject> depleted = new List<GameObject>();

        foreach (GameObject tile in tileImmunities){
            TileBehaviour tileScript = tile.GetComponent<TileBehaviour>();
            if (tileScript != null){
                if (tileScript.getPrevent() > 0){
                    tileScript.depleteFoam();
                    tileScript.foamTile(true);
                }
                else{
                    tileScript.foamTile(false);
                    depleted.Add(tile);
                }
            }
        }

        foreach (GameObject tile in depleted){
            tileImmunities.Remove(tile);
        }
    }


    private void GameStateChanged(GameManager.GameState _state)
    {

        if (_state == GameManager.GameState.EnemyTurn)
        {
            // upon player ending turn, check for win/loss conditions
            Debug.Log("FireManager EnemyTurn");
            handleUnitActions();
            checkTileImmunities();
            if (tileManager.hasTurnsRemaining() || tileManager.GetFireTiles().Count > 0)
            {
                tileManager.SpreadFire((TileManager.WindDirection)wind);
                GameManager.Instance.UpdateGameState(GameManager.GameState.PreTurn, null);
            }
        }

        if (_state == GameManager.GameState.PreTurn)
        {
            Debug.Log("firemanager PreTurn");
            ChangeWindDirection();
            tileManager.DecayFire();
            Turn.text = "TURN " + numberOfTurns;
            Turn2.text = "TURN " + numberOfTurns;
            Heat.text = "HEAT " + tileManager.GetSpreadRate();
            Fire.text = "FIRE " + tileManager.GetFireTiles().Count;
            baseManager.CheckSpawn(numberOfTurns);
            GameManager.Instance.UpdateGameState(GameManager.GameState.PlayerTurn, null);
        }
    }
}