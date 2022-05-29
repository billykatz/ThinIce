using System.Collections;
using System;
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

   private BaseUnit attackingUnit;
   private BaseUnit defendingUnit;

   private CombatUIConfiguration _config;

   public void ShowCombat(BaseUnit attackerUnit, BaseUnit defenderUnit, Action callback) {
        combatUIGameObject.SetActive(true);
        attackingUnit = attackerUnit;
        defendingUnit = defenderUnit;

        CombatUIConfiguration config = new CombatUIConfiguration();
        config.attackerAttackStat = attackerUnit.attack;
        config.attackerName = attackerUnit.UnitName;
        config.attackSprite = attackerUnit.sprite;

        config.defenderHealthStat = defenderUnit.health;
        config.defenderArmorStat = defenderUnit.armor;
        config.defenderName = defenderUnit.UnitName;
        config.defenderSprite = defenderUnit.sprite;

        // hacky for now
        config.defenderHasArmor = attackerUnit.Faction != Faction.Hero;

        // combat math
        config.attackerIsPlayer = attackerUnit.Faction == Faction.Hero;

        config.attackerEndAttackStat = attackerUnit.attack;
        if (attackerUnit.Faction == Faction.Hero) {
            // players lose attack stat when they attack
            config.attackerEndAttackStat = Mathf.Max(0, defenderUnit.armor - defenderUnit.health);
        }

        config.defenderEndArmorStat = Mathf.Max(0, defenderUnit.armor - attackerUnit.attack);

        config.defenderEndHealthStat = Mathf.Max(0, Mathf.Min(defenderUnit.health, (defenderUnit.health + defenderUnit.armor) - attackerUnit.attack));

        config.animationCompleteCallback = callback;

        Debug.Log($"CombatManager: Did Configure with config {config}");
        _config = config;
        combatUIController.Configure(config);
   }

   public async void CombatAnimationComplete() {
        Debug.Log($"CombatManager: Did Complete Combat");

        // wait a quarter of a second
        await Task.Delay(250);

        // and then remove the combat ui
        combatUIGameObject.SetActive(false);

        attackingUnit.attack = _config.attackerEndAttackStat;
        defendingUnit.health = _config.defenderEndHealthStat;
        defendingUnit.armor = _config.defenderEndArmorStat;

        // and then complete the move or wahtever
        if (_config.attackerIsPlayer) {
            // CardRuleManager.Instance.DidCompleteCombat();
        } else {
            PlayerManager.Instance.HeroUnitUpdated();
        }

        _config.animationCompleteCallback.Invoke();

   }
}
