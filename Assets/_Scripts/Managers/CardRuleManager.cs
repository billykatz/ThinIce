using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardRuleManager : MonoBehaviour
{
    public static CardRuleManager Instance;
    private CardRuleState cardRuleState;

    private CombinedCard currentCard;

    private void Awake() {
        Instance = this;
        cardRuleState = CardRuleState.Start;
    }

    public void PlayCard(CombinedCard card, ModifyTarget target) {
        Debug.Log($"CardRuleManager: Playing a card");
        // actually play the card
        // playedCard.PlayCard();
        currentCard = card;

        // tell the grid manager to show some movement arrows
        CardRuleStep step = new CardRuleStep();
        step.state = CardRuleState.ChooseMovement;
        step.card = card;
        step.movement = card.movementCard.movement;
        StartCardRuleStep(step);
    }

    public void DidCompleteMovement() {
        currentCard.movementCard.movementIndex++;

        if (currentCard.movementCard.movementIndex >= currentCard.movementCard.movement.Count) {
            // no more moves
            StartCardRuleStep(CardRuleStep.Init(CardRuleState.Finish));
        } else {
            CardRuleStep step = new CardRuleStep();
            step.state = CardRuleState.ChooseMovement;
            step.card = currentCard;
            step.movement = currentCard.movementCard.movement;
            StartCardRuleStep(step);
        }
    }

    public void StartCardRuleStep(CardRuleStep step) {
        switch (step.state) {
            case CardRuleState.Start:
                break;
            case CardRuleState.ChooseMovement:
                Debug.Log($"CardRuleManager: Did Start Choose movement {step.card.movementCard}");
                Debug.Log($"CardRuleManager: Did Start Choose movement {step.card.movementCard.movementIndex}");
                GridManager.Instance.ShowMovementHelper(step.card, step.card.movementCard.movementIndex);

                break;
            case CardRuleState.Collect:
                break;
            case CardRuleState.Combat:
                Debug.Log("CardRuleManager: DidStartCombat");
                Debug.Log($"CardRuleManager: attacker {step.attackerUnit}");
                Debug.Log($"CardRuleManager: defender {step.defenderUnit}");
                CombatManager.Instance.ShowCombat(step.attackerUnit, step.defenderUnit);
                break;
            case CardRuleState.Hazard:
                break;
            case CardRuleState.Finish:
                FinishCard();
                break;
        }
    }

    public void FinishCard() {
        HandManager.Instance.DidFinishPlayingCard(currentCard);
        currentCard = null;
    }
}

public struct CardRuleStep {
    public CombinedCard card;
    public CardRuleState state;
    public List<Movement> movement;
    public BaseUnit attackerUnit;
    public BaseUnit defenderUnit;

    public static CardRuleStep Init(CardRuleState state) {
        CardRuleStep step = new CardRuleStep();
        step.state = state;
        return step;
    }
}

public enum CardRuleState {
    Start,
    ChooseMovement,
    Combat,
    Collect,
    Hazard,
    Finish
    

}