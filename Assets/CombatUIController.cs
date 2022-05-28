using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using DG.Tweening;

public struct CombatUIConfiguration {
    public Sprite attackSprite;
    public string attackerName;
    public int attackerAttackStat;
    public Sprite defenderSprite;
    public string defenderName;
    public int defenderArmorStat;
    public int defenderHealthStat;
    public bool defenderHasArmor;
    public bool attackerIsPlayer;

    public int attackerEndAttackStat;
    public int defenderEndHealthStat;
    public int defenderEndArmorStat;
}
public class CombatUIController : MonoBehaviour
{
    [SerializeField] GameObject defenderStatsNoArmor;
    [SerializeField] GameObject defenderStatsHasArmor;

    [SerializeField] GameObject attackerStatParent;

    [SerializeField] Image attackerImagePlaceholder;
    [SerializeField] Image defenderImagePlaceholder;

    [SerializeField] Text attackerName;
    [SerializeField] Text defenderName;

    [SerializeField] Text attackStatText;
    [SerializeField] Text armorStatText;
    [SerializeField] Text healthStatTextNoArmor;
    [SerializeField] Text healthStatTextHasArmor;


    public static event Action animationComplete;
    private CombatUIConfiguration _config;
    public void Configure(CombatUIConfiguration config) {
        _config = config;

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

        AnimateCombat();
    }

    public void AnimateCombat() {

        Vector3 originalAttackerPosition = attackerImagePlaceholder.transform.position;
        Vector3 originalAttackStatPosition = attackerStatParent.transform.position;

        Sequence attackSequence = DOTween.Sequence();
        attackSequence
            .Append(attackerImagePlaceholder.transform.DOMove(defenderImagePlaceholder.transform.position, 0.5f))
            .AppendInterval(0.05f)
            .Append(attackerImagePlaceholder.transform.DOMove(originalAttackerPosition, 0.5f))
            .PrependInterval(1.5f)
            .SetEase(Ease.InOutSine);

        Sequence statSequence = DOTween.Sequence();
        statSequence
            .Append(attackerStatParent.transform.DOMove(healthStatTextNoArmor.transform.position, 0.5f))
            .AppendInterval(0.05f)
            .Append(attackerStatParent.transform.DOMove(originalAttackStatPosition, 0.5f))
            .PrependInterval(1.5f)
            .SetEase(Ease.InOutSine);

        attackSequence.Play();
        statSequence.Play();

        attackSequence.OnComplete(() => AttackAnimationComplete());
    }

    public void AttackAnimationComplete() {
        Debug.Log("CombatUIController: Attack animation finished");

        animationCount = 0;
        animationComplete += EndAnimation;

        // decrement attack if it is a player
        if (_config.attackerIsPlayer) {
            int endValue = _config.attackerEndAttackStat;
            StartCoroutine(DecrementStat(attackStatText, _config.attackerAttackStat, endValue, animationComplete));
            animationCount++;
        }

        // decrement armor
        if (_config.defenderHasArmor) {
            int endValue = _config.defenderEndArmorStat;
            StartCoroutine(DecrementStat(armorStatText, _config.attackerAttackStat, endValue, animationComplete));
            animationCount++;
        }

        // decrement health after armor is depleted
        int healthEndValue = _config.defenderEndHealthStat;
        if (_config.defenderHasArmor) {
            StartCoroutine(DecrementStat(healthStatTextHasArmor, _config.defenderHealthStat, healthEndValue, animationComplete));
            animationCount++;
        } else {
            StartCoroutine(DecrementStat(healthStatTextNoArmor, _config.defenderHealthStat, healthEndValue, animationComplete));
            animationCount++;
        }
    }
    private void OnDisable() {
        animationComplete -= EndAnimation;
    }

    private int animationCount = 0;
    private void EndAnimation() {
        animationCount--;
        if (animationCount <= 0) {
            CombatManager.Instance.CombatAnimationComplete();
            animationCount = 10000;
        } 
    }

    private IEnumerator DecrementStat(Text stat, int startValue, int endValue, Action callback) {
        int difference = startValue - endValue;

        if (difference <= 0) {
            yield return new WaitForSeconds(0);
        }

        for (int i = 0; i < difference; i++) {
            yield return new WaitForSeconds(0.33f);
            stat.text = $"{startValue - (i+1)}";
        }

        callback.Invoke();
    }
}
