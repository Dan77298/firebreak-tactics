using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    [SerializeField] private Grid unitsGrid;
    [SerializeField] private Grid tilesGrid;


    void Awake(){
        // make a unit spawner function with a spawn tile 

        initializeUnitPositions();
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

    public GameObject GetUnitOnTile(GameObject tile)
    {
        TileBehaviour tileScript = tile.GetComponent<TileBehaviour>();
        if (tileScript.IsOccupied())
        {
            return tileScript.GetOccupyingUnit();
        }
        return null;
    } 

    private GameObject getTile(GameObject unit)
    {
        Vector3Int cellPos = unit.GetComponent<UnitBehaviour>().getCellPos();
        
        return gridManager.getTile(cellPos);
    }

    public void moveUnitToTile(GameObject unit, GameObject newTile)
    {
        UnitBehaviour unitScript = unit.GetComponent<UnitBehaviour>();
        GameObject oldTile = unitScript.GetOccupyingTile();

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


}