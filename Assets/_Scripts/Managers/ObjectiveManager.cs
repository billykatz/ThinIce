using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveManager : MonoBehaviour
{
    [SerializeField] private GameObject objectiveManagerUI;
    [SerializeField] private ObjectiveToggle objective1;
    [SerializeField] private ObjectiveToggle objective2;

    public static ObjectiveManager Instance;
    private List<ScriptableLevelRules> levelRules;

    private int numberEnemiesKilled = 0;
    private int numPlayedCards = 0;

    private int completedObjectives = 0;

    private bool completedKillMonsterObjective = false;
    private bool completedPlayedCardsObjective = false;

    private void Awake() {
        Instance = this;

        levelRules = Resources.LoadAll<ScriptableLevelRules>("Rules").ToList();

    }

    private void Start() {
        objective1.gameObject.SetActive(false);
        objective2.gameObject.SetActive(false);
        ScriptableLevelRules rule = levelRules[GameManager.Instance.levelIndex];
            
        if (rule.numberOfEnemiesToKill > 0) {
            objective1.gameObject.SetActive(true);
            objective1.SetObjectiveText($"Destroy {rule.numberOfEnemiesToKill} enemies");
            objective1.SetProgressText($"({numberEnemiesKilled}/{rule.numberOfEnemiesToKill})");
        }

        if (rule.numberOfCardsToPlay > 0) {
            objective2.gameObject.SetActive(true);
            objective2.SetObjectiveText($"Play {rule.numberOfCardsToPlay} or fewer cards");
            objective2.SetProgressText($"({numPlayedCards}/{rule.numberOfCardsToPlay})");
        }

        // listen for dead units
        CombatManager.OnUnitDidDie += UnitDidDie;
    }

    private void UpdateProgressText() {
        ScriptableLevelRules rule = levelRules[GameManager.Instance.levelIndex];
        objective1.SetProgressText($"({numberEnemiesKilled}/{rule.numberOfEnemiesToKill})");
        objective2.SetProgressText($"({numPlayedCards}/{rule.numberOfCardsToPlay})");
    }

    private void CheckForWin() {
        ScriptableLevelRules rule = levelRules[GameManager.Instance.levelIndex];
        if (rule.numberOfEnemiesToKill > 0) {
            if (numberEnemiesKilled >= rule.numberOfEnemiesToKill && !completedKillMonsterObjective) {
                // player win
                objective1.SetToggle(true);
                completedKillMonsterObjective = true;
                completedObjectives++;
            }


        }

        if (rule.numberOfCardsToPlay > 0) {
            if (numPlayedCards <= rule.numberOfCardsToPlay && !completedPlayedCardsObjective) {
                // this one is a little weirder
            }
        
        }

        if (completedObjectives >= rule.totalObjectives) {
            WinLoseManager.Instance.GameWin();
        }
    }

    private void UnitDidDie(BaseUnit unit) {
        Debug.Log($"Objective Manager: {unit.UnitName} did die");
        if (unit.Faction == Faction.Enemy) {
            numberEnemiesKilled++;
            UpdateProgressText();
            CheckForWin();
        } else {
            // player died
            WinLoseManager.Instance.GameLose();
        }
    }


}
