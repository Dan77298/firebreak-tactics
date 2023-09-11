using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileBehaviour : MonoBehaviour
{
    // code inheritance
    [SerializeField] private MaterialChanger materialChanger; // handles changing materials 
    
    // tile states
    [SerializeField] private TileType tile; // what type of tile is it
    private TileType defaultTile; // what type of tile was it originally 
    private GameObject occupyingUnit = null;
    private bool onFire = false; // is the tile burning
    private bool onEmber = false; // is the tile embering 
    private bool burned = false; // is the tile fully depleted naturally 
    private bool hasEmbered = false; // has the tile used its ember state 
    private int prevent = 0; // how many turns of preventative the tile has 
    private int capacity = 0; // for water tiles 
    [SerializeField] private Vector3Int cellPos;

    private Material tileMaterial; // the tile's default material 
    private Material displayMaterial; // the tile's displayed material 

    // tile traversal
    private int altitude = 1; // [1]low, [2]medium, [3]high 
    private int traversalCost; // cost of traversing the tile 
    private int traversalRule; // [1]all units, [2]ground only, [3]air only, [4] no traversal
    private Grid grid;

    // tile health
    private int vegetation; // how much health a tile has (max health)  
    private int decay; // how much health a tile has left (remaining)
	
    //pathfinding variables
    public TileBehaviour connection { get; private set; }
    public float G { get; private set; }
    public float H { get; private set; }
    public float F => G + H;

    public void SetConnection(TileBehaviour tile) => connection = tile;
    public void SetG(float g) => G = g;
    public void SetH(float h) => H = h;

    public enum TileType{
        Grass1,
        Grass2,
        Grass3,
        Dirt,
        Fire,
        Water,
        Ember,
        Burned,
        Highlighted,
        Selected,
        Road,
        // add more here
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
                defaultTile = TileType.Grass1;
                vegetation = 1;
                traversalCost = 1;
                traversalRule = 1;
                displayMaterial = tileMaterial;
                onFire = false;
                hasEmbered = false;
                decay = 5;
                break;
            case TileType.Grass2:
                defaultTile = TileType.Grass2;
                vegetation = 2;
                traversalCost = 1;
                traversalRule = 1;
                onFire = false;
                hasEmbered = false;
                decay = 6;
                break;
            case TileType.Grass3:
                defaultTile = TileType.Grass3;
                vegetation = 3;
                traversalCost = 1;
                traversalRule = 1;
                onFire = false;
                hasEmbered = false;
                decay = 7;
                break;
            case TileType.Dirt:
                defaultTile = TileType.Dirt;
                vegetation = 0;
                hasEmbered = true;
                traversalCost = 1;
                traversalRule = 1;
                onFire = false;
                hasEmbered = false;
                break;
            case TileType.Water:
                defaultTile = TileType.Water;
                vegetation = 0;
                capacity = 3;
                hasEmbered = true;
                traversalCost = 1;
                traversalRule = 3;
                onFire = false;
                hasEmbered = true;
                break;
            case TileType.Fire:
                defaultTile = TileType.Fire;
                onFire = true;
                vegetation = 0;
                decay = 6;
                traversalCost = 2;
                traversalRule = 3;
                hasEmbered = false;
                break;
            case TileType.Road:
                defaultTile = TileType.Road;
                decay = 0;
                traversalCost = 0;
                traversalRule = 1;
                break;
            // add more here
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
            case TileType.Highlighted:
                break;
            case TileType.Selected:
                break;
            case TileType.Road:
                decay = 0;
                onFire = false;
                break;
            // add more here
            default:
                break;
        }
    }

    public void depleteWater(){
        capacity--;
        if (capacity <= 0){
            consumeTile();
        }
    }

    public int getCapacity(){
        return capacity;
    }

    public void highlightTile(bool highlight){
    // sets highlighted on a tile
        if (highlight){
            tile = TileType.Highlighted; 
        }
        else{
            tile = GetDefaultTile(); 
        }
        changeMaterial();
    }

    public void selectTile(bool select){
    // sets highlighted on a tile
        if (select){
            tile = TileType.Selected; 
        }
        else{
            tile = GetDefaultTile(); 
        }
        changeMaterial();
    }

    public bool IsOccupied(){
        return occupyingUnit != null;
    }

    public GameObject GetOccupyingUnit(){
        return occupyingUnit;
    }

    public void SetOccupyingUnit(GameObject unit){
        occupyingUnit = unit;
    }

    public Vector3Int getCellPos(){
        return grid.WorldToCell(new Vector3(transform.position.x, 0, transform.position.z));
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
            tile = TileType.Burned;
            ChangeTileState(TileType.Burned);
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
    public void decayTile(){
        if (decay > 0){
           decay--; 
        }else{
            consumeTile();
        } 
    }

    public void consumeTile(){
        burned = true;
        gameObject.name = "Burned";
        tile = TileType.Burned;
        defaultTile = tile;
        ChangeTileState(tile);
        changeMaterial(); 
    }

    public void setToDirt(){
        tile = TileType.Dirt;
        ChangeTileState(tile);
        changeMaterial();
    }

    public void applyFoam(int duration){
        prevent = duration;
    }

    public void depleteFoam(){
        prevent--;
    }
    public int getPrevent(){
        return prevent;
    }

    public bool CanOnFire(){
        if (!onFire && decay > 0 && prevent <= 0){
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

    public int GetTraversalRule()
    {
        return this.traversalRule;
    }

    public int GetTraversalCost()
    {
        return this.traversalCost;
    }

    public int GetDistance(TileBehaviour target)
    {
        Vector3Int targetCoords = target.cellPos;

        return Mathf.Max(
            Mathf.Abs(targetCoords.y - cellPos.y),
            Mathf.Abs(targetCoords.y - cellPos.y),
            Mathf.Abs(targetCoords.y - cellPos.y)
            );
    }
}