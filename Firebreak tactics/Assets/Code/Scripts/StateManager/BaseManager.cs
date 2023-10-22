using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseManager : MonoBehaviour
{
    [SerializeField] private GameObject unitBase;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private UnitManager unitManager;
    [SerializeField] private Grid unitGrid;

    [SerializeField] private GameObject foamPrefab;  
    [SerializeField] private GameObject scoutPrefab;  
    [SerializeField] private GameObject spotterPrefab;  
    [SerializeField] private GameObject strikerPrefab;
    [SerializeField] private GameObject tankerPrefab;
    [SerializeField] private GameObject transportPrefab;      

    private List<GameObject> SpawnList = new List<GameObject>();

    private Dictionary<int, UnitType> spawnTable = new Dictionary<int, UnitType>
    {
        { 6, UnitType.Tanker },
        { 3, UnitType.Striker },
        { 5, UnitType.Foam },
        { 8, UnitType.Spotter },
        { 10, UnitType.Scout },
        { 12, UnitType.Transport }
    };

    private enum UnitType
    {
        Foam,
        Scout,
        Spotter,
        Striker,
        Tanker,
        Transport
    }

private void trySpawn(){
    Debug.Log("trySpawn");
    TileBehaviour baseScript = unitBase.GetComponent<TileBehaviour>();
    List<GameObject> spawningTiles = gridManager.getNeighbours(baseScript.getCellPos());

    if (SpawnList.Count > 0){
        foreach (GameObject tile in spawningTiles){
            TileBehaviour tileScript = tile.GetComponent<TileBehaviour>();

            if (tileScript != null && !tileScript.GetOccupyingUnit() && tile != unitBase){

                GameObject unitPrefab = Instantiate(SpawnList[0], unitGrid.transform);
                unitManager.CenterUnitToTile(unitPrefab, tile);
                tileScript.SetOccupyingUnit(unitPrefab);
                unitPrefab.name = SpawnList[0].name;
                SpawnList.RemoveAt(0);
                break; 
            }
        }
    }
}


    public void CheckSpawn(int turn){
        if (spawnTable.TryGetValue(turn, out UnitType unitType)){

            switch (unitType){
                case UnitType.Foam:
                    SpawnList.Add(foamPrefab);
                    break;
                case UnitType.Scout:
                    SpawnList.Add(scoutPrefab);
                    break;
                case UnitType.Spotter:
                    SpawnList.Add(spotterPrefab);
                    break;
                case UnitType.Striker:
                    SpawnList.Add(strikerPrefab);
                    break;
                case UnitType.Tanker:
                    SpawnList.Add(tankerPrefab);
                    break;
                case UnitType.Transport:
                    SpawnList.Add(transportPrefab);
                    break;

                default:
                    break;
            }
        }
        trySpawn();
    }
}
