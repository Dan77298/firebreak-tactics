using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    [SerializeField] private Grid gridTiles;
    [SerializeField] private GridManager gridManager; 
    [SerializeField] private BaseManager baseManager; 

    private List<List<GameObject>> gridXZ;
    private List<GameObject> fireTiles = new List<GameObject>();
    
    private bool hasIgnitableTile = true;

    private int burnt = 0;
    private int fire = 0;
    private int forest1 = 0;
    private int forest2 = 0;
    private int forest3 = 0;
    private int water = 0; 

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
            Debug.Log("ProduceTerrain");
            updateFireTiles();
            gridManager.initializeGrid();
            gridManager.updateNeighbourLookup();
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
            //Debug.Log("fire removed from list. Remaining: " + fireTiles.Count);
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
        return Mathf.FloorToInt(1 + (fireTiles.Count / 7));
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
        bool ember = false;
        // roll a 75% chance to ember a tile
        int emberChance = Random.Range(0, 75);
        if (emberChance < 1000){
            ember = true;
            spreadRate = spreadRate - 1;
        }

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
                    
                    // with a 75% chance, select the tile with the highest z value
                    int randomChance = Random.Range(0, 100);
                    if (randomChance < 75){
                        // select the tile that has the highest z value
                        int cZ = cTile.GetComponent<TileBehaviour>().getCellPos().z;
                        if (!igniteList.Contains(cTile)){
                            if (selected == null){
                                selected = cTile;
                            if (cZ > adjacencies){
                                adjacencies = cZ;
                            }
                            }
                            else
                            {
                                if (cZ > adjacencies){
                                    selected = cTile;
                                    adjacencies = cZ;
                                }
                            }
                            
                        }
                    } else {

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
                                if (cAdjacencies <= adjacencies || cVegetation <= vegetation) 
                                {
                                    // if the current candidate has more fire adjacencies or vegetation than the current selected 
                                    selected = cTile;
                                    adjacencies = cAdjacencies;
                                    vegetation = cVegetation;
                                }
                            }
                        }
                    }


                }
                igniteList.Add(selected);
            }

            // select a viable ember candidate and add it to the ignite list
            if (ember){
                List<GameObject> emberCandidates = new List<GameObject>();
                List<GameObject> fireNeighbours = new List<GameObject>();
                // create a list of all tiles that are one tile away from a fire tile and are not already on fire or touching a fire tile
                // loop through all fire tiles and create a list of all their neighbours
                foreach (GameObject fTile in fireTiles){
                    // get the manager script of the fire tile
                    TileBehaviour fScript = fTile.GetComponent<TileBehaviour>();
                    // get the neighbours of the fire tile
                    List<GameObject> fNeighbours = gridManager.getNeighbours(fScript.getCellPos());
                    // add the neighbours of the fire tile to the list of fire neighbours
                    fireNeighbours.AddRange(fireNeighbours);
                foreach (GameObject nTile in fNeighbours){
                    // get the manager script of the neighbour tile
                    TileBehaviour nScript = nTile.GetComponent<TileBehaviour>();
                    // get the neighbours of the neighbour tile
                    List<GameObject> fnNeighbours = gridManager.getNeighbours(nScript.getCellPos());
                    // loop through all neighbours of neighbours of fire tiles and check if they are ember candidates
                    foreach (GameObject fnTile in fnNeighbours){
                        // get the manager script of the neighbour of neighbour tile
                        TileBehaviour fnScript = fnTile.GetComponent<TileBehaviour>();
                        // get the neighbours of the neighbour of neighbour tile
                        List<GameObject> emberNeighbours = gridManager.getNeighbours(fnScript.getCellPos());
                        // set the ember candidate to true as a default
                        bool emberCandidate = true;
                        // check if a tile is a neighbour of a fire tile
                        if (fireNeighbours.Contains(fnTile)){
                            // if the tile is a neighbour of a fire tile, it is not an ember candidate
                            emberCandidate = false;
                            break;
                        }
                        // check if the tile is already on fire or cannot be set on fire
                        if (fnScript.GetOnFire() || fnScript.CanOnFire() == false){
                            // if the tile is already on fire or cannot be set on fire, it is not an ember candidate
                            emberCandidate = false;
                            break;
                        }
                        // check if any of the tiles neighbours are on fire or are already in the ignite list
                        foreach (GameObject fnnTile in emberNeighbours){
                            TileBehaviour fnnScript = fnnTile.GetComponent<TileBehaviour>();
                            if (fnnScript.GetOnFire() || igniteList.Contains(fnnTile)){
                                emberCandidate = false;
                                break;
                            }
                        }
                        
                        if (emberCandidate){
                            emberCandidates.Add(fnTile);
                        }
                    }
                }
                }
                // log the positions of all ember candidates
                foreach (GameObject emberTile in emberCandidates){
                    Debug.Log("Ember candidate " + emberTile.name + " at " + emberTile.transform.position);
                }

                // select a random ember candidate and add it to the ignite list
                if (emberCandidates.Count > 0){
                    int randomIndex = Random.Range(0, emberCandidates.Count);
                    GameObject emberTile = emberCandidates[randomIndex];
                    igniteList.Add(emberTile);
                    // Debug.Log("Embered tile " + emberTile.name);
                    // log the position of the embered tile
                    Debug.Log("Embered tile " + emberTile.name + " at " + emberTile.transform.position);
                }
            }

            // ignite all tiles in the ignite list
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

    public void Extinguish(GameObject tile){
        List<GameObject> extinguishList = new List<GameObject>();

        foreach (GameObject fTile in fireTiles){
            if (fTile == tile){
                TileBehaviour script = fTile.GetComponent<TileBehaviour>();
                script.applyFoam(3);
                fireTiles.Remove(fTile);
                script.Extinguish();
                break;
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

    private void countTiles()
    {   
        burnt = 0;
        fire = 0;
        forest1 = 0;
        forest2 = 0;
        forest3 = 0;
        water = 0;
        
        foreach (Transform child in gridTiles.transform) {
            string name = child.gameObject.name;
            
            if (name == "Grass3") {
                forest3++;
            } else if (name == "Grass2") {
                forest2++;
            } else if (name == "Grass1") {
                forest1++;
            } else if (name == "Water") {
                water++;
            } else if (name == "Burned") {
                burnt++;
            } else if (name == "Fire") {
                fire++;
            }
        }
    }

    public int GetBurnt()
    {
        return burnt;
    }

    public int GetFire()
    {
        return fire;
    }

    public int GetForest1()
    {
        return forest1;
    }

    public int GetForest2()
    {
        return forest2;
    }

    public int GetForest3()
    {
        return forest3;
    }

    public int GetWater()
    {
        return water;
    }


}


