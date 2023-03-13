using System;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

[System.Serializable]
public enum TileType
{
    Ice = 1,
    Wall = 2, 
    Goal = 3
}

public abstract class Tile : MonoBehaviour
{
    [SerializeField] private PlayableDirector PlayableDirector;
    [SerializeField] private PlayableAsset MoveDownTimeline;
    [SerializeField] private PlayableAsset EntranceTimeline;
    
    [SerializeField] private GameObject _highlight;
    [SerializeField] private GameObject _startingPositionHighlight;
    [SerializeField] private bool _isWalkable;
    [SerializeField] public string TileName;
    [SerializeField] public TextMeshPro debugText;
    
    [SerializeField] public TileType tileType;

    [SerializeField] private InputActionReference DidSelect;
    [SerializeField] private InputActionReference MousePosition;

    [SerializeField] private Collider collider;
    public int x, y;
    public Vector2 coord {
        get {
            return new Vector2((float)x, (float)y);
        }
    }

    public BaseUnit OccupiedUnit;
    public bool isWalkable => (OccupiedUnit == null) && _isWalkable;

    public virtual void Init(Vector2 coord) {
        Init((int)coord.x, (int)coord.y);
        
    }
    public virtual void Init(int x, int y) {
        this.x = x;
        this.y = y;

        debugText.text = $"x:{x}, y:{y}";
        
    }

    private void Start()
    {
        DidSelect.action.performed += ctx => OnMouseDown();
        MousePosition.action.performed += ctx => MouseMoved();
    }

    public void PlayMoveDownAnimation()
    {
        PlayableDirector.playableAsset = MoveDownTimeline;
        PlayableDirector.Play();
        if (OccupiedUnit is BaseEnemy)
        {
            OccupiedUnit.Play();
        }
    }
    
    public void PlayEntranceAnimation()
    {
        PlayableDirector.playableAsset = EntranceTimeline;
        PlayableDirector.Play();
    }

    private void MouseMoved()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(mousePos.x, mousePos.y, 0.0f));
        RaycastHit hit;
        if (collider.Raycast(ray, out hit, Mathf.Infinity))
        {
            OnMouseEnter();
        }
        else
        {
            if (_highlight.activeSelf)
            {
                OnMouseExit();
            }
        }
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

    private void OnMouseDown()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(mousePos.x, mousePos.y, 0.0f));
        RaycastHit hit;
        if (collider.Raycast(ray, out hit, Mathf.Infinity))
        {
            GridManager.Instance.TileWasTapped(x, y);
            HandManager.Instance.DeselectAll();
        }
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
