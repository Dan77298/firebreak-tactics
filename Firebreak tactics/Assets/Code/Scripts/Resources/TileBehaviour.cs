using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileBehaviour : MonoBehaviour
{
    // code inheritance
    [SerializeField] private MaterialChanger materialChanger; // handles changing materials 

    public Grid grid;
    public Vector3Int cellPos;

    // tile states
    [SerializeField] private TileType tile; // what type of tile is it
    private TileType defaultTile; // what type of tile was it originally 
    private bool onFire = false; // is the tile burning
    private bool onEmber = false; // is the tile embering 
    private bool burned = false; // is the tile fully depleted naturally 
    private bool hasEmbered = false; // has the tile used its ember state 

    private Material tileMaterial; // the tile's default material 
    private Material displayMaterial; // the tile's displayed material 

    // tile traversal
    private int altitude = 1; // [1]low, [2]medium, [3]high 
    private int traversalCost; // cost of traversing the tile 
    private int traversalRule; // [1]all units, [2]ground only, [3]air only, [4] no traversal

    // tile health
    private int vegetation; // how much health a tile has (max health)  
    private int decay; // how much health a tile has left (remaining)
	

    public enum TileType{
        Grass1, // veg 1, 
        Grass2,
        Grass3,
        Dirt,
        Fire,
        Water,
        Ember,
        Burned
    }

    private void Awake(){
        ChangeTileState(tile); 
        SetDefaultState(tile);

        grid = transform.parent.GetComponent<Grid>();

        cellPos = grid.WorldToCell(new Vector3(transform.position.x, 0, transform.position.z));

    }

    private void SetDefaultState(TileType _tile){
        switch (_tile){
            case TileType.Grass1:
                vegetation = 1;
                traversalCost = 1;
                traversalRule = 1;
                displayMaterial = tileMaterial;
                onFire = false;
                hasEmbered = false;
                decay = 5;
                break;
            case TileType.Grass2:
                vegetation = 2;
                traversalCost = 1;
                traversalRule = 1;
                onFire = false;
                hasEmbered = false;
                decay = 6;
                break;
            case TileType.Grass3:
                vegetation = 3;
                traversalCost = 1;
                traversalRule = 1;
                onFire = false;
                hasEmbered = false;
                decay = 7;
                break;
            case TileType.Dirt:
                vegetation = 0;
                hasEmbered = true;
                traversalCost = 1;
                traversalRule = 1;
                onFire = false;
                hasEmbered = false;
                break;
            case TileType.Water:
                vegetation = 0;
                hasEmbered = true;
                traversalCost = 1;
                traversalRule = 3;
                onFire = false;
                hasEmbered = true;
                break;
            case TileType.Fire:
                onFire = true;
                vegetation = 0;
                decay = 6;
                traversalCost = 2;
                traversalRule = 3;
                hasEmbered = false;
                break;
            default:
                break;
        }
    }

    private void ChangeTileState(TileType _tile){
        // changes made to tile variables exclusively for switching states to/from ember/fire
        switch (_tile){
            case TileType.Grass1:
                onFire = false;
                break;
            case TileType.Grass2:
                onFire = false;
                break;
            case TileType.Grass3:
                onFire = false;
                break;
            case TileType.Dirt:
                vegetation = 0;
                hasEmbered = true;
                onFire = false;
                hasEmbered = true;
                break;
            case TileType.Fire:
                onFire = true;
                break;
            case TileType.Ember:
                onEmber = true;
                hasEmbered = true;
                break;
            case TileType.Burned:
                burned = true;
                decay = 0;
                onFire = false;
                vegetation = 0;
                break;
            default:
                break;
        }
    }

    public List<GameObject> GetNeighbouringTiles()
    {
        List<GameObject> neighbouringTiles = new List<GameObject>();

        float maxDistance = 0.8659f;
        Collider[] colliders = Physics.OverlapSphere(transform.position, maxDistance);
        foreach (Collider collider in colliders)
        {
            if (collider.gameObject != gameObject)
            {
                neighbouringTiles.Add(collider.gameObject);
            }
        }
        
        return neighbouringTiles;
    }


    public bool GetOnFire(){
        return this.onFire;
    }

    public bool GetOnEmber(){
        return this.onEmber;
    }

    public void SetOnFire(){
        if (CanOnFire()){
            // can tile be ignited
            tile = TileType.Fire;
            gameObject.name = "Fire";
            ChangeTileState(TileType.Fire);
            changeMaterial();
        }
    }

    public void SetOnEmber(){
        if (CanOnEmber()){
            // can tile be embered
            tile = TileType.Ember;
            gameObject.name = "Ember";
            ChangeTileState(TileType.Ember);
            changeMaterial();
        }
    }

    public void Extinguish(){
        // extinguish a burning tile
        if (GetOnFire() || GetOnEmber()){
            // rename the tile from the enum variable
            ChangeTileState(GetDefaultTile());
            changeMaterial();
        }
    }

    public TileType GetDefaultTile(){
        // returns the defaultTile
        return this.defaultTile;
    }

    public TileType GetTileType(){
        // returns the currentTile
        return this.tile;
    }

    public int GetVegetation(){
        return this.vegetation;
    }

    public int getDecay(){
        return this.decay;
    }
    public void DecayTile(){
        if (decay > 0){
           decay--; 
        }else{
            Debug.Log("TILE IS CONSUMED");
            burned = true;
            gameObject.name = "Burned";
            tile = TileType.Burned;
            ChangeTileState(tile);
            changeMaterial();
        } 
    }

    public void setToDirt(){
        tile = TileType.Dirt;
        ChangeTileState(tile);
        changeMaterial();
    }


    public bool CanOnFire(){
        if (!onFire && decay > 0){
        // if the tile isn't on fire, had fuel to begin with, and has fuel remaining 
            return true;
        }
        return false;
    }

    public bool CanOnEmber()
    {
        if (!onEmber && !onFire && hasEmbered && decay > 0){
        // if the tile isn't on fire, had fuel to begin with, hasn't been embered already, and has fuel remaining
            return true;
        }
        return false;
    }

    private void changeMaterial()
    {
        MaterialChanger.TileType materialType = (MaterialChanger.TileType)tile;
        materialChanger.SetTile(materialType);
    }
}