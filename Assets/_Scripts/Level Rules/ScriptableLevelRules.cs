using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Level Rule", menuName = "Level Rule")]
public class ScriptableLevelRules : ScriptableObject
{
    public int LevelNumber;
    public int CurrentNumberRows;

    public int Width;
    public int StartingRows;

    public ScriptableRow[] Rows;

    public bool isLevel;
    public bool isTutorial;

    public string WorldMapLevelTitle;

    public DeckImprovementModel[] DeckImprovementModels;

}

[Serializable]
public class Row
{
    public ScriptableTile[] Tiles;
}

[Serializable]
public enum ShopType
{
    None,
    Remove,
    Upgrade,
    Add,
    StatBoost,
}