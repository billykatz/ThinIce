using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;
    public int _width, _height;
    [SerializeField] private Tile _iceTilePrefab;
    [SerializeField] private Tile _wallTilePrefab;

    [SerializeField] private Transform _cam;

    [SerializeField] private GameObject arrowController;

    private Dictionary<Vector2, Tile> _tiles;

    private void Awake() {
        Instance = this;

        Debug.Log("Grid Manager Awake()");
    }
    
    public void GenerateGrid() {
        _tiles = new Dictionary<Vector2, Tile>();
        for (int x = 0; x < _width; x++) {
            for (int y = 0; y < _height; y++) {
                var shouldBeWall = (y == 3 && x == 2);
                var spawnTile = Instantiate(shouldBeWall ? _wallTilePrefab : _iceTilePrefab, new Vector3(x, y), Quaternion.identity);
                spawnTile.name = $"Tile {x} {y}";
                spawnTile.Init(x, y);


                _tiles[new Vector2(x, y)] = spawnTile;   
            }
        }


        // put the camera at the center of our grid
        _cam.transform.position = new Vector3((float)_width /2 - 0.5f, (float)_height / 2 - 1.75f, -10);

        GameManager.Instance.EndGameState(GameState.GenerateGrid);
    }


    public void HighlightPlayerStartingPositions() {
        List<Vector2> startingTiles = PotentialPlayerStartingPositions();
        foreach (Vector2 tileCoord in startingTiles) {
            _tiles[tileCoord].HighlightStartingPlayerPosition();
        }
    }

    public void RemoveHighlightPlayerStartingPositions() {
        List<Vector2> startingTiles = PotentialPlayerStartingPositions();
        foreach (Vector2 tileCoord in startingTiles) {
            _tiles[tileCoord].RemoveHighlightStartingPlayerPosition();
        }
    }

    private List<Vector2> PotentialPlayerStartingPositions() {
        List<Vector2> level1Coords = new List<Vector2>();
        level1Coords.Add(new Vector2(1, 0));
        level1Coords.Add(new Vector2(1, 1));
        level1Coords.Add(new Vector2(2, 0));
        level1Coords.Add(new Vector2(2, 1));
        return level1Coords;
    }

    public Tile GetTilePosition(Vector2 pos) {
        if (_tiles.TryGetValue(pos, out var tile)) {
            return tile;
        }

        return null;
    }

    public Tile GetEnemySpawnTile() {
        return _tiles.Where(t => t.Value.isWalkable && !PotentialPlayerStartingPositions().Contains(t.Key)).OrderBy(o=>Random.value).First().Value;
    }

    public async void TileWasTapped(int x, int y) {
        Vector2 tappedCoord = new Vector2(x, y);
        if (GameManager.Instance.GameState == GameState.PlaceHero) {
            if (PotentialPlayerStartingPositions().Contains(tappedCoord)) {
                Debug.Log($"Tile at {x}, {y} was tapped");
                BaseHero hero = UnitManager.Instance.SpawnHeroUnit();
                _tiles[tappedCoord].SetUnit(hero);
                hero.OccupiedTile = _tiles[tappedCoord];

                // remove the special highlights
                RemoveHighlightPlayerStartingPositions();

                // we are done placing the hero, move the game along
                GameManager.Instance.EndGameState(GameState.PlaceHero);
            } else {
                RemoveHighlightPlayerStartingPositions();
                await Task.Delay(100);
                HighlightPlayerStartingPositions();
                await Task.Delay(50);
                RemoveHighlightPlayerStartingPositions();
                await Task.Delay(100);
                HighlightPlayerStartingPositions();
            }
        } 
    }

    private void Update() {
        if (!currentlyMoving) {
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) {
                // move left
                Debug.Log("moved left");
                currentlyMoving = true;
                ArrowTapped(GridMovement.Left);
            } else if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) {
                // move up
                Debug.Log("moved up");
                currentlyMoving = true;
                ArrowTapped(GridMovement.Up);
            } else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) {
                // move down
                Debug.Log("moved down");
                currentlyMoving = true;
                ArrowTapped(GridMovement.Down);
            }  else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) {
                // move right
                Debug.Log("moved right");
                currentlyMoving = true;
                ArrowTapped(GridMovement.Right);
            }
        } else {
            if (Input.GetKeyDown(KeyCode.P)) {
                Debug.Log($"Currently Moving? {currentlyMoving}");
                // waitingForInput = true;
            }
        }
        
    }

    private bool ValidMove(GridMovement movement) {

        Tile playerTile = _tiles.Where(t=>(t.Value.OccupiedUnit != null) && t.Value.OccupiedUnit.Faction == Faction.Hero).ToList().First().Value;

        if (playerTile == null) { 
            return false;
        }

        return currentCard.movementCard.CanMoveForGridMovement(movement, currentCard.movementCard.movementIndex);
    } 

    public bool IsOccupied(Vector2 coord) {
        return _tiles[coord].OccupiedUnit != null || !_tiles[coord].isWalkable;
    }

    public void MoveUnit(BaseUnit unit, Tile fromTile, Vector2 toCoord) {
        // update the tiles
        _tiles[toCoord].SetUnit(unit);

        // remove the old state
        fromTile.OccupiedUnit = null;
    }

    public BaseUnit GetHeroUnit() {
        // current player unit
        return GetHeroTile().OccupiedUnit;
    }
    public Tile GetHeroTile() {
        // current player tile
        Tile playerTile = _tiles.Where(t=>(t.Value.OccupiedUnit != null) && t.Value.OccupiedUnit.Faction == Faction.Hero).ToList().First().Value;
        return playerTile;
    }

    public List<Tile> GetEnemyUnits() {
        // grab all the enemy tiles
        return _tiles.Where(t=>(t.Value.OccupiedUnit != null) && t.Value.OccupiedUnit.Faction == Faction.Enemy).Select(s=>s.Value).ToList();
    }

    private void MoveHero(GridMovement movement) {
        // current palyer
        Tile playerTile = _tiles.Where(t=>(t.Value.OccupiedUnit != null) && t.Value.OccupiedUnit.Faction == Faction.Hero).ToList().First().Value;

        if (ValidMove(movement)) {
            Debug.Log($"GridManager: Valid movement.");
            waitingForInput = false;

            Vector2 newPlayerPosition = TileAfterMovement(movement);
            if (DidInitiateCombat(newPlayerPosition)) {
                
                // we need to fight and then finish the movement.
                CardRuleStep newStep = new CardRuleStep();
                newStep.state = CardRuleState.Combat;
                newStep.attackerUnit = playerTile.OccupiedUnit;
                newStep.defenderUnit = _tiles[newPlayerPosition].OccupiedUnit;

                Debug.Log($"GridManager: Did initiate combat {playerTile.OccupiedUnit} {_tiles[newPlayerPosition].OccupiedUnit}");
                CardRuleManager.Instance.StartCardRuleStep(newStep);

            } else {
                Debug.Log($"GridManager: Did finish moving");
                if (!_tiles[newPlayerPosition].isWalkable) {
                    // dont let player go into walls

                } else {
                    // valid move no combat
                    _tiles[newPlayerPosition].SetUnit(playerTile.OccupiedUnit);
                    playerTile.OccupiedUnit = null;
                }
                // send it back to the CardRule contoller
                CardRuleManager.Instance.DidCompleteMovement();
            }
        } else {
            // invalid move
            Debug.Log($"GridManager: Invalid move");
            currentlyMoving = false;
        }
    }

    private bool DidInitiateCombat(Vector2 targetTile) {
        if (_tiles[targetTile].OccupiedUnit != null && _tiles[targetTile].OccupiedUnit.Faction != Faction.Hero) {
            //  combat
            return true;
        } else {
            return false;
        }
    }

    public bool CheckForDeadEnemy() {
        List<KeyValuePair<Vector2, Tile>> deadEnemyList = _tiles.Where(t=>(t.Value.OccupiedUnit != null) && t.Value.OccupiedUnit.Faction == Faction.Enemy && t.Value.OccupiedUnit.health == 0).ToList();
        if (deadEnemyList.Count > 0) {
            return true;
        }
        return false;
    }

    public void CheckForDeadUnits() {
        return;
    }

    public void KillEnemyAndMovePlayer() {
        Vector2 deadEnemy = _tiles.Where(t=>(t.Value.OccupiedUnit != null) && t.Value.OccupiedUnit.Faction == Faction.Enemy && t.Value.OccupiedUnit.health == 0).ToList().First().Key;
        Tile playerTile = _tiles.Where(t=>(t.Value.OccupiedUnit != null) && t.Value.OccupiedUnit.Faction == Faction.Hero).ToList().First().Value;
        Debug.Log("There is a dead enemy");
        if (deadEnemy != null && playerTile != null) {
            Debug.Log("Enemy is dead and player is moving to their square");
            // destroy the unit that is there
            Destroy(_tiles[deadEnemy].OccupiedUnit.gameObject);
            // move the player to the unit
            _tiles[deadEnemy].SetUnit(playerTile.OccupiedUnit);
            // remove player from old tile
            playerTile.OccupiedUnit = null;
        }
    }
    private Vector2 TileAfterMovement(GridMovement movement) {
        Vector2 playerPosition = _tiles.Where(t=>(t.Value.OccupiedUnit != null) && t.Value.OccupiedUnit.Faction == Faction.Hero).ToList().First().Key;

        Debug.Log($"Player is at {(int)playerPosition.x}, {(int)playerPosition.y}");

        // this assumes that there will be no rounding erros for the float values
        int coordX = (int)playerPosition.x;
        int coordY = (int)playerPosition.y;

        if (movement == GridMovement.Left) {
            coordX = Mathf.Max(0, coordX-1);
            Debug.Log($"Movement is left. New PP is {(int)coordX}, {(int)coordY}");

        } else if (movement == GridMovement.Right) {
            coordX = Mathf.Min(_width-1, coordX+1);
            Debug.Log($"Movement is right. New PP is {(int)coordX}, {(int)coordY}");

        } else if (movement == GridMovement.Up) {
            coordY = Mathf.Min(_height-1, coordY+1);
            Debug.Log($"Movement is Up. New PP is {(int)coordX}, {(int)coordY}");

        } else if (movement == GridMovement.Down) {
            coordY = Mathf.Max(0, coordY-1);
            Debug.Log($"Movement is Down. New PP is {(int)coordX}, {(int)coordY}");

        }

        
        return new Vector2(coordX, coordY);
    }  

    private CombinedCard currentCard;
    private bool currentlyMoving;

    private bool waitingForInput;

    public void ShowMovementHelper(CombinedCard card, int movementIndex) {
        currentCard = card;
        // show helper text
        MenuManager.Instance.PlayCardInstructions(card, movementIndex);

        // wait for input (arrow keys can control movement)
        currentlyMoving = false;
        waitingForInput = true;

        ShowMovementArrows(card, movementIndex);

    }

    private MovementArrowController movementArrowController;
    private GameObject movementArrowGameObject;

    private void ShowMovementArrows(CombinedCard card, int movementIndex) {
        movementArrowGameObject = Instantiate(arrowController);
        movementArrowController = movementArrowGameObject.GetComponent<MovementArrowController>();

        movementArrowController.SetArrows(card.movementCard.GetGridMovement(movementIndex));
        movementArrowController.OnArrowTapped += ArrowTapped;

        Tile playerTile = _tiles.Where(t=>(t.Value.OccupiedUnit != null) && t.Value.OccupiedUnit.Faction == Faction.Hero).ToList().First().Value;

        if (playerTile == null) {
            return;
        }

        // put the arrows on the player's tile
        movementArrowGameObject.transform.position = playerTile.transform.position;
    }

    public void ArrowTapped(GridMovement gridMovement) {
        if (!waitingForInput) { return; }
        if (ValidMove(gridMovement)) {
            movementArrowController.OnArrowTapped -= ArrowTapped;
            Destroy(movementArrowGameObject);
        }
        MoveHero(gridMovement);
    }
}
public enum GridMovement {
    Left = 0,
    Right = 1, 
    Up = 2,
    Down = 3
}