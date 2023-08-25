using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitBehaviour : MonoBehaviour
{
    [SerializeField] private UnitType unit;
    [SerializeField] private int range; // how many tiles away an action can be made 
    [SerializeField] private int movements; // how many tiles the unit can move before making an action
    [SerializeField] private int movementsLeft; // how many movements are left per turn
    [SerializeField] private int actions; // how many actions the unit can make
    [SerializeField] private int actionsLeft; // how many actions are left for each turn
    [SerializeField] private int capacity; // how many actions the unit can make before needing to replenish
    [SerializeField] private bool preventative; // unit can disable tiles
    [SerializeField] private bool extinguish; // unit can put out fires
    [SerializeField] private bool support; // unit can replenish other units 

    private int water;
    private Vector3Int cellPos;
    private Grid grid;

    public enum UnitType
    {
        Foam,
        Scout,
        Spotter,
        Striker,
        Tanker,
        Transport,
        Transport2

    }
    void Awake()
    {
        grid = transform.parent.GetComponent<Grid>();
        cellPos = grid.WorldToCell(new Vector3(transform.position.x, 0, transform.position.z));
    }

    private void SetDefaultState(UnitType unit)
    {
        switch (unit)
        {
            case UnitType.Foam:
                range = 2;
                movements = 3;
                movementsLeft = movements;
                actions = 2;
                actionsLeft = actions;
                capacity = 6;
                preventative = true;
                extinguish = false;
                support = false;
                break;
            case UnitType.Scout:
                range = 0;
                movements = 7;
                movementsLeft = movements;
                actions = 0;
                actionsLeft = actions;
                capacity = 0;
                preventative = false;
                extinguish= false;
                support= false;
                break;
            case UnitType.Spotter:
                range = 2;
                movements = 3;
                movementsLeft = movements;
                actions = 4;
                actionsLeft = actions;
                capacity = 4;
                preventative = false;
                extinguish = true;
                support = false;
                break;
            case UnitType.Striker:
                range = 3;
                movements = 4;
                movementsLeft = movements;
                actions = 2;
                actionsLeft = actions;
                capacity = 6;
                preventative = true;
                extinguish = false;
                support = false;
                break;
            case UnitType.Tanker:
                range = 2;
                movements = 3;
                movementsLeft = movements;
                actions = 3;
                actionsLeft = actions;
                capacity = 9;
                preventative = true;
                extinguish = true;
                support = false;
                break;
            case UnitType.Transport:
                range = 1;
                movements = 4;
                movementsLeft = movements;
                actions = 3;
                actionsLeft = actions;
                capacity = 12;
                preventative = false;
                extinguish = false;
                support = true;
                break;
            case UnitType.Transport:
                range = 1; 
                movements = 5;
                movementsLeft = movements;
                actions = 7;
                actionsLeft = actions;
                capacity = 7;
                preventative = false;
                extinguish = false;
                support = true;
                break;
            default:
                break;
        }
    }

    
    private bool hasTurn(){
        // If unit has actions or movements remaining, Then they have a turn remaining
        if ((getActions() != 0) || (getMovements() != 0)){
            return true;
        } else {
            return false;
        }
    }

    public int getActionsLeft(){
        return this.actionsLeft;
    }

    // Resets actionsLeft and movementsLeft to max on new turn
    public void resetActions()
    {
        this.actionsLeft = this.actions;
        this.movementsLeft = this.movements;
    }

    // Reduce actionsLeft by the amount of actions used
    public void useActions(int actionsUsed){
        if (actionsUsed <= getActionsLeft())
        {
            this.actionsLeft -= actionsUsed;
        } else
        {
            //TODO: Will probably check in UnitController for correct amount of actions left 
        }
        
    }

    public int getMovementsLeft(){
        return this.movementsLeft;
    }

    public void useMovements(int movementsUsed){
        if (movementsUsed <= getMovementsLeft())
        {
            this.movementsLeft -= movementsUsed;
        }
    }

    public int getRange()
    {
        return this.range;
    }

    public int getCapacity()
    {
        return this.capacity;
    }

    public bool getPreventative()
    {
        return this.preventative;
    }

    public bool getExtinguish()
    {
        return this.extinguish;
    }

    public bool getSupport()
    {
        return this.support;
    }
}
