using System.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public CurrentLevelReference _level;

    public GameState GameState;

    void Awake() {
        Instance = this;
        _level.LevelRules.CurrentNumberRows = _level.LevelRules.StartingRows;
        Debug.Log("Game Manager Awake()");

    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Game Manager Start()");
        ChangeState(GameState.GenerateGrid);
    }

    public void EndGameState(GameState currentGameState) {
        Debug.Log($"Game Manager: End Game State {currentGameState}");
        if (DeckManager.Instance.IsTutorial)
        {
            TutorialManager.Instance.EndGameState(currentGameState);
        }
        StartCoroutine(ArtificalWaitForSeconds(0.1f, () =>
        {
            switch (currentGameState)
            {
                case GameState.GenerateGrid:
                    StartCoroutine(ArtificalWaitForSeconds(0.3f, () =>
                    {
                        ChangeState(GameState.SpawnEnemies);
                    }));
                    break;
                case GameState.SpawnEnemies:
                    StartCoroutine(ArtificalWaitForSeconds(0.3f, () =>
                    {
                        ChangeState(GameState.SpawnItems);
                    }));
                    break;
                case GameState.SpawnItems:
                    StartCoroutine(ArtificalWaitForSeconds(0.3f, () =>
                    {
                        ChangeState(GameState.SpawnHazards);
                    }));
                    break;
                case GameState.SpawnHazards:
                    StartCoroutine(ArtificalWaitForSeconds(0.3f, () =>
                    {
                        ChangeState(GameState.PlaceHero);
                    }));
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
                    ChangeState(GameState.EndTurn);
                    break;
                case GameState.EndTurn:
                    ChangeState(GameState.DrawHand);
                    break;
                case GameState.LevelWin:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(currentGameState), currentGameState,
                        "You added a state and forgot to handle it here");
            }
        }));
    }

    private IEnumerator ArtificalWaitForSeconds(float seconds, Action completion)
    {
        yield return new WaitForSeconds(seconds);
        completion.Invoke();
    }

    
    void ChangeState(GameState newState) {
        Debug.Log($"Game Manager: Start Game State {newState}");
        GameState = newState;
        MenuManager.Instance.ShowTurnPhase(newState);
        if (DeckManager.Instance.IsTutorial)
        {
            TutorialManager.Instance.ChangeState(newState);
        }
        switch (newState) {
            case GameState.GenerateGrid:
                GridManager.Instance.LoadGrid();
                break;
            case GameState.SpawnEnemies:
                GridManager.Instance.LoadEnemies();
                break;
            case GameState.SpawnHazards:
                GridManager.Instance.LoadHazards();
                break;
            case GameState.SpawnItems:
                GridManager.Instance.LoadItems();
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
            case GameState.EndTurn:
                EndGameState(GameState.EndTurn);
                break;
            case GameState.LevelWin:
                WinLoseManager.Instance.GameWin();
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
    EnemyTurn = 10,
    EndTurn = 11,
    LevelWin = 12
}