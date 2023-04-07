using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;
using Extensions;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;
    [SerializeField] private CurrentLevelReference _level;
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

    [SerializeField] private GameObject _levelBackground;
    [SerializeField] private GameObject _handBackground;
    
    public int BottomMostRowIndex;
    
    // the number of visible rows
    private int _width;
    private int _visibleRows;
    private bool currentlyMoving;
    private bool waitingForInput;
    private int _consecutiveGeneratedRocks;
    private bool _stopGenerating { get { return _level.LevelRules.Rows.Length <= _level.LevelRules.CurrentNumberRows; } }

    private ScriptableRow[] _scriptableRows;
    private Dictionary<Vector2, Tile> _tiles;
    private CombinedCard currentCard;
    private MovementArrowController movementArrowController;
    private GameObject movementArrowGameObject;

    private void Awake() {
        Instance = this;

        Debug.Log("Grid Manager Awake()");

        _scriptableRows = _level.LevelRules.Rows;

    }

    private void Start()
    {
        _visibleRows = _level.LevelRules.StartingRows;
        _width = _level.LevelRules.Width;
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
    public List<Tile> GetEnemyTiles() { return _tiles.Where(t=>(t.Value.OccupiedUnit != null) && t.Value.OccupiedUnit.Faction == Faction.Enemy).Select(s=>s.Value).ToList(); }
    public List<BaseUnit> GetEnemyUnits() { return _tiles.Where(t=>(t.Value.OccupiedUnit != null) && t.Value.OccupiedUnit.Faction == Faction.Enemy).Select(s=>s.Value.OccupiedUnit).ToList(); }
    public List<Tile> GetHazardTiles() { return _tiles.Where(t=>(t.Value.OccupiedHazard != null)).Select(s=>s.Value).ToList(); }
    public List<Tile> GetItemTiles() { return _tiles.Where(t=>(t.Value.OccupiedItem != null)).Select(s=>s.Value).ToList(); }
    
    public bool CheckForDeadEnemy() { return _tiles.Where(t=>(t.Value.OccupiedUnit != null) && t.Value.OccupiedUnit.Faction == Faction.Enemy && t.Value.OccupiedUnit.Health == 0).Count() > 0; }
    public bool CheckForDeadHero() { return _tiles.Where(t=>(t.Value.OccupiedUnit != null) && t.Value.OccupiedUnit.Faction == Faction.Hero && t.Value.OccupiedUnit.Health == 0).Count() > 0; }

    /// <summary>
    /// Called by the game manager to load the tiles based on the scriptable level rules
    /// </summary>
    public void LoadGrid() {
        // ok now we want this load from preset level.
        _tiles = new Dictionary<Vector2, Tile>();
        for (int x = 0; x < _width; x++) {
            for (int y = 0; y < _visibleRows; y++) {
                // spawns an ice tile, for now
                Vector2 position = new Vector2(x, y);
                position += _boardOffset;
                GameObject prefab = _scriptableRows[y].Row.Tiles[x].TilePrefab;
                GameObject spawnedObject = Instantiate(prefab, position, Quaternion.identity);
                spawnedObject.name = $"Tile {x} {y}";
                Vector2 coord = new Vector2(x, y);
                Tile spawnedTile = spawnedObject.GetComponent<Tile>();
                spawnedTile.Init(coord);
                
                _tiles[coord] = spawnedTile;   
            }
        }
        
        // put the camera at the center of our grid
        Vector3 center = new Vector3((float)_width /2 - 0.5f, (float)_visibleRows / 2 - 1.75f, -10);
        _cam.transform.position = center;
        Vector3 backgroundCenter = center;
        backgroundCenter.z = 1.5f;
        _levelBackground.transform.position = backgroundCenter;
        Vector3 handAreaBackgroundPos = backgroundCenter;
        handAreaBackgroundPos.y -= 4.5f;
        handAreaBackgroundPos.z = 0f;
        _handBackground.transform.position = handAreaBackgroundPos;

        GameManager.Instance.EndGameState(GameState.GenerateGrid);
    }

    /// <summary>
    /// Called by the game manager to load enemies based on the scriptable level rules
    /// </summary>
    public void LoadEnemies()
    {
        for (int x = 0; x < _width; x++) {
            for (int y = 0; y < _visibleRows; y++) {
                // spawns an ice tile, for now
                Vector2 position = new Vector2(x, y);
                position += _boardOffset;
                Vector2Int coord = new Vector2Int(x, y);
                
                // grab the prefab
                ScriptableUnit unit = _scriptableRows[y].Row.Tiles[x].Unit;
                if (unit)
                {
                    // spawn the unit
                    SpawnUnit(unit.UnitPrefab, position, coord);
                }
                
            }
        }
        DrawEnemiesAttackIndicators();
        GameManager.Instance.EndGameState(GameState.SpawnEnemies);
    }

    /// <summary>
    /// Called by the game manager to load enemies based on the scriptable level rules
    /// </summary>
    public void LoadHazards()
    {
        for (int x = 0; x < _width; x++) {
            for (int y = 0; y < _visibleRows; y++) {
                // spawns an ice tile, for now
                Vector2 position = new Vector2(x, y);
                position += _boardOffset;
                Vector2Int coord = new Vector2Int(x, y);
                
                // grab the prefab
                ScriptableHazards hazard = _scriptableRows[y].Row.Tiles[x].Hazard;
                if (hazard)
                {
                    // spawn the unit
                    SpawnHazard(hazard, hazard.HazardPrefab, position, coord);
                }
                
            }
        }
        GameManager.Instance.EndGameState(GameState.SpawnHazards);
    }

    public void LoadItems()
    {
        for (int x = 0; x < _width; x++) {
            for (int y = 0; y < _visibleRows; y++) {
                Vector2 position = new Vector2(x, y);
                position += _boardOffset;
                Vector2Int coord = new Vector2Int(x, y);
                
                // grab the prefab
                ScriptableItem item = _scriptableRows[y].Row.Tiles[x].Item;
                if (item)
                {
                    SpawnItem(item, item.ItemPrefab, position, coord);
                }
            }
        }
        GameManager.Instance.EndGameState(GameState.SpawnItems);
    }

    private void SpawnUnit(BaseUnit prefab, Vector3 position, Vector2Int coord)
    {
        BaseUnit spawnedEnemy = Instantiate(prefab, position, Quaternion.identity);
        _tiles[coord].SetUnit(spawnedEnemy);
    }

    private void SpawnItem(ScriptableItem scriptableItem, BaseItem item, Vector3 position, Vector2Int coord)
    {
        BaseItem prefab = Instantiate(item, position, Quaternion.identity);
        prefab.Setup(scriptableItem.stat, scriptableItem.amount, scriptableItem.Name);
        _tiles[coord].SetItem(prefab);
        
        
    }
    private void SpawnHazard(ScriptableHazards scriptableHazards, BaseHazard hazard, Vector3 position, Vector2Int coord)
    {
        BaseHazard prefab = Instantiate(hazard, position, Quaternion.identity);
        prefab.Configure(scriptableHazards);
        _tiles[coord].SetHazard(prefab);
        
    }

    private void GenerateRow()
    {
        // dont create rows if we are at the level length max
        if (_stopGenerating)
        {
            return;
        }

        // get the top left tile and calculate the new position for our Y coord.
        Vector2 topLeftTileCoord = new Vector2(0, _level.LevelRules.CurrentNumberRows-1);
        float posY = _tiles[topLeftTileCoord].transform.position.y + 1 - _boardOffset.y;

        for (int x = 0; x < _width; x++)
        {
            // create the coord
            Vector2Int coord = new Vector2Int(x, _level.LevelRules.CurrentNumberRows);
            
            // calculcate the screen position
            Vector2 newPosition = new Vector2(coord.x, posY);
            newPosition += _boardOffset;
            
            // scriptable tile
            ScriptableTile tile = _scriptableRows[coord.y].Row.Tiles[coord.x];
            
            // get and instantiate the prefab
            GameObject tilePrefab = tile.TilePrefab;
            Tile spawnTile = Instantiate(tilePrefab, newPosition, Quaternion.identity).GetComponent<Tile>();
            
            // set up the tile game object
            spawnTile.name = $"Tile {coord.x} {coord.y}";
            spawnTile.Init(coord);
            _tiles[coord] = spawnTile;
            
            // play the animation
            spawnTile.PlayEntranceAnimation();

            if (tile.Unit != null)
            {
                SpawnUnit(tile.Unit.UnitPrefab, newPosition, coord);
            }
            
            if (tile.Item != null)
            {
                SpawnItem(tile.Item, tile.Item.ItemPrefab, newPosition, coord);
            }
            
            if (tile.Hazard != null)
            {
                SpawnHazard(tile.Hazard, tile.Hazard.HazardPrefab, newPosition, coord);
            }
        }

        // update the state of the level
        _visibleRows = _level.LevelRules.CurrentNumberRows;
        _level.LevelRules.CurrentNumberRows++;
        
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
    
    public void TileWasTapped(int x, int y) {
        Vector2 tappedCoord = new Vector2(x, y);
        if (GameManager.Instance.GameState == GameState.PlaceHero) {
            if (PotentialPlayerStartingPositions().Contains(tappedCoord)) {
                Debug.Log($"Tile at {x}, {y} was tapped");
                BaseUnit hero = UnitManager.Instance.SpawnHeroUnit();
                PlayerManager.Instance.SetPlayerStartingStats(hero);
                _tiles[tappedCoord].SetUnit(hero);
                hero.OccupiedTile = _tiles[tappedCoord];

                // remove the special highlights
                RemoveHighlightPlayerStartingPositions();

                // we are done placing the hero, move the game along
                GameManager.Instance.EndGameState(GameState.PlaceHero);
            } else {
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
    public void KillEnemyAndMovePlayer(Action animationCompleteCallback) {
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
                MovePlayerUpGrid(GetHeroUnit(), playerTile, _tiles[deadEnemy], animationCompleteCallback);
            }
            else
            {
                // move the player to the unit
                _tiles[deadEnemy].SetUnit(playerTile.OccupiedUnit);
                
                // remove player from old tile
                playerTile.OccupiedUnit = null;
                
                animationCompleteCallback.Invoke();
            }
            
        }
        else
        {
            animationCompleteCallback.Invoke();
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


    /// <summary>
    /// This function is responsible for resolving game rules after moving the hero.
    /// It can either:
    /// - initiated combat by sending a new CardRuleStep to the CardRuleManager
    /// - or
    /// - move the player on the grid 
    /// </summary>
    /// <param name="movement"></param>
    /// <param name="animationFinishedCallback"></param>
    private void MoveHero(GridMovement movement, bool playerWasBounced, Action animationFinishedCallback) {
        // current player
        Tile playerTile = GetHeroTile();

        if (ValidMove(movement) || playerWasBounced) {
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

            }
            else {
                Debug.Log($"GridManager: Did finish moving");

                if (playerTile.coord == newPlayerCoord || !_tiles[newPlayerCoord].isWalkable)
                {
                    // player played the "stay" card or player tried to move into a non-walkable space, just finish the movement
                    FinishMovement();
                }
                else
                {
                    // valid move no combat
                    BaseUnit playerUnit = GetHeroUnit();
                    playerTile.OccupiedUnit = null;
                    if (movement == GridMovement.Up)
                    {
                        MovePlayerUpGrid(playerUnit, playerTile, _tiles[newPlayerCoord], animationFinishedCallback);
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

    public bool CheckForWin()
    {
        return GetHeroTile() is GoalTile;
    }
    
    public void TriggerWin()
    {
        WinLoseManager.Instance.GameWin();
    }

    private void FinishMovement()
    {
        Debug.Log("Did finish movement");
        currentlyMoving = false;
        // TODO: Remove hack that we call this from the grid manager
        if (CheckForWin())
        {
            WinLoseManager.Instance.GameWin();
        }
        else
        {
            // send it back to the CardRule controller
            CardRuleManager.Instance.DidCompleteMovement();
        }
    }

    
    int _expectedCount = 0;
    /// <summary>
    /// We want to move the player up the grid and then do the following:
    /// - move all the tiles down to simulate that the player is moving up
    /// - generate a new row
    /// - potentially spawn enemies.  
    /// </summary>
    private void MovePlayerUpGrid(BaseUnit playerUnit, Tile oldPlayerTile, Tile newPlayerTile, Action completion)
    {
        // we dont want to move the player up the screen, so just make the assignments here
        newPlayerTile.OccupiedUnit = playerUnit;
        playerUnit.OccupiedTile = newPlayerTile;
        oldPlayerTile.OccupiedUnit = null;

        if (_stopGenerating)
        {
            AnimatePlayerMovement(playerUnit, oldPlayerTile, newPlayerTile, completion);

            return;
        }

        GenerateRow();

        // play animations and wait until the last one finishes, then call the completion
        foreach (Vector2 key in _tiles.Keys)
        {
            _expectedCount++;
            if (key.y == BottomMostRowIndex)
            {
                _tiles[key].PlayExitAnimation(() =>
                {
                    _expectedCount--;
                    if (_expectedCount == 0)
                    {
                        DestroyBottomRow(BottomMostRowIndex);
                        // update the bottom most row
                        BottomMostRowIndex++;
                        DrawEnemiesAttackIndicators();
                        completion.Invoke();
                    }
                });

            } else
            {
                _tiles[key].PlayMoveDownAnimation(() =>
                {
                    _expectedCount--;
                    if (_expectedCount == 0)
                    {
                        DestroyBottomRow(BottomMostRowIndex);
                        // update the bottom most row
                        BottomMostRowIndex++;
                        DrawEnemiesAttackIndicators();
                        completion.Invoke();
                    }
                });
            }
        }

    }

    private void AnimatePlayerMovement(BaseUnit playerUnit, Tile oldPlayerTile, Tile newPlayerTile, Action completion)
    {
        AnimationData data = new AnimationData();
        data.StartPosition = oldPlayerTile.transform.position;
        data.EndPosition = newPlayerTile.transform.position;
        _animator.Animate(playerUnit.gameObject, data, _moveToFXView, () => { completion.Invoke(); });
    }
    
    public void AnimateEnemyMovement(BaseUnit unit, Tile oldTile, Vector2 toCoord, Action completion)
    {
        AnimationData data = new AnimationData();
        data.StartPosition = oldTile.transform.position;
        data.EndPosition = _tiles[toCoord].transform.position;
        _animator.Animate(unit.gameObject, data, _moveToFXView, () =>
        {
            // update data store
            MoveUnit(unit, oldTile, toCoord);
            // update the UI
            DrawEnemyAttackIndicators(unit);
            completion.Invoke();
        });
    }

    public void DrawEnemyAttackIndicators(BaseUnit unit)
    {
        int width = _level.LevelRules.Width;
        int minHeight = BottomMostRowIndex;
        int maxHeight = _level.LevelRules.CurrentNumberRows;
        unit.ToggleAttackIndicators(false);
        for (int i = 0; i < unit.AttackVectors.Length; i++)
        {
            Vector2 checkCoord = unit.OccupiedTile.coord + unit.AttackVectors[i];
            bool isInBounds = checkCoord.IsInBounds(width, minHeight, maxHeight);
            if (isInBounds)
            {
                unit.ToggleAttackIndicator(unit.AttackVectors[i], _tiles[checkCoord].tileType != TileType.Wall);
            }
        }
    }
    public void DrawEnemiesAttackIndicators()
    {
        List<BaseUnit> enemies = GetEnemyUnits();
        for (int i = 0; i < enemies.Count; i++)
        {
            DrawEnemyAttackIndicators(enemies[i]);
        }
    }

    private void DestroyBottomRow(int index)
    {
        for (int i = 0; i < _width; i++)
        {
            Vector2 coord = new Vector2(i, index);
            _tiles[coord].DestroyTile();
            _tiles.Remove(coord);
        }

    }

    private bool DidInitiateCombat(Vector2 targetTile)
    {
        return _tiles[targetTile].OccupiedUnit != null && _tiles[targetTile].OccupiedUnit.Faction != Faction.Hero;
    }
    
    private bool DidCollectItem(Vector2 targetTile) {
        return _tiles[targetTile].OccupiedItem != null;
    }

    public bool ShouldCollectItem()
    {
        return GetHeroTile().OccupiedItem != null;
    }
    
    public bool ShouldResolveHazard()
    {
        return GetHeroTile().OccupiedHazard != null;
    }
    
    public void ResolveHazard()
    {
        Vector2 coord = GetHeroTile().coord;
        // We landed on an item to collect, good to give this to the player before combat
        CardRuleStep newStep = new CardRuleStep();
        newStep.state = CardRuleState.Hazard;
        newStep.attackerUnit = GetHeroUnit();
        newStep.hazard = _tiles[coord].OccupiedHazard;
                
        Debug.Log($"GridManager: Did step on hazard { coord } {_tiles[coord].OccupiedUnit}");
        CardRuleManager.Instance.StartCardRuleStep(newStep);
    }

    public void ResolveHazard(BaseHazard harazrd, BaseUnit unit, Tile fromTile, Action completion)
    {
        if (harazrd.HazardType == HazardType.Movement)
        {
            // then we need something that moves the player and calls a completion
            MoveHero(harazrd.Movements[0], true, completion);
        } 
        else if (harazrd.HazardType == HazardType.Spikes)
        {
            // then we need to damage the player and then cal the completion
            unit.PlayTakeDamageAnimation(() =>
            {
                PlayerManager.Instance.PlayerTakesDamage(harazrd.Damage);
                completion.Invoke();
            });
        }
    }
    
    public void CollectItemAfterCombat()
    {
        Vector2 coord = GetHeroTile().coord;
        // We landed on an item to collect, good to give this to the player before combat
        CardRuleStep newStep = new CardRuleStep();
        newStep.state = CardRuleState.Collect;
        newStep.attackerUnit = GetHeroUnit();
        newStep.collectedItem = _tiles[coord].OccupiedItem;
                
        Debug.Log($"GridManager: Did collect item at { coord } {_tiles[coord].OccupiedUnit}");
        CardRuleManager.Instance.StartCardRuleStep(newStep);
    }
    
    /// <summary>
    /// Called by the card rule step to move the player to where the item is
    /// </summary>
    /// <param name="targetTile"></param>
    /// <returns></returns>
    public void CollectItem(BaseUnit unit, Tile fromTile, Tile targetTile, Action movementCompleteCallback)
    {
        AnimatePlayerMovement(unit, fromTile, targetTile, () =>
        {
            targetTile.SetUnit(unit);
            movementCompleteCallback.Invoke();
        });    
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
            ArrowTapped(GridMovement.None);
        } else if (gridMovements.Count == 1)
        {
            ArrowTapped(gridMovements[0]);
        }
    }

    public void ArrowTapped(GridMovement gridMovement) {
        if (!waitingForInput) { return; }
        if (ValidMove(gridMovement)) {
            movementArrowController.OnArrowTapped -= ArrowTapped;
            Destroy(movementArrowGameObject);
        }
        MoveHero(gridMovement, false,() =>
        {
            FinishMovement();
        });
    }
}
