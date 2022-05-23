using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Tile : MonoBehaviour
{
    [SerializeField] protected SpriteRenderer _spriteRenderer;
    [SerializeField] private GameObject _highlight;
    [SerializeField] private GameObject _startingPositionHighlight;
    [SerializeField] private bool _isWalkable;
    [SerializeField] public string TileName;

    private int x, y;

    public BaseUnit OccupiedUnit;
    public bool isWalkable => (OccupiedUnit == null) && _isWalkable;

    public virtual void Init(int x, int y) {
        this.x = x;
        this.y = y;
        
    }

    private void OnMouseEnter() {
        _highlight.SetActive(true);

        if (OccupiedUnit != null) {
            MenuManager.Instance.ShowSelectedUnit(OccupiedUnit);
        } else {
            MenuManager.Instance.ShowSelectedUnit(null);
        }

        MenuManager.Instance.ShowSelectedTile(this);
    }
    private void OnMouseExit() {
        _highlight.SetActive(false);

        if (OccupiedUnit == null) {
            MenuManager.Instance.ShowSelectedUnit(null);
        }

        MenuManager.Instance.ShowSelectedTile(null);
    }

    private void OnMouseDown() {
        GridManager.Instance.TileWasTapped(x, y);
    }

    public void HighlightStartingPlayerPosition() {
        _startingPositionHighlight.SetActive(true);
    }

    public void RemoveHighlightStartingPlayerPosition() {
        _startingPositionHighlight.SetActive(false);
    }
    

    public void SetUnit(BaseUnit baseUnit) {
        if (baseUnit.OccupiedTile != null) { 
            baseUnit.OccupiedTile = null;
        }
        
        baseUnit.transform.position = transform.position;
        OccupiedUnit = baseUnit;
        baseUnit.OccupiedTile = this;
    }
}
