using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class CombatManager : MonoBehaviour
{
   public static CombatManager Instance;

   [SerializeField] private GameAnimator _animator;
   [SerializeField] private GameObject _damageBadgePrefab;

   private void Awake() {
       Instance = this;
       Debug.Log("Combat Manager Awake()");
   }

   public void ShowCombat(BaseUnit attackerUnit, BaseUnit defenderUnit, Action callback)
   {

       int attackDamage = attackerUnit.Attack;

       
       Debug.Log($"{attackerUnit.name} showing combat");
       attackerUnit.ToggleAttackIndicators(false);
       _animator.AnimateCombat(attackerUnit, defenderUnit,
           () =>
           {
               Debug.Log($"{attackerUnit.name} attack did hit");
               AnimationData data = new AnimationData();
               data.StartPosition = defenderUnit.transform.position;
               data.EndPosition = defenderUnit.transform.position; 
               GameObject damageBadge = Instantiate(_damageBadgePrefab, defenderUnit.transform.position, Quaternion.identity); 
               FXView view = damageBadge.GetComponent<FXView>();
               view.SetUp(damageBadge.transform);
               view.SetText("-" + attackDamage);
               _animator.Animate(view, data);
           },
           () =>
           {
               Debug.Log($"{attackerUnit.name} attack animation did finish");
               int defenderHealthPlusArmor = defenderUnit.Health + defenderUnit.Armor;
               // update defender stats
               bool defenderHasArmor = attackerUnit.Faction != Faction.Hero;
               if (defenderHasArmor)
               {
                   int extraDamage = defenderUnit.Armor - attackerUnit.Attack;
                   defenderUnit.Armor = Mathf.Max(0, defenderUnit.Armor - attackerUnit.Attack);
                   if (extraDamage < 0)
                   {
                       defenderUnit.Health = Mathf.Max(0, defenderUnit.Health + extraDamage);
                   }
               }
               else
               {
                   defenderUnit.Health = Mathf.Max(0, defenderUnit.Health - attackerUnit.Attack);
               }

               if (attackerUnit.Faction == Faction.Hero)
               {
                   // players lose Attack stat when they Attack
                   attackerUnit.Attack = Mathf.Max(0, attackerUnit.Attack - defenderHealthPlusArmor);
               }


               attackerUnit.ToggleAttackIndicators(true);
               PlayerManager.Instance.HeroUnitUpdated();

               callback.Invoke();
           });
   }
}
