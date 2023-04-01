using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance;

    [SerializeField] private GameObject CombinedCardPrefab;
    private List<ScriptableCard> _startingDeck;
    public List<CombinedCard> drawPile;
    private List<CombinedCard> discardPile;
    
    public List<ScriptableCard> discardMovement;
    public List<ScriptableCard> discardModifier;
    public List<ScriptableCard> deckMovement;
    public List<ScriptableCard> deckModifier;

    private void Awake() {
        Instance = this;
        Debug.Log("Deck Manager Awake()");
        deckMovement = new List<ScriptableCard>();
        deckModifier = new List<ScriptableCard>();

        _startingDeck = Resources.LoadAll<ScriptableCard>("Cards").ToList();
    }

    public void CreateDeck() {
        CreateStarterDeck();

        GameManager.Instance.EndGameState(GameState.CreateDeck);

    }
    private void CreateStarterDeck() {
        // separate the cards into movement cards and modifier card. Also shuffles the deck
        deckMovement = ShuffleDiscardIntoDeck(_startingDeck.Where(t=>t.CardType == CardType.Movement).OrderBy(o=>Random.value).ToList());
        deckModifier = ShuffleDiscardIntoDeck(_startingDeck.Where(t=>t.CardType == CardType.Modifier).OrderBy(o=>Random.value).ToList());
    }

    private List<ScriptableCard> ShuffleDiscardIntoDeck(List<ScriptableCard> discardedCards)
    {
        List<ScriptableCard> shuffledDeck = new List<ScriptableCard>();
        for (int i = 0; i < discardedCards.Count; i++)
        {
            shuffledDeck.Add(discardedCards[i]);
        }

        return shuffledDeck.OrderBy((o) => Random.value).ToList();
    }

    public CombinedCard DrawCard() {
        Debug.Log("Draw a card");
        
        CombinedCard drawnCard = CreateTopCard();

        return drawnCard;
    }

    public CombinedCard CreateTopCard()
    {
        Debug.Log("Create the top");
        
        // make sure neither deck is empty
        if (deckMovement.Count == 0)
        {
            Debug.Log("Shuffling Movement");
            deckMovement = ShuffleDiscardIntoDeck(discardMovement);
        }

        if (deckModifier.Count == 0)
        {
            Debug.Log("Shuffling MODIFIER");
            deckModifier = ShuffleDiscardIntoDeck(discardModifier);
        }

        // create the movement and modifier cards
        MovementCard movementCard = (MovementCard)deckMovement[0].BaseCard;
        movementCard.ScriptableCard = deckMovement[0];
        ModifierCard modifierCard = (ModifierCard)deckModifier[0].BaseCard;
        modifierCard.ScriptableCard = deckModifier[0];
        
        // remove them from the deck
        deckMovement.RemoveAt(0);
        deckModifier.RemoveAt(0);
        
        // create the combined card
        GameObject newCard = Instantiate(CombinedCardPrefab);
        CombinedCard card = newCard.GetComponent<CombinedCard>();
        card.Create(movementCard, modifierCard, newCard);
        card.cardParent.SetActive(true);
        
        return card;
    }

    public void DiscardCard(CombinedCard card) 
    {
        RecycleCard(card);
    }
    public void DidPlayCard(CombinedCard card) {
        RecycleCard(card);
    }

    private void RecycleCard(CombinedCard card)
    {
        card.DestroyCard();
        discardMovement.Add(card.movementCard.ScriptableCard);
        discardModifier.Add(card.modifierCard.ScriptableCard);
    }
}
