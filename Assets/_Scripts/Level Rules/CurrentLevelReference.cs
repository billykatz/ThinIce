using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(fileName = "New Current Level Reference", menuName = "Current Level Reference")]
public class CurrentLevelReference : ScriptableObject
{
    public ScriptableLevelRules LevelRules;
}
