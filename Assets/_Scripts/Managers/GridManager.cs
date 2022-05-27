using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;
    [SerializeField] private int _width, _height;
    [SerializeField] private Tile _iceTilePrefab;
    [SerializeField] private Tile _wallTilePrefab;

    [SerializeField] private Transform _cam;

    [SerializeField] private GameObject arrowController;

    private Dictionary<Vector2, Tile> _tiles;

    private void Awake() {
        Instance = this;
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
        if (waitingForInput && !currentlyMoving) {
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) {
                // move left
                Debug.Log("moved left");
                currentlyMoving = true;
                MoveHero(GridMovement.Left);
            } else if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) {
                // move up
                Debug.Log("moved up");
                currentlyMoving = true;
                MoveHero(GridMovement.Up);
            } else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) {
                // move down
                Debug.Log("moved down");
                currentlyMoving = true;
                MoveHero(GridMovement.Down);
            }  else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) {
                // move right
                Debug.Log("moved right");
                currentlyMoving = true;
                MoveHero(GridMovement.Right);
            }
        } else {
            if (Input.GetKeyDown(KeyCode.P)) {
                Debug.Log($"waiting for input {waitingForInput}.  Currently Moving? {currentlyMoving}");
                // waitingForInput = true;
            }
        }
        
    }

    private void MoveHero(GridMovement movement) {
        // current palyer
        Tile playerTile = _tiles.Where(t=>(t.Value.OccupiedUnit != null) && t.Value.OccupiedUnit.Faction == Faction.Hero).ToList().First().Value;

        if (playerTile == null) { 
            Debug.Log($"player tile is null.");
            waitingForInput = true;
            currentlyMoving = false;
            return; 
        }

        if (currentCard.movementCard.CanMoveForGridMovement(movement, movementCardIndex)) {
            Debug.Log($"Valid movement.");
            // valid move
            Vector2 newPlayerPosition = TileAfterMovement(movement);
            _tiles[newPlayerPosition].SetUnit(playerTile.OccupiedUnit);
            playerTile.OccupiedUnit = null;

            // update movement index
            movementCardIndex++;


            if (movementCardIndex >= currentCard.movementCard.movement.Count) {
                Debug.Log($"Card should be done.");
                FinishPlayingCard();

            } else {
                Debug.Log($"nmore stuff to happen.");
                ShowMovementHelper(currentCard, movementCardIndex);
                waitingForInput = true;
                currentlyMoving = false;
            }
        } else {
            // invalid move
            Debug.Log($"Invalid move");
            waitingForInput = true;
            currentlyMoving = false;
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

        } else if (movement == GridMovement.Down-1) {
            coordY = Mathf.Max(0, coordY);
            Debug.Log($"Movement is Down. New PP is {(int)coordX}, {(int)coordY}");

        }

        
        return new Vector2(coordX, coordY);
    }  


    private bool waitingForInput;
    private int movementCardIndex = 0;
    private CombinedCard currentCard;
    private bool currentlyMoving;
    public void PlayedCard(CombinedCard card) {
        Debug.Log($"First call to play card {card}");
        // keep track of the card
        currentCard = card;

        // show text 
        ShowMovementHelper(card, movementCardIndex);
    }

    public void ShowMovementHelper(CombinedCard card, int movementIndex) {

        // show helper text
        MenuManager.Instance.PlayCardInstructions(card, movementCardIndex);
        


        // wait for input (arrow keys can control movement)
        waitingForInput = true;
        currentlyMoving = false;

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
        MoveHero(gridMovement);
        movementArrowController.OnArrowTapped -= ArrowTapped;
        Destroy(movementArrowGameObject);
    }


    private void FinishPlayingCard() {
        Debug.Log($"Is finished playing cards");
        waitingForInput = false;
        movementCardIndex = 0;
        currentlyMoving = false;

        HandManager.Instance.DidFinishPlayingCard(currentCard);
        currentCard = null;
    }
}
public enum GridMovement {
    Left = 0,
    Right = 1, 
    Up = 2,
    Down = 3
}