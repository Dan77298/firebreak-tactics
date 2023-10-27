using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    [SerializeField] private Grid unitsGrid;
    [SerializeField] private Grid tilesGrid;

    private Dictionary<GameObject, List<GameObject>> unitActions = new Dictionary<GameObject, List<GameObject>>();

    void Awake(){ 
        GameManager.OnGameStateChanged += GameStateChanged;
    }

    void OnDestroy(){
    // remove state listener when game is finished
        GameManager.OnGameStateChanged -= GameStateChanged;
    }

    public Dictionary<GameObject, List<GameObject>> getActions(){
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

    public Dictionary<GameObject, List<GameObject>> getUnitActions(){
        return unitActions;
    }

    private GameObject getTile(GameObject unit){
        Vector3Int cellPos = unit.GetComponent<UnitBehaviour>().getCellPos();
        return gridManager.getTile(cellPos);
    }

    public void CenterUnitToTile(GameObject unit, GameObject tile){
        Debug.Log("centerunit");

        Vector3 tilePosition = tile.transform.position;
        Vector3Int cellPos = unitsGrid.WorldToCell(tilePosition); 

        Vector3 gridPosition = unitsGrid.transform.position;
        Vector3 adjustedNewPosition = unitsGrid.GetCellCenterWorld(cellPos) + new Vector3(gridPosition.x+0.1f, 0f, gridPosition.z-0.3f);

        adjustedNewPosition.y = unit.transform.position.y;

        unit.transform.position = adjustedNewPosition;
    }

    public bool duplicateAction(GameObject unit, GameObject target){
        // returns whether the target is already a set action 

        if (unitActions.ContainsKey(unit)){
            List<GameObject> targetList = unitActions[unit];
            if (targetList.Contains(target)){
                return true;
            }
        }
        return false;
    }

    public bool canRefill(GameObject unit){
        // Get the tile position of the unit
        Debug.Log("try refill");
        UnitBehaviour unitScript = unit.GetComponent<UnitBehaviour>();
        Vector3Int tilePos = unitScript.GetOccupyingTile().GetComponent<TileBehaviour>().getCellPos();
        Debug.Log(tilePos);
        // Get neighboring tiles
        List<GameObject> neighbours = gridManager.getNeighbours(tilePos);
        foreach (GameObject tile in neighbours){
            Debug.Log(tile.name);
            if (tile.name == "Water"){  
                Debug.Log("water neighbour");
                TileBehaviour waterTile = tile.GetComponent<TileBehaviour>();
                if (waterTile.getCapacity() > 0 && unitScript.getWater() < unitScript.getCapacity() && !unitScript.getFillingWater()){
                    // if the water source is not empty, the unit can be refilled, and unit is not refilling already
                    return true;
                }
            }
        }
        return false; // The unit cannot refill water.
    }

    private void setAction(GameObject unit, GameObject target){
        // adds action to list 
        UnitBehaviour unitScript = unit.GetComponent<UnitBehaviour>();

        if (!unitActions.ContainsKey(unit)){
            unitActions[unit] = new List<GameObject>();
        }

        // Add the target to the list associated with the unit.
        unitActions[unit].Add(target);

        Debug.Log("new action: " + unit + ", " + target);
        unitScript.useAction();
    }

    public void interactTile(GameObject unit, GameObject target){
    // issue an action for the unit to that tile 
        UnitBehaviour unitScript = unit.GetComponent<UnitBehaviour>();

        if (target.name == "Water" && !duplicateAction(unit, target) && !unitScript.getFillingWater()){
            // refill unit from water tile, only allow one refill per unit
            target.GetComponent<TileBehaviour>().foamTile(true);
            unitScript.fillingWater(true);
            setAction(unit,target);
        }
        else if (unit.GetComponent<UnitBehaviour>().getPreventative() && !duplicateAction(unit, target)){
            // foam a tile
            target.GetComponent<TileBehaviour>().foamTile(true);
            setAction(unit,target);
        }
    }

    public void interactFire(GameObject unit, GameObject target){
    // issue an action for the unit to that tile
       if (unit.GetComponent<UnitBehaviour>().getExtinguish() && !duplicateAction(unit, target)){
            TileBehaviour tile = target.GetComponent<TileBehaviour>();
            tile.highlightFireTile(true);
            setAction(unit,target);
        } 
    }

    public void interactUnit(GameObject unit, GameObject target){
    // issue an action for the unit to that unit
        if (!duplicateAction(unit, target)){
        //isSupport() is checked when determining click 
            TileBehaviour tile = target.GetComponent<TileBehaviour>();
            tile.foamTile(true);
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
        Debug.Log("moveUnitToTile");
    // used when issuing a move command 
        UnitBehaviour unitScript = unit.GetComponent<UnitBehaviour>();
        GameObject oldTile = unitScript.GetOccupyingTile();

        // remove any actions associated with this move 

        if (oldTile != null){
            oldTile.GetComponent<TileBehaviour>().SetOccupyingUnit(null);
        }

        if (newTile != null){
            newTile.GetComponent<TileBehaviour>().SetOccupyingUnit(unit);
        }

        unitScript.SetOccupyingTile(newTile);
    }

    public void moveUnit(GameObject unit){
        Debug.Log("moveUnit");
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
        //Debug.Log(newState);
        if (newState == GameManager.GameState.PreTurn)
        {
            initializeUnitPositions();
            ResetUnitStats();
            unitActions.Clear();
        }
    }

    public void ResetUnitStats(){
        // called before player turn
        foreach (Transform unitTransform in unitsGrid.transform)
        {
            // for all units       
            if (unitTransform.tag == "Unit")
            {
                UnitBehaviour unit = unitTransform.gameObject.GetComponent<UnitBehaviour>();
                unit.resetActions();
                unit.fillingWater(false);
                unit.SetMovements(unit.GetMaxMovements());
            }
        }
    }
}