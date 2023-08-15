using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public Grid grid;

    public List<List<GameObject>> gridXZ = new List<List<GameObject>>();


    void Awake()
    {
        grid = GetComponent<Grid>();

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

        /*
            //Example list coords use
            for (int x = 0; x < gridXZ.Count; x++)
            {
                //print("column " + x);

                for (int y = 0; y < gridXZ[x].Count; y++)
                {
                    //print("row " + y);

                    if (gridXZ[x][y] != null)
                        print("gameObj " + gridXZ[x][y].name);
                }
            }
         */
    }

}
