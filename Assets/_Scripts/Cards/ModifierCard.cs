using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModifierCard : BaseCard
{
    [SerializeField] ModifyOperation modifyOperation;
    [SerializeField] int modifyAmount;
}
