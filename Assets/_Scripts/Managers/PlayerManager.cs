using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ModifyTarget {
    Armor = 0,
    Attack = 1
}

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;

    public int hp;
    public int maxHp;
    public int minHp;
    public int armor;
    public int attack;
    public int maxArmor;
    public int minArmor;
    public int maxAttack;
    public int minAttack;

    [SerializeField] private Text healthText;
    [SerializeField] private Text armorText;
    [SerializeField] private Text attackText;

    [SerializeField] private GameObject previewPlayerStats;
    [SerializeField] private Text previewHealthText;
    [SerializeField] private Text previewArmorText;
    [SerializeField] private Text previewAttackText;

    private void Awake() {
        Instance = this;

        healthText.text = $"{hp}";
        armorText.text = $"{armor}";
        attackText.text = $"{attack}";
    }

    public void ShowPreview(CombinedCard card, ModifyTarget target) {
        previewPlayerStats.SetActive(true);
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

        previewArmorText.text = $"{newArmor}";
        previewAttackText.text = $"{newAttack}";

    }

    public void HidePreview() {
        previewPlayerStats.SetActive(false);
    }
}
