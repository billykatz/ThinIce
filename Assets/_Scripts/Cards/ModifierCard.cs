using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModifierCard : BaseCard
{
    [SerializeField] public ModifyOperation modifyOperation;
    [SerializeField] public int modifyAmount;
    [SerializeField] public bool AffectsHealth;
}
