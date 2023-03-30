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

    [SerializeField] private GameObject currentPlayerStats;
    [SerializeField] private Text healthText;
    [SerializeField] private Text armorText;
    [SerializeField] private Text attackText;
    
    [SerializeField] private Color increaseColor;
    [SerializeField] private Color decreaseColor;
    [SerializeField] private Color noChangeColor;
    [SerializeField] private GameObject _baseHeroPrefab;
    
    private BaseUnit heroUnit;
    private BaseUnit previewUnit;

    private float _lastTimePreviewShown = 0.25f;

    private int armor {
        get {
            return heroUnit.Armor;
        }
    }

    private int attack {
        get {
            return heroUnit.Attack;
        }
    }

    private void Awake() {
        Debug.Log("PlayerManager Awake()");
        Instance = this;

        healthText.text = $"-";
        armorText.text = $"-";
        attackText.text = $"-";
        previewUnit = Instantiate(_baseHeroPrefab, new Vector3(-100, -100, 0), Quaternion.identity).GetComponent<BaseUnit>();
    }

    public void HeroUnitUpdated() {
        heroUnit = GridManager.Instance.GetHeroUnit();

        healthText.text = $"{heroUnit.Health}";
        armorText.text = $"{heroUnit.Armor}";
        attackText.text = $"{heroUnit.Attack}";
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
            Destroy(previewUnit.gameObject);
        }

        // set the spawn pos to be slightly closer to the camera above the actual unit
        Vector3 spawnPos = heroUnit.transform.position;
        spawnPos.z -= 1f;
        previewUnit = Instantiate(_baseHeroPrefab, spawnPos, Quaternion.identity).GetComponent<BaseUnit>();

        int previousArmor = heroUnit.Armor;
        int previousHealth = heroUnit.Health;
        int previousAttack = heroUnit.Attack;
        
        previewUnit.Armor = previousArmor;
        previewUnit.Health = previousHealth;
        previewUnit.Attack = previousAttack;
        
        // update the stats based on the card to preview
        UpdateNewStats(card, target, previewUnit);
        
        // animate the change
        previewUnit.AnimateStatChange(heroUnit);

        if (target == ModifyTarget.None)
        {
            Destroy(previewUnit.gameObject);
        }
    }

    public void PlayedCard(CombinedCard card, ModifyTarget target) {
        
        // destroy the last one
        if (previewUnit != null && previewUnit.gameObject != null)
        {
            Destroy(previewUnit.gameObject);
        }
        
        StartCoroutine(UpdateCurrentStats(card, target, heroUnit));
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

        if (newAttack > currentAttack) {
            attackText.color = increaseColor;
        } else if (newAttack < currentAttack) {
            attackText.color = decreaseColor;
        } else {
            attackText.color = noChangeColor;
        }
        attackText.text = $"{newAttack}";

        if (newArmor > currentArmor) {
            armorText.color = increaseColor;
        } else if (newArmor < currentArmor) {
            armorText.color = decreaseColor;
        } else {
            armorText.color = noChangeColor;
        }
        armorText.text = $"{newArmor}";

        unit.Armor = newArmor;
        unit.Attack = newAttack;
    }
    private IEnumerator UpdateCurrentStats(CombinedCard card, ModifyTarget target, BaseUnit unit) {
        UpdateNewStats(card, target, unit);

        yield return new WaitForSeconds(2.5f);

        armorText.color = noChangeColor;
        attackText.color = noChangeColor;

    }
}
