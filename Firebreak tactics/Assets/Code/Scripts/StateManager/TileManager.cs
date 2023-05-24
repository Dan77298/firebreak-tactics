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
        GameManager.OnGameStateChanged += GameStateChanged;
    }

    void OnDestroy(){
        GameManager.OnGameStateChanged -= GameStateChanged;
    }

    private void GameStateChanged(GameManager.GameState _state){
        if (_state == GameManager.GameState.ProduceTerrain){
            Debug.Log("tilemanager listening");
            GetTileObjects();
            SetNextFireTiles();
            Debug.Log("Ending state: PreTurn");
            GameManager.Instance.UpdateGameState(GameManager.GameState.PreTurn);
        }
    }

    public void GetTileObjects(){
        tiles.Clear();
        fireTiles.Clear();

        if (boardGrid != null){
            foreach (Transform child in boardGrid.transform){
                if (child.name == "Fire"){
                    fireTiles.Add(child.gameObject);
                }
                else if (child.name != "Dirt" && child.name != "Burned" && child.name != "Water"){
                    tiles.Add(child.gameObject);
                }
            }
        }
    }

    public void SetNextFireTiles(){ 
        Debug.Log("tiles: " + tiles.Count);
        Debug.Log("fireTiles: " + fireTiles.Count);
        Debug.Log("nextFireTiles: " + nextFireTiles.Count);
        nextFireTiles.Clear();
        foreach (GameObject tile in fireTiles){
            TileBehaviour tileBehaviour = tile.GetComponent<TileBehaviour>();
            List<GameObject> neighbouringTiles = tileBehaviour.GetNeighbouringTiles();
            foreach (GameObject _tile in neighbouringTiles){
                TileBehaviour tilescript = _tile.GetComponent<TileBehaviour>();
                if (tilescript != null && tilescript.CanOnFire()){
                    nextFireTiles.Add(_tile);                  
                }
            }
        }
    }

    public void DecayFire(){
        foreach (GameObject tile in fireTiles)
        {
           TileBehaviour tileBehaviour = tile.GetComponent<TileBehaviour>(); 
           if (tileBehaviour != null)
           {
                tileBehaviour.DecayTile();
                if (!tileBehaviour.GetOnFire())
                {
                    fireTiles.Remove(tile);
                }
           }
        }
    }

    public List<GameObject> GetFireTiles(){
        return fireTiles;
    }

    public List<GameObject> GetNextFireTiles(){
        return nextFireTiles;
    }

    public int GetSpreadRate(){
        return ((fireTiles.Count)/ 4 ) + 1;
    }

    public void SpreadFire(){
        
        for (int i=0; i < GetSpreadRate(); i++){
            int randomNumber = Random.Range(0, nextFireTiles.Count);
            GameObject selected = nextFireTiles[randomNumber];
            TileBehaviour tileBehaviour = selected.GetComponent<TileBehaviour>();
            if (tileBehaviour != null)
            {
                tileBehaviour.SetOnFire();
                fireTiles.Add(selected);
            }
        }  
    }

}