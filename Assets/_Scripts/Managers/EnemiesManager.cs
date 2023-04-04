using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using Extensions;
using System.Threading.Tasks;

public class EnemiesManager : MonoBehaviour
{
    
    [SerializeField] private CurrentLevelReference _levelRules;
    public static EnemiesManager Instance;
    private List<Tile> enemyTiles;
    private int enemyTurnIndex = 0;

    private static event Action enemyTurnComplete;

    private void Awake() {
        Instance = this;
        Debug.Log("Enemies Manager Awake()");
    }

    public void DestroyEnemyOnTile(Tile tile)
    {
        Destroy(tile.OccupiedUnit.gameObject);
    }

    public void DoEnemyTurn(Action callback) {
        Tile heroTile = GridManager.Instance.GetHeroTile();
        int width = _levelRules.LevelRules.Width;
        int minHeight = GridManager.Instance.BottomMostRowIndex;
        int maxHeight = _levelRules.LevelRules.CurrentNumberRows;
        
        bool enemyAttacked = false;
        bool enemyHasTakenTurn = false;

        if (enemyTiles.Count <= 0) { 
            callback.Invoke();
            return; 
        }
        
        Tile tile = enemyTiles[enemyTurnIndex];

        if (tile.OccupiedUnit.ShouldAttack(tile, heroTile)) {
            Debug.Log($"EnemiesManager: Enemy {tile.OccupiedUnit.UnitName} attacks from {tile.coord} to {heroTile.coord}");
            MenuManager.Instance.SetTextForEnemyTurn(tile.OccupiedUnit.UnitName, "attacks you!");
            CombatManager.Instance.ShowCombat(tile.OccupiedUnit, heroTile.OccupiedUnit, callback);
            return;
        } else {
            List<Vector2> wantToMove = tile.OccupiedUnit.WantToMoveTo(tile, heroTile).Where(coord=>coord.IsNeighbor(tile.coord)).ToList();
            foreach (Vector2 coord in wantToMove) {
                if (!GridManager.Instance.IsOccupied(coord)) {
                    Debug.Log($"EnemiesManager: Can't Attack. Enemy moves from {tile.coord} to {coord} to Attack next turn");
                    // if coord is not occupied then move there
                    // GridManager.Instance.MoveUnit(tile.OccupiedUnit, tile, coord);
                    enemyHasTakenTurn = true;
                    GridManager.Instance.AnimateEnemyMovement(tile.OccupiedUnit, tile, coord, () =>
                    {
                        callback.Invoke();
                    });
                    return;
                }
            }
            // then move towards the closest "want to move" that isnt occupied
            // could be optimized for sure
            foreach (Vector2 wantToMoveCoord in wantToMove) {
                if (!enemyHasTakenTurn) {
                    // creates a list of all neighbors and orders them by which one would be closest to where I eventually want to be (within Attack range of the player)
                    List<Vector2> potentialMoves = tile.coord.MyNeighbors().Where(coord=>coord.IsInBounds(width, minHeight, maxHeight)).OrderBy(coord=>Vector2.Distance(wantToMoveCoord, coord)).ToList();
                    foreach (Vector2 moveTo in potentialMoves) {
                        if (!GridManager.Instance.IsOccupied(moveTo)) {
                            Debug.Log($"EnemiesManager: Can't Attack next turn. Enemy moves from {tile.coord} to {moveTo} to Attack next turn");
                            // if coord is not occupied then move there
                            enemyHasTakenTurn = true;
                            GridManager.Instance.AnimateEnemyMovement(tile.OccupiedUnit, tile, moveTo, () =>
                            {
                                callback.Invoke();
                            });
                            return;
                        }
                    }
                }
            }
            
            //fall back in case we havent moved the enemy yet
            if (!enemyHasTakenTurn) {
                Debug.Log($"EnemiesManager: Fall back to random movement that puts us closer to the player");
                List<Vector2> potentialMoves = tile.coord.MyNeighbors().Where(coord=>coord.IsInBounds(width, minHeight, maxHeight)).OrderBy(coord=>Vector2.Distance(heroTile.coord, coord)).ToList();
                foreach (Vector2 moveTo in potentialMoves) {
                    if (!GridManager.Instance.IsOccupied(moveTo)) {
                        Debug.Log($"EnemiesManager: Can't Attack next turn. Enemy moves from {tile.coord} to {moveTo} to Attack next turn");
                        // if coord is not occupied then move there
                        GridManager.Instance.AnimateEnemyMovement(tile.OccupiedUnit, tile, moveTo, () =>
                        {
                            callback.Invoke();
                        });
                        return;
                    }
                }
            }
        }
    }
    public void StartEnemiesTurns() {
        
        // get the enemies from the grid manager
        enemyTiles = GridManager.Instance.GetEnemyTiles();
        enemyTurnIndex = 0;

        DoEnemyTurn(DidCompleteEnemyTurn);

    }

    public void DidCompleteEnemyTurn()
    {
        enemyTurnIndex++;
        if (enemyTurnIndex >= enemyTiles.Count) {
            EndAllEnemiesTurn();
        } else {
            DoEnemyTurn(DidCompleteEnemyTurn);
        }
    }

    public void EndAllEnemiesTurn() {
        GameManager.Instance.EndGameState(GameState.EnemyTurn);
    }
}
