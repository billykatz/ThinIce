using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;
    [SerializeField] private int _width, _height;
    [SerializeField] private Tile _iceTilePrefab;
    [SerializeField] private Tile _wallTilePrefab;

    [SerializeField] private Transform _cam;


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
        _cam.transform.position = new Vector3((float)_width /2 - 0.5f, (float)_height / 2 - 1.5f, -10);

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
}
