using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitBehaviour : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private int range; // how many tiles away an action can be made 
    [SerializeField] private int movements; // how many tiles the unit can move before making an action
    [SerializeField] private int maxMovements; // how many tiles the unit can move before making an action
    [SerializeField] private int actions; // how many actions the unit can make
    [SerializeField] private int capacity; // how many actions the unit can make before needing to replenish
    [SerializeField] private bool preventative; // unit can disable tiles
    [SerializeField] private bool extinguish; // unit can put out fires
    [SerializeField] private bool support; // unit can replenish other units 

    [SerializeField] private int traversalType; //the type of unit encoded as an int
                                                //0 == ground
                                                //1 == air

    [SerializeField] private Vector3Int cellPos;
   	[SerializeField] private Vector3Int originPos; // position before the end of turn

    private int water;
    private GameObject occupyingTile = null;
    private Grid grid;

    void Awake()
    {
        grid = transform.parent.GetComponent<Grid>();
        water = capacity;
        setOriginPos();
    }

    public void SetOccupyingTile(GameObject tile){
    	this.occupyingTile = tile;
    }

    public GameObject GetOccupyingTile(){
    	return occupyingTile;
    }

    public Vector3Int getOriginPos(){
    	return originPos;
    }

    public void setOriginPos(){
    	originPos = grid.WorldToCell(new Vector3(transform.position.x, 0, transform.position.z));
    	cellPos = originPos;
    }

    public Vector3Int getCellPos(){
    	return grid.WorldToCell(new Vector3(transform.position.x, 0, transform.position.z));
    }

    public bool getSupport(){
    	return support;
    }

    public bool getPreventative(){
    	return preventative;
    }

    public bool getExtinguish(){
    	return extinguish;
    }

    public int getWater(){
    	return water;
    }

    public int getMovements(){
        return movements;
    }

    public int getActions(){
        return actions;
    }


    public int getCapacity(){
    	return capacity;
    }

    public void refillWater(){
    	water = capacity;
    }

    public void useWater(int water){
    	this.water = this.water - water;
    }

    public void ResetMovements() 
    {
        movements = maxMovements;
    }

    public void SetMovements(int input) 
    {
        movements = input;
    }

    public int GetMovements()
    {
        return movements;
    }

    public int GetTraversalType()
    {
        return traversalType;
    }
}
