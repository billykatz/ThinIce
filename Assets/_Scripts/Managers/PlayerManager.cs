using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public enum ModifyTarget {
    Armor = 0,
    Attack = 1,
    None = 2,
}

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;

    public int maxHp;
    public int minHp;
    public int maxArmor;
    public int minArmor;
    public int maxAttack;
    public int minAttack;

    [SerializeField] private GameObject _baseHeroPrefab;
    
    private BaseUnit heroUnit;
    private BaseUnit previewUnit;

    private float _lastTimePreviewShown = 0.25f;

    private void Awake() {
        Debug.Log("PlayerManager Awake()");
        Instance = this;
        previewUnit = Instantiate(_baseHeroPrefab, new Vector3(-100, -100, 0), Quaternion.identity).GetComponent<BaseUnit>();
    }

    public void HeroUnitUpdated() {
        heroUnit = GridManager.Instance.GetHeroUnit();
    }

    public void PlayerTakesDamage(int damage)
    {
        // save the old hero stats in this dummy class
        previewUnit = new BaseUnit();
        heroUnit.Clone(previewUnit);
        
        UpdateStatsBasedOnDamage(heroUnit, damage);
        heroUnit.AnimateStatChange(previewUnit, () => { });
    }

    public void CollectItem(BaseItem item, Action animationComplete)
    {
        // save the old hero stats in this dummy class
        previewUnit = new BaseUnit();
        heroUnit.Clone(previewUnit);

        // update the hero units
        heroUnit = UpdateStatsBasedOnItem(heroUnit, item);
        
        // animate the collection and then the stat change
        heroUnit.AnimateStatChange(previewUnit, () => { });
        item.PlayCollectedAnimation(() =>
        {
            item.OccupiedTile.OccupiedItem = null;
            Destroy(item.gameObject);
            animationComplete.Invoke();
        });
    }

    public void ShowModifierPreview(CombinedCard card, ModifyTarget target)
    {
        float previewThreshold = Time.time - _lastTimePreviewShown;
        if (previewThreshold > 0.25f)
        {
            _lastTimePreviewShown = Time.time;
        }
        else
        {
            return;
        }
        
        // destroy the last one
        if (previewUnit != null && previewUnit.gameObject != null)
        {
            heroUnit.gameObject.SetActive(true);
            Destroy(previewUnit.gameObject);
        }

        // set the spawn pos to be slightly closer to the camera above the actual unit
        Vector3 spawnPos = heroUnit.transform.position;
        heroUnit.gameObject.SetActive(false);
        previewUnit = Instantiate(_baseHeroPrefab, spawnPos, Quaternion.identity).GetComponent<BaseUnit>();

        int previousArmor = heroUnit.Armor;
        int previousHealth = heroUnit.Health;
        int previousAttack = heroUnit.Attack;
        
        previewUnit.Armor = previousArmor;
        previewUnit.Health = previousHealth;
        previewUnit.Attack = previousAttack;
        
        UpdateNewStats(card, target, previewUnit);

        // animate the change
        previewUnit.AnimateStatChange(heroUnit, () => { });

        if (target == ModifyTarget.None)
        {
            heroUnit.gameObject.SetActive(true);
            Destroy(previewUnit.gameObject);
        }
    }
    
    private void UpdateNewStats(CombinedCard card, ModifyTarget target, BaseUnit unit)
    {
        int currentAttack = unit.Attack;
        int currentArmor = unit.Armor;
        
        int newAttack = unit.Attack;
        int newArmor = unit.Armor;
        if (target == ModifyTarget.Attack)
        {
            newAttack = ComputeNewStat(currentAttack, minAttack, maxAttack, card);
        } else if (target == ModifyTarget.Armor)
        {
            newArmor = ComputeNewStat(currentArmor, minArmor, maxArmor, card);
        }

        unit.Armor = newArmor;
        unit.Attack = newAttack;
    }

    public void PlayedCard(CombinedCard card, ModifyTarget target) {
        
        // destroy the last one
        if (previewUnit != null && previewUnit.gameObject != null)
        {
            heroUnit.gameObject.SetActive(true);
            Destroy(previewUnit.gameObject);
        }
        
        UpdateNewStats(card, target, heroUnit);
    }

    private int ComputeNewStat(int currentStat, int minStat, int maxStat, CombinedCard card)
    {
        int newStat = currentStat;

        switch (card.modifierCard.modifyOperation) {
            case ModifyOperation.Add:
                newStat = Mathf.Min(maxStat, newStat + card.modifierCard.modifyAmount);
                break;
            case ModifyOperation.Subtract:
                newStat = Mathf.Max(minStat, newStat - card.modifierCard.modifyAmount);
                break;
            case ModifyOperation.Multiply:
                newStat = Mathf.Min(maxStat, newStat * card.modifierCard.modifyAmount);
                break;
            case ModifyOperation.Divide:
                newStat = Mathf.Max(minStat, newStat / card.modifierCard.modifyAmount);
                break;
            case ModifyOperation.ToMax:
                newStat = maxStat;
                break;
            case ModifyOperation.ToMin:
                newStat = minStat;
                break;
        }

        return newStat;
    }

    private void UpdateStatsBasedOnDamage(BaseUnit unit, int damage)
    {
        unit.Health -= damage;
        unit.Health = Mathf.Max(0, unit.Health);
    }
    private BaseUnit UpdateStatsBasedOnItem(BaseUnit unit, BaseItem item)
    {
        if (item.Stat == ItemStat.Armor)
        {
            unit.Armor = Mathf.Min(maxArmor, unit.Armor + item.Amount);
            unit.Armor = Mathf.Max(0, unit.Armor);
        }
        else if (item.Stat == ItemStat.Attack)
        {
            unit.Attack = Mathf.Min(maxAttack,  unit.Attack + item.Amount) ;
            unit.Attack = Mathf.Max(0, unit.Attack);
        }
        else if (item.Stat == ItemStat.Health)
        {
            unit.Health = Mathf.Min(maxHp,  unit.Health + item.Amount) ;
            unit.Health = Mathf.Max(0, unit.Health);
        }

        return unit;
    }
}
