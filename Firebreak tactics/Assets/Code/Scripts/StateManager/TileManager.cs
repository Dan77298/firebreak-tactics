using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    [SerializeField] private Grid gridTiles;
    [SerializeField] private GridManager gridManager; 

    private List<List<GameObject>> gridXZ;
    private List<GameObject> fireTiles = new List<GameObject>();
    
    private bool hasIgnitableTile = true;

    public enum WindDirection{
        N, E, S, W
    }

    void Awake(){ 
        GameManager.OnGameStateChanged += GameStateChanged;
    }

    void Start(){
        gridXZ = gridManager.getGridXZ();
    }

    void OnDestroy(){
    // remove state listener when game is finished
        GameManager.OnGameStateChanged -= GameStateChanged;
    }

    private void GameStateChanged(GameManager.GameState _state){
    // all actions before the game gives the player their first turn
        if (_state == GameManager.GameState.ProduceTerrain)
        {
            updateFireTiles();
            GameManager.Instance.UpdateGameState(GameManager.GameState.PreTurn);
        }
    }

    public void updateFireTiles(){
    // write a list of all tiles named "Fire"
        foreach (Transform child in gridTiles.transform){
            if (child.name == "Fire") fireTiles.Add(child.gameObject);
        }
    }
    
    public void DecayFire(){
        Debug.Log("DecayFire");
        List<GameObject> depleted = new List<GameObject>();

        foreach (GameObject tile in fireTiles){
        // queue all depleted fires for removal
            TileBehaviour script = tile.GetComponent<TileBehaviour>();
            script.decayTile();
            if (tile.name != "Fire"){
                depleted.Add(tile);
            }
        }

        foreach (GameObject tile in depleted){
            Debug.Log("fire removed from list. Remaining: " + fireTiles.Count);
            fireTiles.Remove(tile);
        }
    }

    public List<GameObject> GetFireTiles(){
        return fireTiles;
    }

    public int GetSpreadRate(){
        return Mathf.FloorToInt(2 + (fireTiles.Count / 7));
    }

    public bool hasIgnitableTiles(){
        return hasIgnitableTile;
    }

    public void SpreadFire(WindDirection wind){
        List<GameObject> igniteList = new List<GameObject>(); // prevents ignite piggybacking 
        int spreadRate = GetSpreadRate();

        if (fireTiles.Count > 0)
        {
            for (int i = 0; i < spreadRate; i++)
            {
                GameObject fireTile = SelectFireTile(wind);
                if (fireTile != null)
                {
                    GameObject selected = selectIgniteTile(fireTile, igniteList, wind); // prevents duplicate entries
                    if (selected != null)
                    {
                        igniteList.Add(selected);
                    }
                    else
                    {
                        Debug.Log("No suitable ignitable neighbors found for tile " + fireTile.name);
                    }
                }
                else
                {
                    Debug.Log("No suitable tile found to spread fire from.");
                    hasIgnitableTile = false;
                    return; // no more tiles to ignite
                }
            }

            foreach (GameObject newFire in igniteList)
            {
                TileBehaviour script = newFire.GetComponent<TileBehaviour>();
                Debug.Log("Tile " + newFire.name + " set on fire.");
                script.SetOnFire();
                fireTiles.Add(newFire);
            }
        }
    }

    private GameObject SelectFireTile(WindDirection wind){
        // currently random, add wind direction factor
        int attempts = 0;

        while (attempts < fireTiles.Count)
        {
            int randomIndex = Random.Range(0, fireTiles.Count);
            GameObject fireTile = fireTiles[randomIndex];

            if (hasIgnitableNeighbours(fireTile, wind))
            {
                Debug.Log("Selected tile has ignitable neighbours");
                return fireTile;
            }
            else
            {
                Debug.Log("Selected tile " + fireTile.name + " does not have ignitable neighbors.");
            }

            attempts++;
        }

        Debug.Log("No suitable tile found to spread fire to.");
        return null;
    }

    private GameObject selectIgniteTile(GameObject fireTile, List<GameObject> excludeTiles, WindDirection wind){
        // tile selected based on wind
        TileBehaviour script = fireTile.GetComponent<TileBehaviour>();
        List<GameObject> neighbours = gridManager.getDirectionalNeighbours(script.getCellPos(),(GridManager.WindDirection)wind);
        List<GameObject> ignitableNeighbours = new List<GameObject>(); // neighbour candidates list

        if (neighbours != null){
            foreach (GameObject nTile in neighbours){
                // make a candidate list of all possible neighbours to spread to
                TileBehaviour nScript = nTile.GetComponent<TileBehaviour>();
                if (nScript.CanOnFire() && !excludeTiles.Contains(nTile)){
                    ignitableNeighbours.Add(nTile);
                }
            }
            
            if (ignitableNeighbours.Count > 0){
                int randomIndex = Random.Range(0, ignitableNeighbours.Count);
                return ignitableNeighbours[randomIndex];
            }
        }
        return null;
    }

    private bool hasIgnitableNeighbours(GameObject _tile, WindDirection wind)
    {
        TileBehaviour script = _tile.GetComponent<TileBehaviour>();
        List<GameObject> nTiles = gridManager.getDirectionalNeighbours(script.getCellPos(),(GridManager.WindDirection)wind);
        //List<GameObject> nTiles = GetNeighbouringTiles(script.getCellPos());
        //Debug.Log("hasIgnitableNeighbours " + script.getCellPos() + " |  " + nTiles.Count);
        if (nTiles != null){
            foreach (GameObject nTile in nTiles)
            {
                TileBehaviour nScript = nTile.GetComponent<TileBehaviour>();
                if (nScript.CanOnFire())
                {
                    return true;
                }
            }
        }
        return false;
    }
}
