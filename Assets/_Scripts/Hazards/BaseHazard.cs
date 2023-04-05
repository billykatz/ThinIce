using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseHazard : MonoBehaviour
{
    public HazardType HazardType;
    public GridMovement[] Movements;
    public int Damage;
    public Tile OccupiedTile;
    public string Name;

    public void Configure(ScriptableHazards hazards)
    {
        HazardType = hazards.HazardType;
        Movements = hazards.Movements;
        Damage = hazards.Damage;
        Name = hazards.name;
    }
    
}
