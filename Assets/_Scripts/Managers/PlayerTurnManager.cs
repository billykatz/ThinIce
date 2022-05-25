using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTurnManager : MonoBehaviour
{
    public static PlayerTurnManager Instance;
    private int numberOfCardsPlayedThisTurn;

    private void Awake() {
        Instance = this;
        numberOfCardsPlayedThisTurn = 0;
    }

    public void StartPlayerTurn() {
        numberOfCardsPlayedThisTurn = 0;
    }

    public void PlayCard(CombinedCard playedCard) {
        if (numberOfCardsPlayedThisTurn >= 2) { return; }
        numberOfCardsPlayedThisTurn++;

        // actually play the card
        playedCard.PlayCard();
    }

    public void DidSelectCard(CombinedCard card) {
        // tell the menu manager to do some ish
    }

    public void EndPlayerTurn() {

    }
}
