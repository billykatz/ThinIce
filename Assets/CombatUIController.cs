using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public struct CombatUIConfiguration {
    public Sprite attackSprite;
    public string attackerName;
    public int attackerAttackStat;
    public Sprite defenderSprite;
    public string defenderName;
    public int defenderArmorStat;
    public int defenderHealthStat;

    public bool defenderHasArmor;
}
public class CombatUIController : MonoBehaviour
{
    [SerializeField] GameObject defenderStatsNoArmor;
    [SerializeField] GameObject defenderStatsHasArmor;

    [SerializeField] Image attackerImagePlaceholder;
    [SerializeField] Image defenderImagePlaceholder;

    [SerializeField] Text attackerName;
    [SerializeField] Text defenderName;

    [SerializeField] Text attackStatText;
    [SerializeField] Text armorStatText;
    [SerializeField] Text healthStatTextNoArmor;
    [SerializeField] Text healthStatTextHasArmor;

    public void Configure(CombatUIConfiguration config) {
        attackerImagePlaceholder.sprite = config.attackSprite;
        defenderImagePlaceholder.sprite = config.defenderSprite;

        attackerName.text = config.attackerName;
        defenderName.text = config.defenderName;

        attackStatText.text = $"{config.attackerAttackStat}";
        armorStatText.text = $"{config.defenderArmorStat}";
        healthStatTextNoArmor.text = $"{config.defenderHealthStat}";
        healthStatTextHasArmor.text = $"{config.defenderHealthStat}";

        defenderStatsHasArmor.SetActive(config.defenderHasArmor);
        defenderStatsNoArmor.SetActive(!config.defenderHasArmor);
    }
}
