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
        await Task.Delay(100);
        switch (currentGameState) {
            case GameState.GenerateGrid:
                ChangeState(GameState.SpawnEnemies);
                break;
            case GameState.SpawnEnemies:
                ChangeState(GameState.PlaceHero);
                break;
            case GameState.SpawnHazards:
                break;
            case GameState.SpawnItems:
                break;
            case GameState.PlaceHero:
                PlayerManager.Instance.HeroUnitUpdated();
                ChangeState(GameState.CreateDeck);
                break;
            case GameState.CreateDeck:
                ChangeState(GameState.DrawHand);
                break;
            case GameState.DrawHand:
                ChangeState(GameState.HeroTurnPlayCardOne);
                break;
            case GameState.HeroTurnPlayCardOne:
                ChangeState(GameState.HeroTurnPlayCardTwo);
                break;
            case GameState.HeroTurnPlayCardTwo:
                ChangeState(GameState.HeroTurnCleanUp);
                break;
            case GameState.HeroTurnCleanUp:
                ChangeState(GameState.EnemyTurn);
                break;
            case GameState.EnemyTurn:
                ChangeState(GameState.DrawHand);
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
                UnitManager.Instance.SpawnEnemyUnit();
                break;
            case GameState.SpawnHazards:
                break;
            case GameState.SpawnItems:
                break;
            case GameState.PlaceHero:
                GridManager.Instance.HighlightPlayerStartingPositions();
                break;
            case GameState.CreateDeck:
                DeckManager.Instance.CreateDeck();
                break;
            case GameState.DrawHand:
                HandManager.Instance.DrawHand();
                break;
            case GameState.HeroTurnPlayCardOne:
                break;
            case GameState.HeroTurnPlayCardTwo:
                break;
            case GameState.HeroTurnCleanUp:
                HandManager.Instance.EndPlayerTurn();
                break;
            case GameState.EnemyTurn:
                EnemiesManager.Instance.StartEnemiesTurns();
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
    HeroTurnPlayCardOne = 7,
    HeroTurnPlayCardTwo = 8,
    HeroTurnCleanUp = 9,
    EnemyTurn = 10
}