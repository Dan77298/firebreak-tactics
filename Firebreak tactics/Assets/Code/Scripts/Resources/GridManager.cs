using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] public Grid grid;

    public List<List<GameObject>> gridXZ = new List<List<GameObject>>();
    int gridCols;
    int gridRows;

    void Start()
    {
        //grid = GetComponent<Grid>();

        List<Transform> tiles = new List<Transform>();
        gridCols = 0;
        gridRows = 0;

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

        
            //Example list coords use
            //for (int x = 0; x < gridXZ.Count; x++)
            //{
                //print("column " + x);

            //    for (int y = 0; y < gridXZ[x].Count; y++)
            //    {
                    //print("row " + y);

            //        if (gridXZ[x][y] != null)
            //            print("gameObj " + gridXZ[x][y].name);
            //    }
            //}
         
    }

    public void PathfindingUnitTests()
    {
        
    }

    //A star pathfinding impl

    //Example use of FindPath fucntion
    //List<Vector2Int> path = FindPath(gridXZ[2][3].GetComponent<TileBehaviour>(), gridXZ[4][4].GetComponent<TileBehaviour>());

    //Unit type
    //0 == ground
    //1 == air
    public List<Vector2Int> FindPath(TileBehaviour startTile, TileBehaviour endTile, int unitType)
    {
        List<TileBehaviour> toSearch = new List<TileBehaviour> () { startTile };
        List<TileBehaviour> processed = new List<TileBehaviour>();

        (int, int)[] neighbourRelativeCoords = new (int, int)[] { (-1, 0), (0, 1), (0, -1), (1, 1), (1, 0), (1, -1) };

        while (toSearch.Count > 0)
        {

            //find next tile to process
            //swap to remove indexing for optimisation
            TileBehaviour current = toSearch[0];
            foreach (TileBehaviour tile in toSearch)
            {
                if (tile.F < current.F || tile.F == current.F && tile.H < current.H)
                    current = tile;
            }


            //print(current.cellPos);

            //move lists
            processed.Add(current);
            toSearch.Remove(current);

            if (current == endTile) break;


            //for each neighbour
            foreach ((int, int) relPos in neighbourRelativeCoords)
            {

                //calculate neighbour pos and get
                Vector2Int nCoords = new Vector2Int(current.cellPos.x + relPos.Item1, current.cellPos.y + relPos.Item2);

                //coords in bounds
                if (nCoords.x < 0 || nCoords.x >= gridCols || nCoords.y < 0 || nCoords.y >= gridRows)
                    continue;

                GameObject obj = gridXZ[nCoords.x][nCoords.y];

                //not null
                if (obj == null)
                    continue;

                TileBehaviour neighbour = obj.GetComponent<TileBehaviour>();

                //if tile is traversable
                if (neighbour.GetTraversalRule() == 4 || processed.Contains(neighbour)
                    || unitType == 0 && neighbour.GetTraversalRule() == 3
                    || unitType == 1 && neighbour.GetTraversalRule() == 2
                    )
                    continue;

                bool inToSearch = toSearch.Contains(neighbour);

                //1 is representative of distance cost
                //replace with traversal cost of tile
                float costToNeighbour = current.G + 1 + current.GetTraversalCost();

                //if not searched or a cheaper travel cost for neighbour has been found
                if (!inToSearch || costToNeighbour < neighbour.G) 
                {
                    //update cost of neighbour
                    neighbour.SetG(costToNeighbour);
                    neighbour.SetConnection(current);

                    if (!inToSearch)
                    {
                        neighbour.SetH(neighbour.GetDistance(endTile));
                        toSearch.Add(neighbour);
                    }

                }

            }
        }

        List<Vector2Int> output = new List<Vector2Int>();

        TileBehaviour outCurrent = endTile;

        while (outCurrent != startTile)
        {
            output.Insert(0, new Vector2Int(outCurrent.cellPos.x, outCurrent.cellPos.y));

            outCurrent = outCurrent.connection;
        }

        output.Insert(0, new Vector2Int(outCurrent.cellPos.x, outCurrent.cellPos.y));

        return output;
    }
}