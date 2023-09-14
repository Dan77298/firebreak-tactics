using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    [SerializeField] private Grid unitsGrid;
    [SerializeField] private Grid tilesGrid;

    private Dictionary<GameObject, GameObject> unitActions = new Dictionary<GameObject, GameObject>();

    void Awake(){ 
        GameManager.OnGameStateChanged += GameStateChanged;
    }

    void OnDestroy(){
    // remove state listener when game is finished
        GameManager.OnGameStateChanged -= GameStateChanged;
    }

    public Dictionary<GameObject, GameObject> getActions(){
        return unitActions;
    }

    private void initializeUnitPositions(){
       
        foreach (Transform unitTransform in unitsGrid.transform){
        // for all units       
            if (unitTransform.tag == "Unit"){
                GameObject unit = unitTransform.gameObject;
                moveUnit(unit);
            }
        }
    }

    public GameObject GetUnitOnTile(GameObject tile){

        TileBehaviour tileScript = tile.GetComponent<TileBehaviour>();
        if (tileScript.IsOccupied())
        {
            return tileScript.GetOccupyingUnit();
        }
        return null;
    } 

    public Dictionary<GameObject, GameObject> getUnitActions(){
        return unitActions;
    }

    private GameObject getTile(GameObject unit){
        Vector3Int cellPos = unit.GetComponent<UnitBehaviour>().getCellPos();
        return gridManager.getTile(cellPos);
    }

    public bool duplicateAction(GameObject target){
    // returns whether the target is already a set action 
        foreach (var action in unitActions)
        {
            if (action.Key == target || action.Value == target)
            {
                return true;
            }
        }
        return false;
    }

    public void requestCancel(GameObject target){
        TileBehaviour script = target.GetComponent<TileBehaviour>();
        List<GameObject> queueRemove = new List<GameObject>();

        if (target != null && script != null){
            foreach (var action in unitActions){
                if (action.Key == target || action.Value == target){
                    queueRemove.Add(action.Key);
                }
                else if (script.GetOccupyingUnit() != null){
                    // Check if action.Key and action.Value are equal to script.GetOccupyingUnit() without null checks
                    if (action.Key == script.GetOccupyingUnit() || action.Value == script.GetOccupyingUnit()){
                        queueRemove.Add(action.Key);
                    }
                }
            }

            foreach (var entry in queueRemove){
                Debug.Log("Action canceled");
                unitActions.Remove(entry);
            }
        }
    }

    private void setAction(GameObject unit, GameObject target){
    // Check if the x GameObject already exists as a key in the dictionary.
        if (!unitActions.ContainsKey(unit)){
        // if unit action is new
            Debug.Log("new action: " + unit + ", " + target);
            unitActions.Add(unit, target);
        }
        else{
        // if unit already has an action 
            Debug.Log("update action: " + unit);
            unitActions[unit] = target;
        }
    }

    public void interactTile(GameObject unit, GameObject target){
    // issue an action for the unit to that tile 
        if (unit.GetComponent<UnitBehaviour>().getPreventative() && !duplicateAction(target)){
            setAction(unit,target);
        }
    }

    public void interactFire(GameObject unit, GameObject target){
    // issue an action for the unit to that tile
       if (unit.GetComponent<UnitBehaviour>().getExtinguish() && !duplicateAction(target)){
            setAction(unit,target);
        } 
    }

    public void interactUnit(GameObject unit, GameObject target){
    // issue an action for the unit to that unit
        if (!duplicateAction(target)){
        //isSupport() is checked when determining click 
            setAction(unit, target);
        }
    }

    public void refillUnit(GameObject unit, GameObject waterTile){
    // unit uses their action to get more water 
        UnitBehaviour unitScript = unit.GetComponent<UnitBehaviour>();
        TileBehaviour tileScript = waterTile.GetComponent<TileBehaviour>();

        unitScript.refillWater();
        tileScript.depleteWater();
    }

    public void transferWater(GameObject tanker, GameObject target){
    // transfer water from tanker to target 
        UnitBehaviour tankerScript = tanker.GetComponent<UnitBehaviour>();
        UnitBehaviour targetScript = target.GetComponent<UnitBehaviour>();

        if (targetScript.getWater() < targetScript.getCapacity() && tankerScript.getWater() > 0){
        // if the tanker has water and the target doesnt have full water
            
            int amount = Mathf.Min(tankerScript.getWater(), (targetScript.getCapacity() - targetScript.getWater()));
            Debug.Log("amount: " + amount);
            // Transfer water from the tanker to the target
            tankerScript.useWater(amount);
            targetScript.refillWater();
        }
    }

    public void moveUnitToTile(GameObject unit, GameObject newTile){
    // used when issuing a move command 
        UnitBehaviour unitScript = unit.GetComponent<UnitBehaviour>();
        GameObject oldTile = unitScript.GetOccupyingTile();

        // remove any actions associated with this move 
        requestCancel(unit);
        requestCancel(oldTile);
        requestCancel(newTile);

        if (oldTile != null){
            oldTile.GetComponent<TileBehaviour>().SetOccupyingUnit(null);
        }

        if (newTile != null){
            newTile.GetComponent<TileBehaviour>().SetOccupyingUnit(unit);
        }

        unitScript.SetOccupyingTile(newTile);
    }

    public void moveUnit(GameObject unit){
    // Update the occupying state of the old tile, new tile, and the unit
    // used when initialising 
        UnitBehaviour unitScript = unit.GetComponent<UnitBehaviour>();
        GameObject newTile = getTile(unit);
        GameObject oldTile = unitScript.GetOccupyingTile();

        if (oldTile != null){
            oldTile.GetComponent<TileBehaviour>().SetOccupyingUnit(null);
        }

        if (newTile != null){
            newTile.GetComponent<TileBehaviour>().SetOccupyingUnit(unit);
        }

        unitScript.SetOccupyingTile(newTile);
    }

    private void GameStateChanged(GameManager.GameState newState){
    // reset unitActions at start of turn 
        Debug.Log(newState);
        if (newState == GameManager.GameState.PreTurn)
        {
            initializeUnitPositions();
            unitActions.Clear();
        }
        else if (newState == GameManager.GameState.PlayerTurn)
        {
            //reset all unit movement, actions, etc.
            ResetUnitStats();
        }
    }

    public void ResetUnitStats()
    {
        foreach (Transform unitTransform in unitsGrid.transform)
        {
            // for all units       
            if (unitTransform.tag == "Unit")
            {
                UnitBehaviour unit = unitTransform.gameObject.GetComponent<UnitBehaviour>();

                unit.SetMovements(unit.GetMaxMovements());
            }
        }
    }
}