using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Scriptable Item")]
public class ScriptableItem : ScriptableObject
{
    public ItemStat stat;
    public int amount;
    public GameObject ItemPrefab;
}

[Serializable]
public enum ItemStat
{
    None = 0,
    Attack,
    Armor,
    Health
}