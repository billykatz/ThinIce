using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

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

        // combat math
        config.attackerIsPlayer = attackerUnit.Faction == Faction.Hero;

        config.attackerEndAttackStat = attackerUnit.attack;
        if (attackerUnit.Faction == Faction.Hero) {
            config.attackerEndAttackStat = Mathf.Max(0, defenderUnit.armor - defenderUnit.health);
        }

        config.defenderEndArmorStat = Mathf.Max(0, defenderUnit.armor - attackerUnit.attack);

        config.defenderEndHealthStat = Mathf.Max(0, (defenderUnit.health + defenderUnit.armor) - attackerUnit.attack);


        Debug.Log($"CombatManager: Did Configure with config {config}");
        combatUIController.Configure(config);
   }

   public async void CombatAnimationComplete() {
        Debug.Log($"CombatManager: Did Complete Combat");

        // wait a quarter of a second
        await Task.Delay(250);

        
   }
}
