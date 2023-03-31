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
    [SerializeField] private PlayableDirector _playableDirector;
    [SerializeField] private PlayableAsset MoveDownTimeline;
    [SerializeField] private PlayableAsset EntranceTimeline;
    [SerializeField] private PlayableAsset ExitTimeline;
    
    [SerializeField] private GameObject _highlight;
    [SerializeField] private GameObject _startingPositionHighlight;
    [SerializeField] private bool _isWalkable;
    [SerializeField] public string TileName;
    [SerializeField] public TextMeshPro debugText;
    
    [SerializeField] public TileType tileType;

    [SerializeField] private InputActionReference DidSelect;
    [SerializeField] private InputActionReference MousePosition;

    [SerializeField] private Collider collider;

    private Action _exitAnimationCallback;
    private Action _moveDownCallback;
    
    public int x, y;
    public Vector2 coord {
        get {
            return new Vector2((float)x, (float)y);
        }
    }

    public BaseUnit OccupiedUnit;
    public BaseItem OccupiedItem;
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
        DidSelect.action.performed += OnMouseDown;
        MousePosition.action.performed += MouseMoved;

    }

    private void OnEnable()
    {
        _playableDirector.stopped += DidStop;
    }

    private void OnDisable()
    {
        _playableDirector.stopped -= DidStop;
    }

    private void DidStop(PlayableDirector director)
    {
        if (director.playableAsset == MoveDownTimeline)
        { 
            _moveDownCallback?.Invoke();
        }

        if (director.playableAsset == ExitTimeline)
        {
            _exitAnimationCallback?.Invoke();
        }
    }

    private void OnDestroy()
    {
        DidSelect.action.performed -= OnMouseDown;
        MousePosition.action.performed -= MouseMoved;
    }

    public void PlayMoveDownAnimation(Action animationCompleteCallback)
    {
        _moveDownCallback = animationCompleteCallback;
        _playableDirector.playableAsset = MoveDownTimeline;
        _playableDirector.Play();
        if (OccupiedUnit is BaseEnemy)
        {
            OccupiedUnit.PlayMoveDownAnimation();
        }
    }
    
    public void PlayEntranceAnimation()
    {
        _playableDirector.playableAsset = EntranceTimeline;
        _playableDirector.Play();
    }

    public void PlayExitAnimation(Action animationCompleteCallback)
    {
        _exitAnimationCallback = animationCompleteCallback;
        _playableDirector.playableAsset = ExitTimeline;
        _playableDirector.Play();
        if (OccupiedUnit is BaseEnemy)
        {
            OccupiedUnit.PlayMoveDownAnimation();
        }
    }

    private void MouseMoved(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
            return;
        
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

    private void OnMouseDown(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
            return;
        
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
        // remove the reference from the old tile to this unit
        if (baseUnit.OccupiedTile)
        {
            baseUnit.OccupiedTile.OccupiedUnit = null;
        }
        
        // update our position
        baseUnit.transform.position = transform.position;
        
        // update this tile's storage of the occupied unit
        OccupiedUnit = baseUnit;
        
        // update the unit to point to this tile
        baseUnit.OccupiedTile = this;
    }
    
    /// <summary>
    ///  Called to set the item on the tile.
    /// </summary>
    /// <param name="baseItem"></param>
    public void SetItem(BaseItem baseItem) {
        baseItem.transform.position = transform.position;
        OccupiedItem = baseItem;
        baseItem.OccupiedTile = this;
        OccupiedItem.transform.parent = transform;
    }

    public void DestroyTile()
    {
        if (OccupiedUnit != null)
        {
            OccupiedUnit.OccupiedTile = null;
            EnemiesManager.Instance.DestroyEnemyOnTile(this);
        }

        OccupiedUnit = null;
        Destroy(this.gameObject);

    }
}
