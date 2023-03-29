using System.Collections;
using System.Collections.Generic;
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

    [SerializeField] private GameObject previewPlayerStats;
    [SerializeField] private Text previewHealthText;
    [SerializeField] private Text previewArmorText;
    [SerializeField] private Text previewAttackText;


    [SerializeField] private Color increaseColor;
    [SerializeField] private Color decreaseColor;
    [SerializeField] private Color noChangeColor;

    private BaseUnit heroUnit;
    private BaseUnit previewUnit;

    private int armor {
        get {
            return heroUnit.armor;
        }
    }

    private int attack {
        get {
            return heroUnit.attack;
        }
    }

    private void Awake() {
        Debug.Log("PlayerManager Awake()");
        Instance = this;

        healthText.text = $"-";
        armorText.text = $"-";
        attackText.text = $"-";
        previewUnit = new BaseUnit();
    }

    public void HeroUnitUpdated() {
        heroUnit = GridManager.Instance.GetHeroUnit();

        healthText.text = $"{heroUnit.health}";
        armorText.text = $"{heroUnit.armor}";
        attackText.text = $"{heroUnit.attack}";
    }

    public void ShowModifierPreview(CombinedCard card, ModifyTarget target)
    {
        previewUnit.armor = heroUnit.armor;
        previewUnit.health = heroUnit.health;
        previewUnit.attack = heroUnit.attack;
        ShowStats(attack, armor, card, target, previewUnit);
    }

    public void PlayedCard(CombinedCard card, ModifyTarget target) {
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

    private void ShowStats(int currentAttack, int currentArmor, CombinedCard card, ModifyTarget target, BaseUnit unit)
    {
        int newAttack = currentAttack;
        int newArmor = currentArmor;

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

        unit.armor = newArmor;
        unit.attack = newAttack;

    }
    private IEnumerator UpdateCurrentStats(CombinedCard card, ModifyTarget target, BaseUnit unit) {
        previewPlayerStats.SetActive(false);
        ShowStats(attack, armor, card, target, unit);

        yield return new WaitForSeconds(2.5f);

        armorText.color = noChangeColor;
        attackText.color = noChangeColor;

    }
    

    public void HidePreview() {
        previewPlayerStats.SetActive(false);
    }
}
