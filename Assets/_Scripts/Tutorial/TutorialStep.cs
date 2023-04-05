using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Tutorial Step", menuName = "Tutorial Step")]
public class TutorialStep: ScriptableObject
{
    public string[] Texts;
    public Sprite[] SpeakerImage;
    public string[] SpeakerName;
    public Sprite[] DetailImage;
    public string[] DetailText;
    public GameState StateToTrigger;
    public bool TriggerOnItemSpawn;
    public bool TriggerOnHazardSpawn;
    public bool TriggerOnGoalSpawn;
    public bool TriggerOnEnemySpawn;
    
}
