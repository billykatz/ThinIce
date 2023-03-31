using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Movement Item", menuName = "Movement Item")]
public class ScriptableMovementItem : MonoBehaviour
{
    [SerializeField] private GridMovement GridMovement;
    [SerializeField] private int Amount;
}
