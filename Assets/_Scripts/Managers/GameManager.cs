using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameState GameState;

    void Awake() {
        Instance = this;

    }

    // Start is called before the first frame update
    void Start()
    {
        ChangeState(GameState.GenerateGrid);
    }

    public void EndGameState(GameState currentGameState) {
        switch (currentGameState) {
            case GameState.GenerateGrid:
                ChangeState(GameState.SpawnEnemies);
                break;
            case GameState.SpawnEnemies:
                break;
            case GameState.SpawnHazards:
                break;
            case GameState.SpawnItems:
                break;
            case GameState.PlaceHero:
                break;
            case GameState.HeroTurn:
                break;
            case GameState.EnemyTurn:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(currentGameState), currentGameState, "You added a state and forgot to handle it here");
        }
    }

    void ChangeState(GameState newState) {
        GameState = newState;
        switch (newState) {
            case GameState.GenerateGrid:
                GridManager.Instance.GenerateGrid();
                break;
            case GameState.SpawnEnemies:
                UnitManager.Instance.SpawnEnemyUnit();
                break;
            case GameState.SpawnHazards:
                break;
            case GameState.SpawnItems:
                break;
            case GameState.PlaceHero:
                break;
            case GameState.HeroTurn:
                break;
            case GameState.EnemyTurn:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, "You added a state and forgot to handle it here");
        }
    }
}

public enum GameState {
    GenerateGrid = 0,
    SpawnEnemies = 1,
    SpawnHazards = 2,
    SpawnItems = 3,
    PlaceHero = 4,
    HeroTurn = 5,
    EnemyTurn = 6
}