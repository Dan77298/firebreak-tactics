using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
    [SerializeField] public Grid grid;

    public List<List<GameObject>> gridXZ = new List<List<GameObject>>();
    public List<GameObject> FogTiles = new List<GameObject>();
    private Dictionary<GameObject, List<GameObject>> neighbourLookup = new Dictionary<GameObject, List<GameObject>>();


    void Awake()
    {
        initializeGrid();
        updateNeighbourLookup();
    }

    public enum WindDirection{
        N, E, S, W
    }

    public void initializeGrid(){
    // writes the [row][col] 2D array for all tiles
        List<Transform> tiles = new List<Transform>();
        int gridCols = 0;
        int gridRows = 0;

        foreach (Transform child in grid.transform)
        {
            //if tile
            if (child.tag == "Tile")
            {
                Vector3Int cellPos = grid.WorldToCell(new Vector3(child.transform.position.x, 0, child.transform.position.z));

                child.GetComponent<TileBehaviour>().cellPos = cellPos;

                if (cellPos.x >= 0 && cellPos.y >= 0)
                {
                    tiles.Add(child);

                    if (cellPos.x > gridCols)
                        gridCols = cellPos.x;

                    if (cellPos.y > gridRows)
                        gridRows = cellPos.y;
                }
            }
        }

        for (int x = 0; x < gridCols + 1; x++)
        {
            gridXZ.Add(new List<GameObject>());

            for (int y = 0; y < gridRows + 1; y++)
                gridXZ[x].Add(null);
        }

        //insert tiles into correct pos
        //assumes tileboard is complete but null values
        //can be checked to see if tiles are null
        foreach (Transform tile in tiles)
        {
            gridXZ[tile.GetComponent<TileBehaviour>().cellPos.x][tile.GetComponent<TileBehaviour>().cellPos.y] = tile.gameObject;
        }

        // for (int x = 0; x < gridXZ.Count; x++)
        // {
        //     //print("column " + x);

        //     for (int y = 0; y < gridXZ[x].Count; y++)
        //     {
        //         //print("row " + y);

        //         if (gridXZ[x][y] != null)
        //             print("gameObj " + gridXZ[x][y].name);
        //     }
        // }
        


        // Create all of the fog of war tiles in the positions of all of the tiles and add them to a list
        // foreach (Transform child in grid.transform)
        // {
        //     //if tile
        //     // if (child.tag == "Tile" || child.tag == "Water" || child.tag == "Fire")
        //     if (child.tag == "Tile")
        //     {
        //         Vector3Int cellPos = child.GetComponent<TileBehaviour>().cellPos;
        //         Vector3 worldPos = grid.GetCellCenterWorld(cellPos);
        //         GameObject fogTile = Instantiate(Resources.Load("Tiles/Fog1"), worldPos, Quaternion.identity) as GameObject;
        //         // set the rotation of the fog tile to match the tile
        //         fogTile.transform.rotation = child.transform.rotation;
        //         fogTile.transform.parent = child;
        //         FogTiles.Add(fogTile);
        //     }
        // }
    }

    public void updateNeighbourLookup(){
    // writes the lookup table for [row][col] of all tiles
        neighbourLookup.Clear();

        foreach (List<GameObject> row in gridXZ){
            foreach (GameObject tile in row){
                if (tile != null){
                    TileBehaviour tileBehavior = tile.GetComponent<TileBehaviour>();
                    List<GameObject> neighbours = getNeighbours(tileBehavior.cellPos);
                    neighbourLookup[tile] = neighbours;
                }
            }
        }
    }


    public List<List<GameObject>> getGridXZ(){
        return gridXZ;
    }

    public List<GameObject> getNeighbours(Vector3Int cellPos){
    // gets a gameObject list of all neighbour tiles to a tile's cellPos
        List<GameObject> neighbours = new List<GameObject>();

        // add neighbours by horizontal matches
        for (int x = cellPos.x - 1; x <= cellPos.x + 1; x++){
            int y = cellPos.y;
            if (x >= 0 && x < gridXZ.Count && y >= 0 && y < gridXZ[x].Count){
                GameObject neighbour = gridXZ[x][y];
                if (neighbour != null)
                {
                    neighbours.Add(neighbour);
                }
            }
        }

        // add neighbours by vertical matches
        for (int y = cellPos.y - 1; y <= cellPos.y + 1; y++){
            int x = cellPos.x;
            if (x >= 0 && x < gridXZ.Count && y >= 0 && y < gridXZ[x].Count){
                GameObject neighbour = gridXZ[x][y];
                if (neighbour != null)
                {
                    neighbours.Add(neighbour);
                }
            }
        }

        return neighbours;
    }

        public List<GameObject> getDirectionalNeighbours(Vector3Int cellPos, WindDirection wind){
    // gets a gameObject list of all neighbour tiles to a tile's cellPos
        List<GameObject> neighbours = new List<GameObject>();

        int ymin, ymax;
        int xmin, xmax;
        switch(wind){
            case WindDirection.N:
                xmin = cellPos.x;
                xmax = cellPos.x + 1;
                ymin = cellPos.y + 1;
                ymax = cellPos.y + 1;
                break;
            case WindDirection.E:
                xmin = cellPos.x + 1;
                xmax = cellPos.x + 1;
                ymin = cellPos.y - 1;
                ymax = cellPos.y + 1;
                break;
            case WindDirection.W:
                xmin = cellPos.x - 1;
                xmax = cellPos.x;
                ymin = cellPos.y - 1;
                ymax = cellPos.y + 1;
                break;
            default:
                xmin = cellPos.x;
                xmax = cellPos.x + 1;
                ymin = cellPos.y - 1;
                ymax = cellPos.y - 1;
                break;
        }

        // add neighbours by horizontal matches
        for (int y = ymin; y <= ymax; y++){
            int x = cellPos.x;
            if (x >= 0 && x < gridXZ.Count && y >= 0 && y < gridXZ[x].Count){
                GameObject neighbour = gridXZ[x][y];
                if (neighbour != null)
                {
                    neighbours.Add(neighbour);
                }
            }
        }

        // add neighbours by horizontal matches
        for (int x = xmin; x <= xmax; x++){
            int y = cellPos.y;
            if (x >= 0 && x < gridXZ.Count && y >= 0 && y < gridXZ[x].Count){
                GameObject neighbour = gridXZ[x][y];
                if (neighbour != null)
                {
                    neighbours.Add(neighbour);
                }
            }
        }
        return neighbours;
    }

    public void addLookup(GameObject _tile){
    // used to create a gridXZ lookup for neighbour tiles
        if (!neighbourLookup.ContainsKey(_tile)){
            TileBehaviour tileBehavior = _tile.GetComponent<TileBehaviour>();
            if (tileBehavior != null){
                List<GameObject> neighbours = getNeighbours(tileBehavior.cellPos);
                neighbourLookup[_tile] = neighbours;
            }
        }
    }

    public void removeLookup(GameObject _tile){
    // used to remove a gridXZ lookup for neighbour tiles
        if (neighbourLookup.ContainsKey(_tile)){
            neighbourLookup.Remove(_tile);
        }
    }

    public Dictionary<GameObject, List<GameObject>> getNeighbourLookup(){
    // get the lookup dictionary
        return neighbourLookup;
    }
}