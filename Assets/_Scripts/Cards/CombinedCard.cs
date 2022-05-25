using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombinedCard : BaseCard, MouseInteractionDelegate
{
    public MovementCard movementCard;
    public ModifierCard modifierCard;

    public GameObject cardParent;

    private int index;

    public void Create(MovementCard movementCard, ModifierCard modifierCard) {
        this.movementCard = movementCard;
        this.modifierCard = modifierCard;
    }

    public GameObject InstantiateCombindCard(int index) {
        movementCard = Instantiate(movementCard);
        modifierCard = Instantiate(modifierCard);

        movementCard.SetInteractionDelegate(this);
        modifierCard.SetInteractionDelegate(this);

        cardParent = new GameObject();
        cardParent.name = $"Combined Card {index}";
        this.index = index;

        movementCard.transform.SetParent(cardParent.transform);
        modifierCard.transform.SetParent(cardParent.transform);
        modifierCard.transform.position = new Vector3(modifierCard.transform.position.x, modifierCard.transform.position.y-1.25f, modifierCard.transform.position.z);

        return cardParent;
    }

    void MouseInteractionDelegate.OnMouseDown() {
        HandManager.Instance.DidSelectCard(index);
    }

    void MouseInteractionDelegate.OnMouseEnter() {
        HandManager.Instance.DidHoverOverCard(index);
    }

    void MouseInteractionDelegate.OnMouseExit() {
        HandManager.Instance.DidStopHoverOverCard(index);
    }

    public void SetSelectedBackground(bool onOff) {
        if (onOff) {
            movementCard.SetFullCardHighlight(true);
        } else {
            movementCard.SetFullCardHighlight(false);
        }
    }
}
