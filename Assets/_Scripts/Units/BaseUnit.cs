using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;

public class BaseUnit : MonoBehaviour
{
    public Tile OccupiedTile;
    public Faction Faction;
    public string UnitName;

    [SerializeField] private int _health;
    [SerializeField] private int _attack;
    [SerializeField] private int _armor;
    
    [SerializeField] private TextMeshPro _healthText;
    [SerializeField] private TextMeshPro _attackText;
    [SerializeField] private TextMeshPro _armorText;

    // Asset related to the timeline
    [SerializeField] private PlayableDirector _playableDirector;
    [SerializeField] private PlayableAsset _moveDownAnimation;
    [SerializeField] private PlayableAsset _attackAnimation;
    [SerializeField] private PlayableAsset _takesDamageAnimation;

    
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

    private bool _firstFrameHappened = false;

    private void Update()
    {
        if (!_firstFrameHappened)
        {
            _firstFrameHappened = true;
            SetStats();
        }
    }

    public void Clone(BaseUnit unit)
    {
        unit._armorText = _armorText;
        unit._attackText = _attackText;
        unit._healthText = _healthText;
        unit._armor = _armor;
        unit._health = _health;
        unit._attack = _attack;
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
    
    private void ShowHealthFor(int health)
    {
        _healthText.text = "" + health;
    }
    private void ShowAttack()
    {
        ShowAttackFor(Attack);
    }
    
    private void ShowAttackFor(int attack)
    {
        _attackText.text = "" + attack;
    }

    private void ShowArmor()
    {
        ShowArmorFor(Armor);
    }
    
    private void ShowArmorFor(int armor)
    {
        if (Faction == Faction.Hero)
        {
            _armorText.text = "" + armor;
        }
    }

    private int _expectedAnimnations = 0;
    public void AnimateStatChange(BaseUnit oldUnit, Action animationFinishedCallback)
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

        Color positiveChange = Color.green;
        Color negativeChange = Color.red;
        
        void CheckCompletion()
        {
            _expectedAnimnations--;
            if (_expectedAnimnations <= 0)
            {
                _attackText.color = Color.white;
                _armorText.color = Color.white;
                _healthText.color = Color.white;
                animationFinishedCallback.Invoke();
            }
        }


        if (attackChange != 0)
        {
            _expectedAnimnations++;
            _attackText.color = attackChange > 0 ? positiveChange : negativeChange;
            StartCoroutine(animationTimer(0.5f, CheckCompletion));
        }

        if (healthChange != 0)
        {
            _expectedAnimnations++;
            _healthText.color = healthChange > 0 ? positiveChange : negativeChange;
            StartCoroutine(animationTimer(0.5f, CheckCompletion));
        }
        
        if (armorChange != 0)
        {
            _expectedAnimnations++;
            _armorText.color = armorChange > 0 ? positiveChange : negativeChange;
            StartCoroutine(animationTimer(0.5f, CheckCompletion));
        }

        if (_expectedAnimnations == 0)
        {
            animationFinishedCallback.Invoke();
        }
    }

    private IEnumerator animationTimer(float timeToWait, Action callback)
    {
        yield return new WaitForSeconds(timeToWait);
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
    private Action _takesDamageCallback;
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

    public void PlayTakeDamageAnimation(Action completion)
    {
        _playableDirector.playableAsset = _takesDamageAnimation;
        _takesDamageCallback = completion;
        _playableDirector.Play();
        _playableDirector.stopped += DidStop;
    }

    private void DidStop(PlayableDirector director)
    {
        _playableDirector.stopped -= DidStop;
        _attackFinishedCallback?.Invoke();
        _attackFinishedCallback = null;
        _takesDamageCallback?.Invoke();
        _takesDamageCallback = null;
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
