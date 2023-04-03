using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseHazard : MonoBehaviour
{
    public HazardType HazardType;
    public Movement[] Movements;
    public int Damage;

    public void Configure(ScriptableHazards hazards)
    {
        HazardType = hazards.HazardType;
        Movements = hazards.Movements;
        Damage = hazards.Damage;
    }
    
}
