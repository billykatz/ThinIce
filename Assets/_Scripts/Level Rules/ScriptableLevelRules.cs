using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Level Rule", menuName = "Level Rule")]
public class ScriptableLevelRules : ScriptableObject
{
    public int numberOfEnemiesToKill;
    public int numberOfCardsToPlay;
    public int totalObjectives;

    public int wave2TurnNumber;
    public int wave3TurnNumber;
    public int wave2NumberEnemies;
    public int wave3NumberEnemies;


}
