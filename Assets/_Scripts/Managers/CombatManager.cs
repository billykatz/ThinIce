using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class CombatManager : MonoBehaviour
{
   public static CombatManager Instance;

   [SerializeField] private GameAnimator _animator;

   private void Awake() {
       Instance = this;
       Debug.Log("Combat Manager Awake()");
   }

   public void ShowCombat(BaseUnit attackerUnit, BaseUnit defenderUnit, Action callback)
   {


       _animator.AnimateCombat(attackerUnit, defenderUnit, () =>
       {
           int defenderHealthPlusArmor = defenderUnit.Health + defenderUnit.Armor;
            // update defender stats
            bool defenderHasArmor = attackerUnit.Faction != Faction.Hero;
            if (defenderHasArmor)
            {
                defenderUnit.Armor = Mathf.Max(0, defenderUnit.Armor - attackerUnit.Attack);
                defenderUnit.Health = Mathf.Max(0,  defenderUnit.Health + defenderUnit.Armor - attackerUnit.Attack);
            }
            else
            {
                defenderUnit.Health = Mathf.Max(0,  defenderUnit.Health - attackerUnit.Attack);
            }
            
            if (attackerUnit.Faction == Faction.Hero) {
                // players lose Attack stat when they Attack
                attackerUnit.Attack = Mathf.Max(0, attackerUnit.Attack - defenderHealthPlusArmor);
            }
            
            
            PlayerManager.Instance.HeroUnitUpdated();
            
            callback.Invoke();
       });
   }
}
