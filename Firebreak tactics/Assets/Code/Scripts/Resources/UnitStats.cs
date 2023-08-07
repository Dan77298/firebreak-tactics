using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitStats : MonoBehaviour
{
    // Number of tiles unit can reach 
    public int range;

    // Number of tiles a unit can move during a turn
    public int movement;

    // Number of tiles that can be selected as an action per turn
    public int actionPoints;

    // Used if unit can extinguish tiles
    public bool canExtinguish;

    // Used if unit can prevent fire from spreading
    public bool preventive;

}
