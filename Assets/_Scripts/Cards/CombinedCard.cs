using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombinedCard : BaseCard
{
    public MovementCard movementCard;
    public ModifierCard modifierCard;

    public GameObject cardParent;

    public void Create(MovementCard movementCard, ModifierCard modifierCard) {
        this.movementCard = movementCard;
        this.modifierCard = modifierCard;
    }

    public GameObject InstantiateCombindCard(int index) {
        movementCard = Instantiate(movementCard);
        modifierCard = Instantiate(modifierCard);

        cardParent = new GameObject();
        cardParent.name = $"Combined Card {index}";

        movementCard.transform.SetParent(cardParent.transform);
        modifierCard.transform.SetParent(cardParent.transform);
        modifierCard.transform.position = new Vector3(modifierCard.transform.position.x, modifierCard.transform.position.y-1.25f, modifierCard.transform.position.z);

        return cardParent;
    }
}
