using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Scriptable Hazard", menuName = "Hazard")]
public class ScriptableHazards: ScriptableObject
{
    public BaseHazard HazardPrefab;
    public HazardType HazardType;
    public GridMovement[] Movements;
    public int Damage;
}

[Serializable]
public enum HazardType
{
    None, 
    Spikes,
    Movement
}