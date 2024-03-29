using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ScriptableCard : ScriptableObject
{
    public CardType CardType;
    public BaseCard BaseCard;
    public ScriptableCard UpgradedVersionCard;
    public Sprite CardSprite;
}

public enum CardType {
    Movement = 0,
    Modifier = 1
}
