using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class BaseUnit : MonoBehaviour
{
    public Tile OccupiedTile;
    public Faction Faction;
    public string UnitName;

    public Sprite sprite;
    
    [SerializeField] private int _health;
    [SerializeField] private int _attack;
    [SerializeField] private int _armor;
    public int Health {
     get { return _health; }
     set
     {
         _health = value;
         ShowHealth();
     }
    }
    public int Attack
    {
        get { return _attack; }
        set
        {
            _attack = value;
            ShowAttack();
        }
    }
    public int Armor
    {
        get { return _armor; }
        set
        {
            _armor = value;
            ShowArmor();
        }
    }

    // Asset related to the timeline
    [SerializeField] private PlayableDirector _playableDirector;
    [SerializeField] private PlayableAsset _moveDownAnimation;
    [SerializeField] private PlayableAsset _attackAnimation;

    // gameobjects representing the stat numbers
    [SerializeField] private GameObject[] _swordNumbers;
    [SerializeField] private GameObject[] _armorNumbers;
    [SerializeField] private GameObject[] _hpNumbers;
    
    
    private bool _firstFrameHappened = false;

    private void Update()
    {
        if (!_firstFrameHappened)
        {
            _firstFrameHappened = true;
            SetStats();
        }
    }

    private void SetStats()
    {
        ShowHealth();
        ShowAttack();
        ShowArmor();
    }
    
    private void ShowHealth()
    {
        ShowHealthFor(Health);
    }
    
    private void ShowHealthFor(int index)
    {
        for (int i = 0; i < _hpNumbers.Length; i++)
        {
            _hpNumbers[i].SetActive(i == index);
        }
    }
    private void ShowAttack()
    {
        ShowAttackFor(Attack);
    }
    
    private void ShowAttackFor(int index)
    {
        for (int i = 0; i < _swordNumbers.Length; i++)
        {
            _swordNumbers[i].SetActive(i == index);
        }
    }

    private void ShowArmor()
    {
        ShowArmorFor(Armor);
    }
    
    private void ShowArmorFor(int index)
    {
        if (Faction == Faction.Hero)
        {
            for (int i = 0; i < _armorNumbers.Length; i++)
            {
                _armorNumbers[i].SetActive(i == index);
            }
        }
    }

    public void AnimateStatChange(BaseUnit oldUnit)
    {
        int oldAttack = oldUnit.Attack;
        int attackChange = _attack - oldAttack;
        ShowArmorFor(_attack);

        int oldHealth = oldUnit.Health;
        int healthChange = _health - oldHealth;
        ShowHealthFor(_health);
        
        int oldArmor = oldUnit.Armor;
        int armorChange = _armor - oldArmor;
        ShowArmorFor(_armor);
    }

    private IEnumerator animationTimer(int index, Action callback)
    {
        yield return new WaitForSeconds(index * 0.08f);
        callback?.Invoke();
    }

    public virtual bool ShouldAttack(Tile currentTile, Tile playerTile) {
        return false;
    }

    public virtual List<Vector2> AttackTiles(Tile currentTile) {
        return new List<Vector2>();
    }

    public virtual List<Vector2> WantToMoveTo(Tile currentTile, Tile playerTile) {
        return new List<Vector2>();
    }

    private Action _attackFinishedCallback;
    private Action _attackHitCallback;
    public void PlayMoveDownAnimation()
    {
        _playableDirector.playableAsset = _moveDownAnimation;
        _playableDirector.Play();
        _playableDirector.stopped += DidStop;
    }
    
    public void PlayAttackAnimation(Action attackHitCallback, Action attackFinishedCallback)
    {
        _playableDirector.playableAsset = _attackAnimation;
        _playableDirector.Play();
        _attackHitCallback = attackHitCallback;
        _attackFinishedCallback = attackFinishedCallback;
        _playableDirector.stopped += DidStop;
    }

    private void DidStop(PlayableDirector director)
    {
        _playableDirector.stopped -= DidStop;
        _attackFinishedCallback?.Invoke();
        _attackFinishedCallback = null;
    }

    /// <summary>
    ///  called by a signal emitter on the timeline to let us know when the attack hit
    /// </summary>
    public void AttackDidHit()
    {
        _attackHitCallback?.Invoke();
        _attackHitCallback = null;
    }
    
    
}
