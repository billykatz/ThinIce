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
       Debug.Log("Combat Manager Awake()");
   }

   private BaseUnit attackingUnit;
   private BaseUnit defendingUnit;

   private CombatUIConfiguration _config;

   public static event Action<BaseUnit> OnUnitDidDie;

   public void ShowCombat(BaseUnit attackerUnit, BaseUnit defenderUnit, Action callback) {
        combatUIGameObject.SetActive(true);
        attackingUnit = attackerUnit;
        defendingUnit = defenderUnit;

        CombatUIConfiguration config = new CombatUIConfiguration();
        config.attackerAttackStat = attackerUnit.Attack;
        config.attackerName = attackerUnit.UnitName;
        config.attackSprite = attackerUnit.sprite;

        config.defenderHealthStat = defenderUnit.Health;
        config.defenderArmorStat = defenderUnit.Armor;
        config.defenderName = defenderUnit.UnitName;
        config.defenderSprite = defenderUnit.sprite;

        // hacky for now
        config.defenderHasArmor = attackerUnit.Faction != Faction.Hero;

        // combat math
        config.attackerIsPlayer = attackerUnit.Faction == Faction.Hero;

        config.attackerEndAttackStat = attackerUnit.Attack;
        if (attackerUnit.Faction == Faction.Hero) {
            // players lose Attack stat when they Attack
            config.attackerEndAttackStat = Mathf.Max(0, attackerUnit.Attack - (defenderUnit.Armor + defenderUnit.Health));
        }

        config.defenderEndArmorStat = Mathf.Max(0, defenderUnit.Armor - attackerUnit.Attack);

        config.defenderEndHealthStat = Mathf.Max(0, Mathf.Min(defenderUnit.Health, (defenderUnit.Health + defenderUnit.Armor) - attackerUnit.Attack));

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

        attackingUnit.Attack = _config.attackerEndAttackStat;
        defendingUnit.Health = _config.defenderEndHealthStat;
        defendingUnit.Armor = _config.defenderEndArmorStat;

        // and then complete the move or wahtever
        if (_config.attackerIsPlayer) {
            // CardRuleManager.Instance.DidCompleteCombat();
        } else {
            PlayerManager.Instance.HeroUnitUpdated();
        }

        if (OnUnitDidDie != null) {
            if (defendingUnit.Health <= 0) {
                Debug.Log($"{defendingUnit.UnitName} did die");
                OnUnitDidDie.Invoke(defendingUnit);
            }
        }

        _config.animationCompleteCallback.Invoke();

   }
}
