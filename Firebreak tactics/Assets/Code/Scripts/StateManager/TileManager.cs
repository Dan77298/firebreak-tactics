using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour{
    [SerializeField] public GameObject boardGrid;
    private List<GameObject> tiles;
    private List<GameObject> fireTiles;
    private List<GameObject> nextFireTiles;

    void Awake(){
        tiles = new List<GameObject>();
        fireTiles = new List<GameObject>();
        nextFireTiles = new List<GameObject>();
        GetTileObjects();
    }

    private void GetTileObjects(){
        tiles.Clear();
        fireTiles.Clear();
        nextFireTiles.Clear();

        if (boardGrid != null)
        {
            foreach (Transform child in boardGrid.transform)
            {
                tiles.Add(child.gameObject);
            }
        }
    }
    public List<GameObject> GetFireTiles(){
        return fireTiles;
    }

    public void SpreadFire(){
        
    }

}