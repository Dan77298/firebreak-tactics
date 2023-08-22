using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitBehaviour : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private int range; // how many tiles away an action can be made 
    [SerializeField] private int movements; // how many tiles the unit can move before making an action
    [SerializeField] private int actions; // how many actions the unit can make
    [SerializeField] private int capacity; // how many actions the unit can make before needing to replenish
    [SerializeField] private bool preventative; // unit can disable tiles
    [SerializeField] private bool extinguish; // unit can put out fires
    [SerializeField] private bool support; // unit can replenish other units 

    private int water;
    private Vector3Int cellPos;
    private Grid grid;

    void Awake()
    {
        grid = transform.parent.GetComponent<Grid>();
        cellPos = grid.WorldToCell(new Vector3(transform.position.x, 0, transform.position.z));
    }
}
