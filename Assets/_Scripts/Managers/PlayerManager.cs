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

    private void Start() {
        Instance = this;

        healthText.text = $"-";
        armorText.text = $"-";
        attackText.text = $"-";
    }

    public void HeroUnitUpdated() {
        heroUnit = GridManager.Instance.GetHeroUnit();

        healthText.text = $"{heroUnit.health}";
        armorText.text = $"{heroUnit.armor}";
        attackText.text = $"{heroUnit.attack}";
    }

    public void ShowPreview(CombinedCard card, ModifyTarget target) {
        StartCoroutine(FlashThenShowPreview(card, target));
    }

    public void PlayedCard(CombinedCard card, ModifyTarget target) {
        StartCoroutine(UpdateCurrentStats(card, target));
    }

    private IEnumerator UpdateCurrentStats(CombinedCard card, ModifyTarget target) {
        previewPlayerStats.SetActive(false);
        int newAttack = attack;
        int newArmor = armor;

        if (target == ModifyTarget.Attack) {
            switch (card.modifierCard.modifyOperation) {
                case ModifyOperation.Add:
                    newAttack = Mathf.Min(maxAttack, newAttack + card.modifierCard.modifyAmount);
                    break;
                case ModifyOperation.Subtract:
                    newAttack = Mathf.Max(minAttack, newAttack - card.modifierCard.modifyAmount);
                    break;
                case ModifyOperation.Multiply:
                    newAttack = Mathf.Min(maxAttack, newAttack * card.modifierCard.modifyAmount);
                    break;
                case ModifyOperation.Divide:
                    newAttack = Mathf.Max(minAttack, newAttack / card.modifierCard.modifyAmount);
                    break;
                case ModifyOperation.ToMax:
                    newAttack = maxAttack;
                    break;
                case ModifyOperation.ToMin:
                    newAttack = minAttack;
                    break;
            }
        } else if (target == ModifyTarget.Armor) {
            switch (card.modifierCard.modifyOperation) {
                case ModifyOperation.Add:
                    newArmor = Mathf.Min(maxArmor, newArmor + card.modifierCard.modifyAmount);
                    break;
                case ModifyOperation.Subtract:
                    newArmor = Mathf.Max(minArmor, newArmor - card.modifierCard.modifyAmount);
                    break;
                case ModifyOperation.Multiply:
                    newArmor = Mathf.Min(maxArmor, newArmor * card.modifierCard.modifyAmount);
                    break;
                case ModifyOperation.Divide:
                    newArmor = Mathf.Max(minArmor, newArmor / card.modifierCard.modifyAmount);
                    break;
                case ModifyOperation.ToMax:
                    newArmor = maxArmor;
                    break;
                case ModifyOperation.ToMin:
                    newArmor = minArmor;
                    break;
            }
        }
        if (newArmor > armor) {
            armorText.color = increaseColor;
        } else if (armor > newArmor) {
            armorText.color = decreaseColor;
        } else {
            armorText.color = noChangeColor;
        }
        armorText.text = $"{newArmor}";

        if (newAttack > attack) {
            attackText.color = increaseColor;
        } else if (attack > newAttack) {
            attackText.color = decreaseColor;
        } else {
            attackText.color = noChangeColor;
        }
        attackText.text = $"{newAttack}";

        heroUnit.armor = newArmor;
        heroUnit.attack = newAttack;
        yield return new WaitForSeconds(2.5f);

        armorText.color = noChangeColor;
        attackText.color = noChangeColor;

    }

    private IEnumerator FlashThenShowPreview(CombinedCard card, ModifyTarget target) {
        previewPlayerStats.SetActive(false);
        currentPlayerStats.SetActive(false);
        yield return new WaitForSeconds(0.1f);

        previewPlayerStats.SetActive(true);
        currentPlayerStats.SetActive(true);
        Preview(card, target);


    }

    private void Preview(CombinedCard card, ModifyTarget target) {
        previewHealthText.text = healthText.text;
        int newAttack = attack;
        int newArmor = armor;

        if (target == ModifyTarget.Attack) {
            switch (card.modifierCard.modifyOperation) {
                case ModifyOperation.Add:
                    newAttack = Mathf.Min(maxAttack, newAttack + card.modifierCard.modifyAmount);
                    break;
                case ModifyOperation.Subtract:
                    newAttack = Mathf.Max(minAttack, newAttack - card.modifierCard.modifyAmount);
                    break;
                case ModifyOperation.Multiply:
                    newAttack = Mathf.Min(maxAttack, newAttack * card.modifierCard.modifyAmount);
                    break;
                case ModifyOperation.Divide:
                    newAttack = Mathf.Max(minAttack, newAttack / card.modifierCard.modifyAmount);
                    break;
                case ModifyOperation.ToMax:
                    newAttack = maxAttack;
                    break;
                case ModifyOperation.ToMin:
                    newAttack = minAttack;
                    break;
            }
        } else if (target == ModifyTarget.Armor) {
            switch (card.modifierCard.modifyOperation) {
                case ModifyOperation.Add:
                    newArmor = Mathf.Min(maxArmor, newArmor + card.modifierCard.modifyAmount);
                    break;
                case ModifyOperation.Subtract:
                    newArmor = Mathf.Max(minArmor, newArmor - card.modifierCard.modifyAmount);
                    break;
                case ModifyOperation.Multiply:
                    newArmor = Mathf.Min(maxArmor, newArmor * card.modifierCard.modifyAmount);
                    break;
                case ModifyOperation.Divide:
                    newArmor = Mathf.Max(minArmor, newArmor / card.modifierCard.modifyAmount);
                    break;
                case ModifyOperation.ToMax:
                    newArmor = maxArmor;
                    break;
                case ModifyOperation.ToMin:
                    newArmor = minArmor;
                    break;
            }
        }

        if (newArmor > armor) {
            previewArmorText.color = increaseColor;
        } else if (armor > newArmor) {
            previewArmorText.color = decreaseColor;
        } else {
            previewArmorText.color = noChangeColor;
        }
        previewArmorText.text = $"{newArmor}";

        if (newAttack > attack) {
            previewAttackText.color = increaseColor;
        } else if (attack > newAttack) {
            previewAttackText.color = decreaseColor;
        } else {
            previewAttackText.color = noChangeColor;
        }
        previewAttackText.text = $"{newAttack}";
    }
    public void HidePreview() {
        previewPlayerStats.SetActive(false);
    }
}
