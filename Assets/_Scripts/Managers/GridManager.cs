using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;
    [SerializeField] private ScriptableLevelRules _levelRules;
    [SerializeField] private Tile _iceTilePrefab;
    [SerializeField] private Tile _goalTilePrefab;
    [SerializeField] private Tile _wallTilePrefab;
    [SerializeField] private Transform _cam;
    [SerializeField] private GameObject arrowController;

    // the number of visible rows
    private int _width;
    private int _visibleRows;
    private bool currentlyMoving;
    private bool waitingForInput;
    
    private Dictionary<Vector2, Tile> _tiles;
    private CombinedCard currentCard;
    private MovementArrowController movementArrowController;
    private GameObject movementArrowGameObject;

    private void Awake() {
        Instance = this;

        Debug.Log("Grid Manager Awake()");
    }

    private void Start()
    {
        _visibleRows = _levelRules.StartingRows;
        _width = _levelRules.Width;
    }


    // current player unit
    public BaseUnit GetHeroUnit() { return GetHeroTile().OccupiedUnit; }
    
    // current player tile
    public Tile GetHeroTile() { return _tiles.Where(t=>(t.Value.OccupiedUnit != null) && t.Value.OccupiedUnit.Faction == Faction.Hero).ToList().First().Value; }

    // grab all the enemy tiles
    public List<Tile> GetEnemyUnits() { return _tiles.Where(t=>(t.Value.OccupiedUnit != null) && t.Value.OccupiedUnit.Faction == Faction.Enemy).Select(s=>s.Value).ToList(); }
    
    public bool CheckForDeadEnemy() { return _tiles.Where(t=>(t.Value.OccupiedUnit != null) && t.Value.OccupiedUnit.Faction == Faction.Enemy && t.Value.OccupiedUnit.health == 0).Count() > 0; }

    public void GenerateGrid() {
        _tiles = new Dictionary<Vector2, Tile>();
        for (int x = 0; x < _width; x++) {
            for (int y = 0; y < _visibleRows; y++) {
                //spawns an ice tile, for now
                Vector2 position = new Vector2(x, y);
                var spawnTile = Instantiate(_iceTilePrefab, position, Quaternion.identity);
                spawnTile.name = $"Tile {x} {y}";
                spawnTile.Init(x, y);
                
                _tiles[position] = spawnTile;   
            }
        }
        
        // put the camera at the center of our grid
        _cam.transform.position = new Vector3((float)_width /2 - 0.5f, (float)_visibleRows / 2 - 1.75f, -10);

        GameManager.Instance.EndGameState(GameState.GenerateGrid);
    }

    public void GenerateRow()
    {
        bool stopGenerating = _levelRules.LevelLength == _levelRules.CurrentNumberRows;
        if (stopGenerating)
        {
            return;
        }
        
        // increase the number of rows
        Vector2 topLeftTileCoord = new Vector2(0, _levelRules.CurrentNumberRows-1);
        float posY = _tiles[topLeftTileCoord].transform.position.y + 1;

        bool isEndRow = _levelRules.LevelLength == _levelRules.CurrentNumberRows + 1;

        for (int x = 0; x < _width; x++)
        {
            // the coord
            Vector2 coord = new Vector2(x, _levelRules.CurrentNumberRows);
            Vector2 pos = new Vector2(x, posY);
            Tile spawnTile = Instantiate(GenerateTile(coord, isEndRow), pos, Quaternion.identity);
            spawnTile.name = $"Tile {x} {coord.y}";
            spawnTile.Init(x, (int)coord.y);
                
            _tiles[coord] = spawnTile;   
            spawnTile.PlayEntranceAnimation();
        }
        
        _levelRules.CurrentNumberRows++;
        _visibleRows = _levelRules.CurrentNumberRows;
    }

    private Tile GenerateTile(Vector2 coord, bool isEndRow)
    {
        if (isEndRow)
        {
            return _goalTilePrefab;
        }

        float baseChanceWall = _levelRules.BaseChanceSpawnWall;

        for (int i = (int)coord.x-1; i > 0; i--)
        {
            Vector2 neighbor = new Vector2(i, coord.y);
            if (_tiles[neighbor].tileType == TileType.Wall)
            {
                baseChanceWall -= .1f;
            }
        }

        if (Random.value < baseChanceWall)
        {
            return _wallTilePrefab;
        }
        else
        {
            return _iceTilePrefab;
        }
    }
    


    public void HighlightPlayerStartingPositions() {
        Vector2[] startingTiles = PotentialPlayerStartingPositions();
        foreach (Vector2 tileCoord in startingTiles) {
            _tiles[tileCoord].HighlightStartingPlayerPosition();
        }
    }

    public void RemoveHighlightPlayerStartingPositions() {
        Vector2[] startingTiles = PotentialPlayerStartingPositions();
        foreach (Vector2 tileCoord in startingTiles) {
            _tiles[tileCoord].RemoveHighlightStartingPlayerPosition();
        }
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
    
    
    public bool IsOccupied(Vector2 coord) {
        return _tiles[coord].OccupiedUnit != null || !_tiles[coord].isWalkable;
    }

    public void MoveUnit(BaseUnit unit, Tile fromTile, Vector2 toCoord) {
        // update the tiles
        _tiles[toCoord].SetUnit(unit);

        // remove the old state
        fromTile.OccupiedUnit = null;
    }


    /// <summary>
    /// Destorys the enemy prefab and updates the player's state
    /// </summary>
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
    
    


    public void ShowMovementHelper(CombinedCard card, int movementIndex) {
        currentCard = card;
        // show helper text
        MenuManager.Instance.PlayCardInstructions(card, movementIndex);

        // wait for input (arrow keys can control movement)
        currentlyMoving = false;
        waitingForInput = true;

        ShowMovementArrows(card, movementIndex);

    }

    private Vector2[] PotentialPlayerStartingPositions()
    {
        Vector2[] startCoords = new Vector2[_width];
        for (int i = 0; i < _width; i++)
        {
            startCoords[i] = new Vector2(i, 0);
        }
        return startCoords;
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

    private bool ValidMove(GridMovement movement)
    {

        Tile playerTile = GetHeroTile();

        if (playerTile == null) { 
            return false;
        }

        return currentCard.movementCard.CanMoveForGridMovement(movement, currentCard.movementCard.movementIndex);
    } 


    private void MoveHero(GridMovement movement) {
        // current player
        Tile playerTile = GetHeroTile();

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
                //TODO eventually make it so the player cant got below the board 
                if (!_tiles[newPlayerPosition].isWalkable) {
                    // dont let player go into walls

                } else {
                    // valid move no combat
                    BaseUnit playerUnit = GetHeroUnit();
                    playerTile.OccupiedUnit = null;
                    
                    // if we are moving up then move the board down
                    if (movement == GridMovement.Up)
                    {
                        MovePlayerUpGrid();
                        // we dont want to move the player up the screen, so just make the assignments here
                        _tiles[newPlayerPosition].OccupiedUnit = playerUnit;
                        playerUnit.OccupiedTile = _tiles[newPlayerPosition];
                    }
                    else
                    {
                        
                        _tiles[newPlayerPosition].SetUnit(playerUnit);
                    }
                    
                    
                    
                }

                if (GetHeroTile() is GoalTile)
                {
                    WinLoseManager.Instance.GameWin();
                }
                else
                {
                    // send it back to the CardRule controller
                    CardRuleManager.Instance.DidCompleteMovement();
                }
            }
        } else {
            // invalid move
            Debug.Log($"GridManager: Invalid move");
            currentlyMoving = false;
        }
    }

    private void MovePlayerUpGrid()
    {

        GenerateRow();
        
        foreach (KeyValuePair<Vector2, Tile> kv in _tiles)
        {
            kv.Value.PlayMoveDownAnimation();
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


    private Vector2 TileAfterMovement(GridMovement movement) {
        Vector2 playerPosition = _tiles.Where(t=>(t.Value.OccupiedUnit != null) && t.Value.OccupiedUnit.Faction == Faction.Hero).ToList().First().Key;

        Debug.Log($"Player is at {(int)playerPosition.x}, {(int)playerPosition.y}");

        // this assumes that there will be no rounding errors for the float values
        int coordX = (int)playerPosition.x;
        int coordY = (int)playerPosition.y;

        if (movement == GridMovement.Left) {
            coordX = Mathf.Max(0, coordX-1);
            Debug.Log($"Movement is left. New PP is {(int)coordX}, {(int)coordY}");

        } else if (movement == GridMovement.Right) {
            coordX = Mathf.Min(_width-1, coordX+1);
            Debug.Log($"Movement is right. New PP is {(int)coordX}, {(int)coordY}");

        } else if (movement == GridMovement.Up) {
            coordY = Mathf.Min(_visibleRows-1, coordY+1);
            Debug.Log($"Movement is Up. New PP is {(int)coordX}, {(int)coordY}");

        } else if (movement == GridMovement.Down) {
            coordY = Mathf.Max(0, coordY-1);
            Debug.Log($"Movement is Down. New PP is {(int)coordX}, {(int)coordY}");

        } else if (movement == GridMovement.None)
        {
            // intentionally left blank
        }
        
        return new Vector2(coordX, coordY);
    }  

    private void ShowMovementArrows(CombinedCard card, int movementIndex) {
        movementArrowGameObject = Instantiate(arrowController);
        movementArrowController = movementArrowGameObject.GetComponent<MovementArrowController>();

        List<GridMovement> gridMovements = card.movementCard.GetGridMovement(movementIndex);
        movementArrowController.SetArrows(gridMovements);
        movementArrowController.OnArrowTapped += ArrowTapped;

        Tile playerTile = GetHeroTile();

        if (playerTile == null) {
            return;
        }

        // put the arrows on the player's tile
        movementArrowGameObject.transform.position = playerTile.OccupiedUnit.transform.position;

        // dont wait for input for the "stay in place"
        //TODO we should auto move the player if they choose a card with no choice
        if (gridMovements.Contains(GridMovement.None))
        {
            ArrowTapped(GridMovement.None);
        }
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
