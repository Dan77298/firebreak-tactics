using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    [SerializeField] public Grid boardGrid;
    [SerializeField] private GridManager gridManager;
    
    private int burnt = 0;
    private int fire = 0;
    private int forest1 = 0;
    private int forest2 = 0;
    private int forest3 = 0;
    private int water = 0; 

    private List<GameObject> fireTiles;
    private List<GameObject> nextFireTiles;
    public List<List<GameObject>> gridXZ = new List<List<GameObject>>();
    private bool hasIgnitableTile = true;

    public enum WindDirection
    {
        N, E, S, W
    }

    void Awake()
    {
        fireTiles = new List<GameObject>();
        nextFireTiles = new List<GameObject>();
        GameManager.OnGameStateChanged += GameStateChanged;
    }

    void OnDestroy()
    {
        GameManager.OnGameStateChanged -= GameStateChanged;
    }

    private void GameStateChanged(GameManager.GameState _state)
    {
        if (_state == GameManager.GameState.ProduceTerrain)
        {
            SetTiles();
            GameManager.Instance.UpdateGameState(GameManager.GameState.PreTurn);
        }
        
        countTiles();
    }

    public void SetTiles()
    {
        List<Transform> tiles = new List<Transform>();
        int gridCols = 0;
        int gridRows = 0;

        foreach (Transform child in boardGrid.transform)
        {
            if (child.name == "Fire") fireTiles.Add(child.gameObject);
        }
    }
    
    public void DecayFire()
    {
        List<GameObject> depleted = new List<GameObject>();

        foreach (GameObject tile in fireTiles)
        {
            TileBehaviour script = tile.GetComponent<TileBehaviour>();
            script.DecayTile();
            if (!script.GetOnFire())
            {
                depleted.Add(tile);
            }
        }

        foreach (GameObject _tile in depleted)
        {
            Debug.Log("fire removed from list. Remaining: " + fireTiles.Count);
            fireTiles.Remove(_tile);
        }
    }

    public List<GameObject> GetFireTiles(){
        return fireTiles;
    }

    public List<GameObject> GetNextFireTiles(){
        return nextFireTiles;
    }

    public int GetSpreadRate()
    {
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
                GameObject fireTile = SelectFireTile();
                if (fireTile != null)
                {
                    GameObject selected = selectIgniteTile(fireTile, igniteList); // prevents duplicate entries
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

            foreach (GameObject iTile in igniteList)
            {
                TileBehaviour script = iTile.GetComponent<TileBehaviour>();
                Debug.Log("Tile " + iTile.name + " set on fire.");
                script.SetOnFire();
                fireTiles.Add(iTile);
            }
        }
    }

    private GameObject SelectFireTile(){
        // currently random, add wind direction factor
        int attempts = 0;

        while (attempts < fireTiles.Count)
        {
            int randomIndex = Random.Range(0, fireTiles.Count);
            GameObject fireTile = fireTiles[randomIndex];

            if (hasIgnitableNeighbours(fireTile))
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

    private GameObject selectIgniteTile(GameObject fireTile, List<GameObject> excludeTiles){
        // currently random, add wind direction factor
        TileBehaviour script = fireTile.GetComponent<TileBehaviour>();
        List<GameObject> neighbours = script.GetNeighbouringTiles();
        //List<GameObject> neighbours = GetNeighbouringTiles(script.getCellPos());
        //Debug.Log("selectIgniteTile " + script.getCellPos() + " |  " + neighbours.Count);
        List<GameObject> ignitableNeighbours = new List<GameObject>(); // neighbour candidates list
        if (neighbours != null){
            foreach (GameObject nTile in neighbours)
            {
                TileBehaviour nScript = nTile.GetComponent<TileBehaviour>();
                Debug.Log(nScript.getCellPos());
                if (nScript.CanOnFire() && !excludeTiles.Contains(nTile))
                {
                    ignitableNeighbours.Add(nTile);
                }
            }

            if (ignitableNeighbours.Count > 0)
            {
                int randomIndex = Random.Range(0, ignitableNeighbours.Count);
                return ignitableNeighbours[randomIndex];
            }
        }
        return null;
    }

    private bool hasIgnitableNeighbours(GameObject tile)
    {
        TileBehaviour script = tile.GetComponent<TileBehaviour>();
        List<GameObject> nTiles = script.GetNeighbouringTiles();
        //List<GameObject> nTiles = GetNeighbouringTiles(script.getCellPos());
        //Debug.Log("hasIgnitableNeighbours " + script.getCellPos() + " |  " + nTiles.Count);
        if (nTiles != null){
            foreach (GameObject nTile in nTiles)
            {
                TileBehaviour nScript = nTile.GetComponent<TileBehaviour>();
                Debug.Log(nScript.getCellPos());
                if (nScript.CanOnFire())
                {
                    return true;
                }
            }
        }
        return false;
    }

    public List<GameObject> GetNeighbouringTiles(Vector3Int cellPos)
{
    List<GameObject> neighbours = new List<GameObject>();

    int x = cellPos.x;
    int y = cellPos.z;

    if (x >= 0 && x < gridXZ.Count && y >= 0 && y < gridXZ[x].Count)
    {
        // NE
        if (x < gridXZ.Count - 1 && y < gridXZ[x + 1].Count && gridXZ[x + 1][y] != null)
            neighbours.Add(gridXZ[x + 1][y]);

        // NW
        if (x > 0 && y < gridXZ[x - 1].Count && gridXZ[x - 1][y] != null)
            neighbours.Add(gridXZ[x - 1][y]);

        // E
        if (x < gridXZ.Count - 1 && y < gridXZ[x].Count - 1 && gridXZ[x + 1][y + 1] != null)
            neighbours.Add(gridXZ[x + 1][y + 1]);

        // SE
        if (x < gridXZ.Count - 1 && y > 0 && gridXZ[x + 1][y - 1] != null)
            neighbours.Add(gridXZ[x + 1][y - 1]);

        // SW
        if (x > 0 && y > 0 && gridXZ[x - 1][y - 1] != null)
            neighbours.Add(gridXZ[x - 1][y - 1]);

        // W
        if (x > 0 && y < gridXZ[x].Count - 1 && gridXZ[x - 1][y + 1] != null)
            neighbours.Add(gridXZ[x - 1][y + 1]);
    }
    return neighbours;
}

private void countTiles()
{   
    burnt = 0;
    fire = 0;
    forest1 = 0;
    forest2 = 0;
    forest3 = 0;
    water = 0;
    
    foreach (Transform child in boardGrid.transform) {
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

// Getter methods for tile variables

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
