using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Level Rule", menuName = "Level Rule")]
public class ScriptableLevelRules : ScriptableObject
{
    public int numberOfEnemiesToKill;
    public int numberOfCardsToPlay;
    public int totalObjectives;

    public int LevelNumber;
    public int LevelLength;
    public int CurrentNumberRows;

    public int Width;
    public int StartingRows;
    
    public float BaseChanceSpawnWall;
    public float BaseChanceSpawnEnemy;
    public float ChanceDeltaSpawnEnemy;


}
