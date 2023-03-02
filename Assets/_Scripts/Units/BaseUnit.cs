using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class BaseUnit : MonoBehaviour
{
    public Tile OccupiedTile;
    public Faction Faction;
    public string UnitName;

    public Sprite sprite;

    public int health;
    public int attack;
    public int armor;

    [SerializeField] public PlayableDirector PlayableDirector;
    public virtual bool ShouldAttack(Tile currentTile, Tile playerTile) {
        return false;
    }

    public virtual List<Vector2> AttackTiles(Tile currentTile) {
        return new List<Vector2>();
    }

    public virtual List<Vector2> WantToMoveTo(Tile currentTile, Tile playerTile) {
        return new List<Vector2>();
    }
    
    public void Play()
    {
        PlayableDirector.Play();
    }
}
