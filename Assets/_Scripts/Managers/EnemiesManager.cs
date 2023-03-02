using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using Extensions;
using System.Threading.Tasks;

public class EnemiesManager : MonoBehaviour
{
    
    [SerializeField] private GameSettings _gameSettings;
    public static EnemiesManager Instance;
    private List<Tile> enemyTiles;
    private int enemyTurnIndex = 0;

    private static event Action enemyTurnComplete;

    private void Awake() {
        Instance = this;
        Debug.Log("Enemies Manager Awake()");
    }

    public async void DoEnemyTurn(Action callback) {
        Tile heroTile = GridManager.Instance.GetHeroTile();
        int width = _gameSettings.Width;
        int height = _gameSettings.VisibleRows;

        enemyTurnComplete += EnemyTurnDone;

        bool enemyAttacked = false;

        if (enemyTiles.Count <= 0) { 
            EnemyTurnDone();
            return; 
        }
        Tile tile = enemyTiles[enemyTurnIndex];

        BaseUnit enemyUnit = tile.OccupiedUnit;
        // check to see if the player is in the enemies attack zone
        // give a little time between each enemy
        MenuManager.Instance.SetTextForEnemyTurn(tile.OccupiedUnit.UnitName, "Thinking...");
        await Task.Delay(1000);

        if (tile.OccupiedUnit.ShouldAttack(tile, heroTile)) {
            Debug.Log($"EnemiesManager: Enemy {tile.OccupiedUnit.UnitName} attacks from {tile.coord} to {heroTile.coord}");
            MenuManager.Instance.SetTextForEnemyTurn(tile.OccupiedUnit.UnitName, "Attacks you!");
            CombatManager.Instance.ShowCombat(tile.OccupiedUnit, heroTile.OccupiedUnit, enemyTurnComplete);
            enemyAttacked = true;
        } else {
            List<Vector2> wantToMove = tile.OccupiedUnit.WantToMoveTo(tile, heroTile).Where(coord=>coord.IsNeighbor(tile.coord)).ToList();
            bool enemyHasTakenTurn = false;
            foreach (Vector2 coord in wantToMove) {
                if (!GridManager.Instance.IsOccupied(coord)) {
                    Debug.Log($"EnemiesManager: Can't attack. Enemy moves from {tile.coord} to {coord} to attack next turn");
                    // if coord is not occupied then move there
                    GridManager.Instance.MoveUnit(tile.OccupiedUnit, tile, coord);
                    enemyHasTakenTurn = true;
                    break;
                }
            }
            // then move towards the closest "want to move" that isnt occupied
            // could be optimized for sure
            foreach (Vector2 wantToMoveCoord in wantToMove) {
                if (!enemyHasTakenTurn) {
                    // creates a list of all neighbors and orders them by which one would be closest to where I eventually ant to be (within attack range of the player)
                    List<Vector2> potentialMoves = tile.coord.MyNeighbors().Where(coord=>coord.IsInBounds(width, height)).OrderBy(coord=>Vector2.Distance(wantToMoveCoord, coord)).ToList();
                    foreach (Vector2 moveTo in potentialMoves) {
                        if (!GridManager.Instance.IsOccupied(moveTo)) {
                            Debug.Log($"EnemiesManager: Can't attack next turn. Enemy moves from {tile.coord} to {moveTo} to attack next turn");
                            // if coord is not occupied then move there
                            GridManager.Instance.MoveUnit(tile.OccupiedUnit, tile, moveTo);
                            enemyHasTakenTurn = true;
                            break;
                        }
                    }
                }
            }

            //fall back in case we havent moved the enemy yet
            if (!enemyHasTakenTurn) {
                Debug.Log($"EnemiesManager: Fall back to random movement that puts us closer to the player");
                List<Vector2> potentialMoves = tile.coord.MyNeighbors().Where(coord=>coord.IsInBounds(width, height)).OrderBy(coord=>Vector2.Distance(heroTile.coord, coord)).ToList();
                foreach (Vector2 moveTo in potentialMoves) {
                    if (!GridManager.Instance.IsOccupied(moveTo)) {
                        Debug.Log($"EnemiesManager: Moving to unoccupied tile closest to the player");
                        GridManager.Instance.MoveUnit(tile.OccupiedUnit, tile, moveTo);
                        enemyHasTakenTurn = true;
                        break;
                    }
                }
            }

            await Task.Delay(500);

            if (!enemyHasTakenTurn) {
                Debug.Log($"EnemiesManager: Enemy couldnt move or attack.  Likely a bug");
                MenuManager.Instance.SetTextForEnemyTurn(enemyUnit.UnitName, "Is paralyzed and didn't move!");
            } else {
                MenuManager.Instance.SetTextForEnemyTurn(enemyUnit.UnitName, "Decides to move!");
                Debug.Log($"EnemiesManager: Enemy successfully ends its turn");
            }
        }

        if (enemyAttacked) {

        } else {
            await Task.Delay(500);
            enemyTurnComplete.Invoke();
        }

    }
    public void StartEnemiesTurns() {
        
        // get the enemies from the grid manager
        enemyTiles = GridManager.Instance.GetEnemyUnits();
        enemyTurnIndex = 0;

        DoEnemyTurn(enemyTurnComplete);

    }

    public void EnemyTurnDone() {
        enemyTurnComplete -= EnemyTurnDone;
        enemyTurnIndex++;
        if (enemyTurnIndex >= enemyTiles.Count) {
            EndEnemitesTurn();
        } else {
            DoEnemyTurn(enemyTurnComplete);
        }

    }

    public void EndEnemitesTurn() {
        GameManager.Instance.EndGameState(GameState.EnemyTurn);
    }
}
