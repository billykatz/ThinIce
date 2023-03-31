using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
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
    [SerializeField] private Vector2 _boardOffset;

    [SerializeField] private InputActionReference _movementLeft;
    [SerializeField] private InputActionReference _movementRight;
    [SerializeField] private InputActionReference _movementUp;
    [SerializeField] private InputActionReference _movementDown;

    [SerializeField] private GameAnimator _animator;
    [SerializeField] private FXView _moveToFXView;
    
    public int BottomMostRowIndex;
    
    // the number of visible rows
    private int _width;
    private int _visibleRows;
    private bool currentlyMoving;
    private bool waitingForInput;
    private int _consecutiveGeneratedRocks;
    private bool _stopGenerating { get { return _levelRules.LevelLength <= _levelRules.CurrentNumberRows; } }

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
        BottomMostRowIndex = 0;
        _movementLeft.action.performed += didTapLeft;
        _movementRight.action.performed += didTapRight;
        _movementUp.action.performed += didTapUp;
        _movementDown.action.performed += didTapDown;
    }

    private void OnDisable()
    {
        _movementLeft.action.performed -= didTapLeft;
        _movementRight.action.performed -= didTapRight;
        _movementUp.action.performed -= didTapUp;
        _movementDown.action.performed -= didTapDown;
    }
    
    private void didTapLeft(InputAction.CallbackContext ctx) => ArrowInputPerformed(ctx, GridMovement.Left);
    private void didTapRight(InputAction.CallbackContext ctx) => ArrowInputPerformed(ctx, GridMovement.Right);
    private void didTapUp(InputAction.CallbackContext ctx) => ArrowInputPerformed(ctx, GridMovement.Up);
    private void didTapDown(InputAction.CallbackContext ctx) => ArrowInputPerformed(ctx, GridMovement.Down);

    private void ArrowInputPerformed(InputAction.CallbackContext ctx, GridMovement gridMovement)
    {

        if (!currentlyMoving && ctx.performed)
        {
            currentlyMoving = true;
            ArrowTapped(gridMovement);
        }
    }
    


    // current player unit
    public BaseUnit GetHeroUnit() { return GetHeroTile().OccupiedUnit; }
    
    // current player tile
    public Tile GetHeroTile() { return _tiles.Where(t=>(t.Value.OccupiedUnit != null) && t.Value.OccupiedUnit.Faction == Faction.Hero).ToList().First().Value; }

    // grab all the enemy tiles
    public List<Tile> GetEnemyUnits() { return _tiles.Where(t=>(t.Value.OccupiedUnit != null) && t.Value.OccupiedUnit.Faction == Faction.Enemy).Select(s=>s.Value).ToList(); }
    
    public bool CheckForDeadEnemy() { return _tiles.Where(t=>(t.Value.OccupiedUnit != null) && t.Value.OccupiedUnit.Faction == Faction.Enemy && t.Value.OccupiedUnit.Health == 0).Count() > 0; }

    public void GenerateGrid() {
        _tiles = new Dictionary<Vector2, Tile>();
        for (int x = 0; x < _width; x++) {
            for (int y = 0; y < _visibleRows; y++) {
                //spawns an ice tile, for now
                Vector2 position = new Vector2(x, y);
                position += _boardOffset;
                var spawnTile = Instantiate(_iceTilePrefab, position, Quaternion.identity);
                spawnTile.name = $"Tile {x} {y}";
                Vector2 coord = new Vector2(x, y);
                spawnTile.Init(coord);
                
                _tiles[coord] = spawnTile;   
            }
        }
        
        // put the camera at the center of our grid
        _cam.transform.position = new Vector3((float)_width /2 - 0.5f, (float)_visibleRows / 2 - 1.75f, -10);

        GameManager.Instance.EndGameState(GameState.GenerateGrid);
    }

    public void GenerateRow()
    {
        // dont create rows if we are at the level length max
        if (_stopGenerating)
        {
            return;
        }
        
        
        // get the top left tile and calculate the new position for our Y coord.
        Vector2 topLeftTileCoord = new Vector2(0, _levelRules.CurrentNumberRows-1);
        float posY = _tiles[topLeftTileCoord].transform.position.y + 1 - _boardOffset.y;
        
        // store data about the new row
        Dictionary<Vector2, TileType> newRow = new Dictionary<Vector2, TileType>();

        // generate a ice tile prefab for each coord
        for (int x = 0; x < _width; x++)
        {
            // the coord
            Vector2 coord = new Vector2(x, _levelRules.CurrentNumberRows);
            newRow[coord] = TileType.Ice;   
        }

        // now we want to generate rocks
        // we hard code this to generate at most 2 rocks
        float baseChanceWall = _levelRules.BaseChanceSpawnWall;
        bool spawnWall = Random.value < baseChanceWall;
        // bool spawnAnotherWall = Random.value < baseChanceWall;
        HashSet<Vector2> wallSet = new HashSet<Vector2>();
        int numWalls = spawnWall ? 1 : 0;
        // numWalls += (spawnAnotherWall ? 1 : 0);
        if (numWalls > 0)
        {
            _consecutiveGeneratedRocks++;
            // quick and dirty way to prevent a unescapable level
            if (_consecutiveGeneratedRocks >= _width)
            {
                numWalls = 0;
            }
        }
        else
        {
            // TODO: might be worth considering increasing the change to spawn a wall here
            _consecutiveGeneratedRocks = 0;
        }
        while (wallSet.Count < numWalls)
        {
            int coordX = Random.Range(0, _width);
            Vector2 coord = new Vector2(coordX, _levelRules.CurrentNumberRows);
            wallSet.Add(coord);
            newRow[coord] = TileType.Wall;
            Debug.Log("Infinite loop, oops");
        }

        // Ice, Walls and Goals are the only 3 tile types so now we can instantiate them
        bool isEndRow = _levelRules.LevelLength == _levelRules.CurrentNumberRows + 1;
        foreach (Vector2 coordKey in newRow.Keys)
        {
            // the coord
            Vector2 newPosition = new Vector2(coordKey.x, posY);
            newPosition += _boardOffset;
            TileType tileType = newRow[coordKey];
            Tile prefab = tileType == TileType.Ice ? _iceTilePrefab : _wallTilePrefab;
            Tile spawnTile = Instantiate(isEndRow ? _goalTilePrefab : prefab, newPosition, Quaternion.identity);
            spawnTile.name = $"Tile {coordKey.x} {coordKey.y}";
            spawnTile.Init(coordKey);
            _tiles[coordKey] = spawnTile;
            
            spawnTile.PlayEntranceAnimation();
        }

        // update the state of the level
        _visibleRows = _levelRules.CurrentNumberRows;
        _levelRules.CurrentNumberRows++;
        
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
                BaseUnit hero = UnitManager.Instance.SpawnHeroUnit();
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
    /// Destroys the enemy prefab and updates the player's state
    /// </summary>
    public void KillEnemyAndMovePlayer() {
        Vector2 deadEnemy = _tiles.Where(t=>(t.Value.OccupiedUnit != null) && t.Value.OccupiedUnit.Faction == Faction.Enemy && t.Value.OccupiedUnit.Health == 0).ToList().First().Key;
        Tile playerTile = _tiles.Where(t=>(t.Value.OccupiedUnit != null) && t.Value.OccupiedUnit.Faction == Faction.Hero).ToList().First().Value;
        Debug.Log("There is a dead enemy");
        if (deadEnemy != null && playerTile != null) {
            Debug.Log("Enemy is dead and player is moving to their square");
            
            // destroy the unit that is there
            Destroy(_tiles[deadEnemy].OccupiedUnit.gameObject);

            // special case where we move up and kill an enemy
            if (deadEnemy.y == playerTile.y + 1)
            {
                MovePlayerUpGrid(GetHeroUnit(), playerTile, _tiles[deadEnemy]);
            }
            else
            {
                // move the player to the unit
                _tiles[deadEnemy].SetUnit(playerTile.OccupiedUnit);
                
                // remove player from old tile
                playerTile.OccupiedUnit = null;
            }
            
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

            Vector2 newPlayerCoord = TileAfterMovement(movement);
            if (DidInitiateCombat(newPlayerCoord)) {
                
                // we need to fight and then finish the movement.
                CardRuleStep newStep = new CardRuleStep();
                newStep.state = CardRuleState.Combat;
                newStep.attackerUnit = playerTile.OccupiedUnit;
                newStep.defenderUnit = _tiles[newPlayerCoord].OccupiedUnit;

                Debug.Log($"GridManager: Did initiate combat {playerTile.OccupiedUnit} {_tiles[newPlayerCoord].OccupiedUnit}");
                CardRuleManager.Instance.StartCardRuleStep(newStep);

            } else {
                Debug.Log($"GridManager: Did finish moving");

                if (playerTile.coord == newPlayerCoord || !_tiles[newPlayerCoord].isWalkable)
                {
                    // we played the "stay" card or we tryied to move into a non-walkable space, just finish the movement
                    FinishMovement();
                }
                else
                {
                    // valid move no combat
                    BaseUnit playerUnit = GetHeroUnit();
                    playerTile.OccupiedUnit = null;
                    if (movement == GridMovement.Up)
                    {
                        MovePlayerUpGrid(playerUnit, playerTile, _tiles[newPlayerCoord]);
                        FinishMovement();

                    }
                    else
                    {
                        // animate the hero to move up
                        AnimationData data = new AnimationData();
                        data.StartPosition = playerTile.transform.position;
                        data.EndPosition = _tiles[newPlayerCoord].transform.position;
                        _animator.Animate(playerUnit.gameObject, data, _moveToFXView, () =>
                        {
                            _tiles[newPlayerCoord].SetUnit(playerUnit);

                            FinishMovement();
                        });
                    }
                }

            }
        } else {
            // invalid move
            Debug.Log($"GridManager: Invalid move");
            currentlyMoving = false;
            
        }
        
    }

    private void FinishMovement()
    {
        currentlyMoving = false;
        // TODO: Remove hack that we call this from the grid manager
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

    /// <summary>
    /// We want to move the player up the grid and then do the following:
    /// - move all the tiles down to simulate that the player is moving up
    /// - generate a new row
    /// - potentially spawn enemies.  
    /// </summary>
    private void MovePlayerUpGrid(BaseUnit playerUnit, Tile oldPlayerTile, Tile newPlayerTile)
    {
        // we dont want to move the player up the screen, so just make the assignments here
        newPlayerTile.OccupiedUnit = playerUnit;
        playerUnit.OccupiedTile = newPlayerTile;
        oldPlayerTile.OccupiedUnit = null;

        if (_stopGenerating)
        {
            
            AnimationData data = new AnimationData();
            data.StartPosition = oldPlayerTile.transform.position;
            data.EndPosition = newPlayerTile.transform.position;
            _animator.Animate(playerUnit.gameObject, data, _moveToFXView, () =>
            {
                FinishMovement();
            });
            
            return;
        }

        GenerateRow();
        SpawnEnemies();
        
        foreach (Vector2 key in _tiles.Keys)
        {
            if (key.y == BottomMostRowIndex)
            {
                _tiles[key].PlayExitAnimation();
                
            } else
            {
                _tiles[key].PlayMoveDownAnimation();
            }
        }
        
        
        DestroyBottomRow(BottomMostRowIndex);
        
        // update the bottom most row
        BottomMostRowIndex++;

    }

    async void DestroyBottomRow(int index)
    {
        await Task.Delay(900);
        for (int i = 0; i < _width; i++)
        {
            Vector2 coord = new Vector2(i, index);
            _tiles[coord].DestroyTile();
            _tiles.Remove(coord);
        }

    }

    private int _consecutiveRowsWithSpawnedEnemy = 0;
    private void SpawnEnemies()
    {
        if (_stopGenerating)
        {
            return;
        }
        // check to see if we should spawn an enemy
        float baseChanceSpawnEnemy = _levelRules.BaseChanceSpawnEnemy;
        // increase or decrease the chanxe to spawn an enemy based on how many rows have been generated with an enemy or not
        baseChanceSpawnEnemy += (_levelRules.ChanceDeltaSpawnEnemy * _consecutiveRowsWithSpawnedEnemy);
        bool spawnEnemy = Random.value < baseChanceSpawnEnemy;
        if (spawnEnemy)
        {
            // yes we spawned an enemy, make this value lower so it is less likely to spawn one in the future
            _consecutiveRowsWithSpawnedEnemy--;
        }
        else
        {
            // no we didnt spawn one, ok, make this value higher so there is a better chance an enemy comes in the future
            _consecutiveRowsWithSpawnedEnemy++;
        }

        if (spawnEnemy)
        {
            BaseUnit enemy = UnitManager.Instance.CreateEnemyUnit();
            bool hasSpawned = false;
            while (!hasSpawned)
            {
                
                int randomX = Random.Range(0, _width);

                Vector2 enemyCoord = new Vector2(randomX, _visibleRows);
                if (_tiles[enemyCoord].tileType == TileType.Ice)
                {
                    BaseUnit instanitatedUnit = Instantiate(enemy, _tiles[enemyCoord].transform.position, Quaternion.identity);
                    _tiles[enemyCoord].SetUnit(instanitatedUnit);
                    hasSpawned = true;
                }
            }
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

        } else if (movement == GridMovement.Up)
        {
            coordY += 1;// Mathf.Min(_visibleRows-1, coordY+1);
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
            ArrowTappedWrapper(GridMovement.None);
        } else if (gridMovements.Count == 1)
        {
            ArrowTappedWrapper(gridMovements[0]);
        }
    }

    async void ArrowTappedWrapper(GridMovement gridMovement)
    {
        // this is just a hair longer than the tile down animation. 
        // but of a hack until we figure out the animation and animation completion hooks
        await Task.Delay(500);
        ArrowTapped(gridMovement);
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
