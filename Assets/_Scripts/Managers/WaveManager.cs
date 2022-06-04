using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance;

    [SerializeField] private Text incomingEnemyWaveText;

    private List<ScriptableLevelRules> levelRules;

    private int numberOfTurns = 0;

    private bool hasSpawnedWave2 = false;
    private bool hasSpawnedWave3 = false;

    private void Awake() {
        Instance = this;

        levelRules = Resources.LoadAll<ScriptableLevelRules>("Rules").ToList();
    }

    private void Start() {
        incomingEnemyWaveText.text = $"{levelRules[GameManager.Instance.levelIndex].wave2TurnNumber} turns";
    }

    public void EndTurn() {
        numberOfTurns++;

        if (numberOfTurns >= levelRules[GameManager.Instance.levelIndex].wave2TurnNumber && !hasSpawnedWave2) {
            hasSpawnedWave2 = true;
            UnitManager.Instance.SpawnEnemyWave(levelRules[GameManager.Instance.levelIndex].wave2NumberEnemies);
        } else if (numberOfTurns >= levelRules[GameManager.Instance.levelIndex].wave3TurnNumber && !hasSpawnedWave3) {
            hasSpawnedWave3 = true;
            UnitManager.Instance.SpawnEnemyWave(levelRules[GameManager.Instance.levelIndex].wave3NumberEnemies);
        } 

        if (!hasSpawnedWave2 && !hasSpawnedWave3) {
            int turnsLeft = levelRules[GameManager.Instance.levelIndex].wave2TurnNumber - numberOfTurns;
            if (turnsLeft > 1) {
                incomingEnemyWaveText.text = $"{turnsLeft} turns";
            } else if (turnsLeft == 1) {
                incomingEnemyWaveText.text = $"Next Turn";
            }
        } else if (hasSpawnedWave2 && !hasSpawnedWave3) {
            int turnsLeft = levelRules[GameManager.Instance.levelIndex].wave3TurnNumber - numberOfTurns;
            if (turnsLeft > 1) {
                incomingEnemyWaveText.text = $"{turnsLeft} turns";
            } else if (turnsLeft == 1) {
                incomingEnemyWaveText.text = $"Next Turn";
            }
        } else {
            incomingEnemyWaveText.text = $"No more waves";
        }

        GameManager.Instance.EndGameState(GameState.EndTurn);
    }
}
