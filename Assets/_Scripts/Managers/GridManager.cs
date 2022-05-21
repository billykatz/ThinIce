using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
                var shouldBeWall = (y == _height-1);
                var spawnTile = Instantiate(shouldBeWall ? _wallTilePrefab : _iceTilePrefab, new Vector3(x, y), Quaternion.identity);
                spawnTile.name = $"Tile {x} {y}";
                spawnTile.Init(x, y);


                _tiles[new Vector2(x, y)] = spawnTile;   
            }
        }


        // put the camera at the center of our grid
        _cam.transform.position = new Vector3((float)_width /2 - 0.5f, (float)_height / 2 - 0.5f, -10);

        GameManager.Instance.EndGameState(GameState.GenerateGrid);
    }

    public Tile GetTilePosition(Vector2 pos) {
        if (_tiles.TryGetValue(pos, out var tile)) {
            return tile;
        }

        return null;
    }

    public Tile GetEnemySpawnTile() {
        return _tiles.Where(t => t.Value.isWalkable).OrderBy(o=>Random.value).First().Value;
    }
}
