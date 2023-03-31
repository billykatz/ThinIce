using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Tile", menuName = "Tile")]
public class ScriptableTile : ScriptableObject
{
    public TileType TileType;
    public GameObject TilePrefab;
    public ScriptableItem Item;
    public ScriptableUnit Unit;
}
