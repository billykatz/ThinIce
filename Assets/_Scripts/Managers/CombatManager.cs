using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
   public static CombatManager Instance;

   [SerializeField] private Canvas canvas;
   [SerializeField] private GameObject combatUIGameObject;
   [SerializeField] private CombatUIController combatUIController;

   private void Awake() {
       Instance = this;
   }

   public void ShowCombat(BaseUnit attackerUnit, BaseUnit defenderUnit) {
        combatUIGameObject.SetActive(true);

        CombatUIConfiguration config = new CombatUIConfiguration();
        config.attackerAttackStat = attackerUnit.attack;
        config.attackerName = attackerUnit.UnitName;
        config.attackSprite = attackerUnit.sprite;

        config.defenderHealthStat = defenderUnit.health;
        config.defenderArmorStat = defenderUnit.armor;
        config.defenderName = defenderUnit.UnitName;
        config.defenderSprite = defenderUnit.sprite;

        Debug.Log($"CombatManager: Did Configure with config {config}");
        combatUIController.Configure(config);
   }
}
