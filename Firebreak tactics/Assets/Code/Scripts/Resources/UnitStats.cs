using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitStats : MonoBehaviour
{
    // Number of tiles unit can reach 
    [SerializeField]
    private int range;

    // Number of tiles a unit can move during a turn
    [SerializeField]
    private int movement;

    // Number of tiles that can be selected as an action per turn
    [SerializeField]
    private int actionPoints;

    // Used if unit can extinguish tiles
    [SerializeField]
    private bool canExtinguish;

    // Used if unit can prevent fire from spreading
    [SerializeField]
    private bool preventive;

    // X, Y, Z coordinates
    [SerializeField]
    private float xLocation;
    [SerializeField]
    private float yLocation;
    [SerializeField]
    private float zLocation;


    // Getters and setters for unit position
    public void setXLocation(float xLocation) { this.xLocation = xLocation; }

    public float getXLocation() { return this.xLocation; }

    public void setYLocation(float yLocation) { this.yLocation = yLocation; }

    public float getYLocation() { return this.yLocation; }

    public void setZLocation(float zLocation) { this.zLocation = zLocation; }

    public float getZLocation() { return this.zLocation; }
}
