using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Game Settings", menuName = "Game Settings")]
public class GameSettings : ScriptableObject
{
    public int Width, VisibleRows;
}
