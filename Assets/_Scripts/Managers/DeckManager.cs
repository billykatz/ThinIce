using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance;

    private List<ScriptableCard> _startingDeck;
    public List<CombinedCard> drawPile;
    public List<CombinedCard> discardPile;

    private void Awake() {
        Instance = this;

        _startingDeck = Resources.LoadAll<ScriptableCard>("Cards").ToList();
    }

    public void CreateDeck() {
        // separate the cards into movement cards and modifier card. Also shuffles the deck
        List<ScriptableCard> movementCards = _startingDeck.Where(t=>t.CardType == CardType.Movement).OrderBy(o=>Random.value).ToList();
        List<ScriptableCard> modifierCards = _startingDeck.Where(t=>t.CardType == CardType.Modifier).OrderBy(o=>Random.value).ToList();

        List<CombinedCard> combinedCards = new List<CombinedCard>();
        for (int i = 0; i < movementCards.Count; i++) {
            MovementCard movementCard = (MovementCard)movementCards[i].BaseCard;
            ModifierCard modifierCard = (ModifierCard)modifierCards[i].BaseCard;

            CombinedCard newCard = new CombinedCard();
            newCard.Create(movementCard, modifierCard);
            combinedCards.Add(newCard);

        }

        drawPile = combinedCards;
        discardPile = new List<CombinedCard>();

        GameManager.Instance.EndGameState(GameState.CreateDeck);

    }
    private void ShuffleDiscardIntoDraw() {
        drawPile = discardPile.OrderBy(o=>Random.value).ToList();
        discardPile = new List<CombinedCard>();
    }

    public CombinedCard DrawCard() {
        Debug.Log("Draw a card");
        if (drawPile.Count == 0) { 
            Debug.Log("First have to shuffle");
            ShuffleDiscardIntoDraw(); 
        }
        Debug.Log($"Ok now can draw a card. Draw pile count: {drawPile.Count}");

        CombinedCard card = drawPile.First();
        drawPile.RemoveAt(0);
        return card;
    }

    public void DiscardCard(CombinedCard card) {
        discardPile.Add(card);
    }
}
