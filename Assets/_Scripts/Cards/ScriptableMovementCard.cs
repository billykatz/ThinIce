using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Movement Card", menuName = "Movement Card")]
public class ScriptableMovementCard : ScriptableCard
{
    public List<Movement> movement;
}

public enum Movement {
    Up = 0,
    Down = 1,
    Left = 2,
    Right = 3,
    UpLeft = 4, 
    UpRight = 5, 
    DownRight = 6, 
    DownLeft = 7


}
