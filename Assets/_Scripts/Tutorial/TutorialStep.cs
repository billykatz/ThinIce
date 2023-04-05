using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;


[CreateAssetMenu(fileName = "New Tutorial Step", menuName = "Tutorial Step")]
public class TutorialStep: ScriptableObject
{
    public string[] Texts;
    public Sprite[] SpeakerImage;
    public string[] SpeakerName;
    public Sprite[] DetailImage;
    public string[] DetailText;
    public RenderTexture[] DetailRenderTexture;
    public VideoClip[] DetailVideoClip;
    public GameState StateToTrigger;
    public bool TriggerOnItemSpawn;
    public bool TriggerOnHazardSpawn;
    public bool TriggerOnGoalSpawn;
    public bool TriggerOnEnemySpawn;
    public bool TriggerOnEnemyDead;
    
    public bool NoTriggers
    {
        get
        {
            return !TriggerOnEnemyDead && !TriggerOnEnemySpawn && !TriggerOnGoalSpawn && !TriggerOnHazardSpawn &&
                   !TriggerOnItemSpawn;
        }
    }
    
}
