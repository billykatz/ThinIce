using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CardRuleManager : MonoBehaviour
{
    public static CardRuleManager Instance;
    private CardRuleState cardRuleState;

    private CombinedCard currentCard;

    private void Awake() {
        Debug.Log("Card Rule Manager Awake()");
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

    /// <summary>
    /// This function is responsible for completing the movement associated with a card
    /// It can either initiate item or hazard resolution
    /// or
    /// Move to the next movement step of a card
    /// or
    /// complete playing the card and moving us to card rule step Finished
    /// </summary>
    public void DidCompleteMovement(bool checkForSpikes = true) {
        Debug.Log("DidCompleteMovement");

        // After moving we check to see if we should collect something first.
        if (GridManager.Instance.ShouldCollectItem())
        {
            GridManager.Instance.CollectItemAfterCombat();
            return;

        }
        
        if (checkForSpikes && GridManager.Instance.ShouldResolveHazard())
        {
            GridManager.Instance.ResolveHazard();
            return;
        }

        if (GridManager.Instance.CheckForWin())
        {
            GridManager.Instance.TriggerWin();
            return;
        }

        // now we check to see if the card is done
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

    public void DidCompleteCombat() {
        // kill or dont kill the enemy
        if (GridManager.Instance.CheckForDeadEnemy()) {
            // kill it and move the player to that tile
            GridManager.Instance.KillEnemyAndMovePlayer(() =>
            {
                if (TutorialManager.Instance != null)
                {
                    TutorialManager.Instance.EnemyDidDie();
                }
                DidCompleteMovement();
                
                // the player should have updated stats so let the player manager know
                PlayerManager.Instance.HeroUnitUpdated();
            });
        }  else {
            DidCompleteMovement();
            
            // the player should have updated stats so let the player manager know
            PlayerManager.Instance.HeroUnitUpdated();
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
                // move the player
                GridManager.Instance.CollectItem(step.attackerUnit, step.attackerUnit.OccupiedTile, step.collectedItem.OccupiedTile, () =>
                    {
                        // after movement, play the collect animation
                        PlayerManager.Instance.CollectItem(step.collectedItem, () =>
                        {
                            // after moving and collect call complete movement
                            DidCompleteMovement();
                        });
                    });

                break;
            case CardRuleState.Hazard:
                GridManager.Instance.ResolveHazard(step.hazard, step.attackerUnit, step.attackerUnit.OccupiedTile,
                    () =>
                    {
                        // pass in a flag so we dont infitinely check for spikes
                        DidCompleteMovement(false);
                    });
                
                break;
            case CardRuleState.Combat:
                Debug.Log("CardRuleManager: DidStartCombat");
                Debug.Log($"CardRuleManager: attacker {step.attackerUnit}");
                Debug.Log($"CardRuleManager: defender {step.defenderUnit}");
                CombatManager.Instance.ShowCombat(step.attackerUnit, step.defenderUnit, DidCompleteCombat);
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
    public BaseItem collectedItem;
    public BaseHazard hazard;

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