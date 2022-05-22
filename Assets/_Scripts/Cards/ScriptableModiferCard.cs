using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Modifier Card", menuName = "Modifier Card")]
public class ScriptableModiferCard : ScriptableCard
{
    public ModifyOperation ModifyOperation;
    public int ModifyAmount;
}

public enum ModifyOperation {
    Add = 0,
    Subtract = 1,
    Multiply = 2,
    ToMax = 3,
    ToMin = 4,
    Divide = 5

}
