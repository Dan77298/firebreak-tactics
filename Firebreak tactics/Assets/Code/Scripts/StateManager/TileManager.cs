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

    public void highlightTile(GameObject tile){
    // sets a tile to highlighted
        if (tile != null){
            TileBehaviour script = tile.GetComponent<TileBehaviour>();
            if (script.GetTileType() == TileBehaviour.TileType.Highlighted){
                Debug.Log("false");
                script.SetHighlighted(false);
            }
            else{
                Debug.Log("true");
                script.SetHighlighted(true);
            }            
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

        if (fireTiles.Count <= 0){
        // victory by fire tiles depletion
            GameManager.Instance.UpdateGameState(GameManager.GameState.Victory);
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

    public List<GameObject> getDownBreezeTiles(WindDirection wind){
    // returns all ignitable tiles that are in wind direction of any fire tiles
        List<GameObject> spreadTiles = new List<GameObject>();
        if (fireTiles.Count > 0){
            foreach (GameObject fTile in fireTiles){
            // for all fire tiles
                TileBehaviour script = fTile.GetComponent<TileBehaviour>();
                List<GameObject> downbreezeNeighbours = gridManager.getDirectionalNeighbours(script.getCellPos(), (GridManager.WindDirection)wind);
                foreach (GameObject nTile in downbreezeNeighbours){
                // for all their adjacent tiles in direction of wind 
                    TileBehaviour nScript = nTile.GetComponent<TileBehaviour>();
                    if (nScript.CanOnFire() && !spreadTiles.Contains(nTile)){
                    // if the tile can be ignited and isn't already considered
                        spreadTiles.Add(nTile);
                    }
                }
            }
        }
        return spreadTiles;
    }

    public void SpreadFire(WindDirection wind){
        List<GameObject> igniteList = new List<GameObject>(); // prevents ignite piggybacking 
        int spreadRate = GetSpreadRate();

        if (fireTiles.Count > 0){
            // for all fires to be created
            List<GameObject> candidates = getDownBreezeTiles(wind);
            for (int i = 0; i < spreadRate; i++){
                // for every tile to be ignited
                GameObject selected = null;
                int adjacencies = 0;
                int vegetation = 0;
                foreach (GameObject cTile in candidates){
                    // for every candidate tile, find the one with the highest fire adjacencies 
                    TileBehaviour cScript = cTile.GetComponent<TileBehaviour>();
                    List<GameObject> cNeighbours = gridManager.getNeighbours(cScript.getCellPos());
                    if (!igniteList.Contains(cTile)){
                        if (selected == null){
                            selected = cTile;
                            adjacencies = getFireAdjacencies(cNeighbours);
                            vegetation = cScript.GetVegetation();
                        }
                        else
                        {
                            int cAdjacencies = getFireAdjacencies(cNeighbours);
                            int cVegetation = cScript.GetVegetation();
                            // change below for variation testing 
                            if (cAdjacencies <= adjacencies || cVegetation >= vegetation) 
                            {
                                // if the current candidate has more fire adjacencies or vegetation than the current selected 
                                selected = cTile;
                                adjacencies = cAdjacencies;
                                vegetation = cVegetation;
                            }
                        }
                    }
                }
                igniteList.Add(selected);
            }
            if (igniteList.Count > 0){
                foreach (GameObject newFire in igniteList)
                {
                    if (newFire)
                    {
                        TileBehaviour script = newFire.GetComponent<TileBehaviour>();
                        Debug.Log("Tile " + newFire.name + " set on fire.");
                        script.SetOnFire();
                        fireTiles.Add(newFire);
                    }
                }                
            }
        }
    }

    public bool hasTurnsRemaining(){
        foreach (GameObject tiles in fireTiles){
        // for all fire tiles
            TileBehaviour script = tiles.GetComponent<TileBehaviour>();
            List<GameObject> neighbours = gridManager.getNeighbours(script.getCellPos());
            foreach (GameObject nTile in neighbours){
            // for all neighbours to fire tiles
                TileBehaviour nScript = nTile.GetComponent<TileBehaviour>();
                if (nScript.CanOnFire())
                {
                    return true;
                }
            }
        }
        return false;
    }

    private int getFireAdjacencies(List<GameObject> list){
    // returns the number of fire adjacencies in a list of neighbour tiles
        int tally = 0;
        foreach (GameObject tile in list){
            if (tile.name == "Fire"){
                tally++;
            }
        }
        return tally;
    }

    // private GameObject SelectFireTile(WindDirection wind){
    //     // currently random, add wind direction factor
    //     int attempts = 0;

    //     while (attempts < fireTiles.Count)
    //     {
    //         int randomIndex = Random.Range(0, fireTiles.Count);
    //         GameObject fireTile = fireTiles[randomIndex];

    //         if (hasIgnitableNeighbours(fireTile, wind))
    //         {
    //             Debug.Log("Selected tile has ignitable neighbours");
    //             return fireTile;
    //         }
    //         else
    //         {
    //             Debug.Log("Selected tile " + fireTile.name + " does not have ignitable neighbors.");
    //         }

    //         attempts++;
    //     }

    //     Debug.Log("No suitable tile found to spread fire to.");
    //     return null;
    // }


    // private GameObject selectIgniteTile(GameObject fireTile, List<GameObject> excludeTiles, WindDirection wind){
    //     // tile selected based on wind
    //     TileBehaviour script = fireTile.GetComponent<TileBehaviour>();
    //     List<GameObject> neighbours = gridManager.getDirectionalNeighbours(script.getCellPos(),(GridManager.WindDirection)wind);
    //     List<GameObject> ignitableNeighbours = new List<GameObject>(); // neighbour candidates list

    //     if (neighbours != null){
    //         foreach (GameObject nTile in neighbours){
    //             // make a candidate list of all possible neighbours to spread to
    //             TileBehaviour nScript = nTile.GetComponent<TileBehaviour>();
    //             if (nScript.CanOnFire() && !excludeTiles.Contains(nTile)){
    //                 ignitableNeighbours.Add(nTile);
    //             }
    //         }

    //         if (ignitableNeighbours.Count > 0){
    //             int randomIndex = Random.Range(0, ignitableNeighbours.Count);
    //             return ignitableNeighbours[randomIndex];
    //         }
    //     }
    //     return null;
    // }

    // private bool hasIgnitableNeighbours(GameObject _tile, WindDirection wind)
    // {
    //     TileBehaviour script = _tile.GetComponent<TileBehaviour>();
    //     List<GameObject> nTiles = gridManager.getDirectionalNeighbours(script.getCellPos(),(GridManager.WindDirection)wind);
    //     //List<GameObject> nTiles = GetNeighbouringTiles(script.getCellPos());
    //     //Debug.Log("hasIgnitableNeighbours " + script.getCellPos() + " |  " + nTiles.Count);
    //     if (nTiles != null){
    //         foreach (GameObject nTile in nTiles)
    //         {
    //             TileBehaviour nScript = nTile.GetComponent<TileBehaviour>();
    //             if (nScript.CanOnFire())
    //             {
    //                 return true;
    //             }
    //         }
    //     }
    //     return false;
    // }

    // public void SpreadFire(WindDirection wind){
    //     List<GameObject> igniteList = new List<GameObject>(); // prevents ignite piggybacking 
    //     int spreadRate = GetSpreadRate();

    //     if (fireTiles.Count > 0)
    //     {
    //         GameObject fireTile = SelectFireTile(wind);
    //         if (fireTile != null)
    //         {
    //             GameObject selected = selectIgniteTile(fireTile, igniteList, wind); // prevents duplicate entries
    //             if (selected != null)
    //             {
    //                 igniteList.Add(selected);
    //             }
    //             else
    //             {
    //                 Debug.Log("No suitable ignitable neighbors found for tile " + fireTile.name);
    //             }
    //         }
    //         else
    //         {
    //             Debug.Log("No suitable tile found to spread fire from.");
    //             hasIgnitableTile = false;
    //             return; // no more tiles to ignite
    //         }
    //     }

    //     foreach (GameObject newFire in igniteList)
    //     {
    //         TileBehaviour script = newFire.GetComponent<TileBehaviour>();
    //         Debug.Log("Tile " + newFire.name + " set on fire.");
    //         script.SetOnFire();
    //         fireTiles.Add(newFire);
    //     }
    // }
}


