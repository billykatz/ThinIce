using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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

    public async void EndGameState(GameState currentGameState) {
        await Task.Delay(2000);
        switch (currentGameState) {
            case GameState.GenerateGrid:
                ChangeState(GameState.SpawnEnemies);
                break;
            case GameState.SpawnEnemies:
                ChangeState(GameState.CreateDeck);
                break;
            case GameState.SpawnHazards:
                break;
            case GameState.SpawnItems:
                break;
            case GameState.PlaceHero:
                break;
            case GameState.CreateDeck:
                ChangeState(GameState.DrawHand);
                break;
            case GameState.DrawHand:
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
        MenuManager.Instance.ShowTurnPhase(newState);
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
            case GameState.CreateDeck:
                DeckManager.Instance.CreateDeck();
                break;
            case GameState.DrawHand:
                HandManager.Instance.DrawHand();
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
    CreateDeck = 5,
    DrawHand = 6,
    HeroTurn = 7,
    EnemyTurn = 8
}