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
    private List<ScriptableCard> _discardMovement;
    private List<ScriptableCard> _discardModifier;
    private List<ScriptableCard> _deckMovement;
    private List<ScriptableCard> _deckModifier;

    public bool IsTutorial;
    public bool ShouldCreateStarterDeck;

    private void Awake() {
        Instance = this;
        // IsTutorial = false;
        Debug.Log("Deck Manager Awake()");
        _deckMovement = new List<ScriptableCard>();
        _deckModifier = new List<ScriptableCard>();

        _startingDeck = Resources.LoadAll<ScriptableCard>("Cards").ToList();

        // lets keep this around from the get go
        DontDestroyOnLoad(gameObject);
    }

    public void CreateDeck() {
        if (IsTutorial)
        {
            CreateTutorialDeck();
        } else if (ShouldCreateStarterDeck)
        {
            CreateStarterDeck();
        }
        GameManager.Instance.EndGameState(GameState.CreateDeck);
    }

    public List<ScriptableCard> GetDeck(CardType cardType)
    {
        if (cardType == CardType.Modifier)
        {
            return _deckModifier;
        } else
        {
            return _deckMovement;
        }
    }
    
    private void CreateStarterDeck() {
        // separate the cards into movement cards and modifier card. Also shuffles the deck
        //_deckMovement = _startingDeck.Where(t=>t.CardType == CardType.Movement).OrderBy(o=>o.name).ToList();
        //_deckModifier = _startingDeck.Where(t=>t.CardType == CardType.Modifier).OrderBy(o=>o.name).ToList();
        

        _deckMovement = ShuffleDiscardIntoDeck(_startingDeck.Where(t=>t.CardType == CardType.Movement).ToList());
        _deckModifier = ShuffleDiscardIntoDeck(_startingDeck.Where(t=>t.CardType == CardType.Modifier).ToList());
    }
    
    private void CreateTutorialDeck() {
        // separate the cards into movement cards and modifier card. Also shuffles the deck
        List<ScriptableCard> tutorialCards = Resources.LoadAll<ScriptableCard>("TutorialCards").ToList();
        _deckMovement = ShuffleDiscardIntoDeck(tutorialCards.Where(t=>t.CardType == CardType.Movement).ToList());
        _deckModifier = ShuffleDiscardIntoDeck(tutorialCards.Where(t=>t.CardType == CardType.Modifier).ToList());
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

        // remove them from the deck
        _deckMovement.RemoveAt(0);
        _deckModifier.RemoveAt(0);

        return drawnCard;
    }

    public CombinedCard CreateTopCard()
    {
        Debug.Log("Create the top");
        
        // make sure neither deck is empty
        if (_deckMovement.Count == 0)
        {
            Debug.Log("Shuffling Movement");
            _deckMovement = ShuffleDiscardIntoDeck(_discardMovement);
        }

        if (_deckModifier.Count == 0)
        {
            Debug.Log("Shuffling MODIFIER");
            _deckModifier = ShuffleDiscardIntoDeck(_discardModifier);
        }

        // create the movement and modifier cards
        MovementCard movementCard = (MovementCard)_deckMovement[0].BaseCard;
        movementCard.ScriptableCard = _deckMovement[0];
        ModifierCard modifierCard = (ModifierCard)_deckModifier[0].BaseCard;
        modifierCard.ScriptableCard = _deckModifier[0];
        
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
        _discardMovement.Add(card.movementCard.ScriptableCard);
        _discardModifier.Add(card.modifierCard.ScriptableCard);
    }

    public void AddCardToDeck(ScriptableCard card, CardType cardType)
    {
        if (cardType == CardType.Modifier)
        {
            _discardModifier.Add(card);
        } else if (cardType == CardType.Movement)
        {
            _discardMovement.Add(card);
        }
    }
    
    public void RemoveCardFromDeck(ScriptableCard card)
    {
        if (card.CardType == CardType.Modifier)
        {
            _deckModifier.Remove(card);
        } else if (card.CardType == CardType.Movement)
        {
            _deckMovement.Remove(card);
        }
    }

    public void UpgradeCard(ScriptableCard card, CardType cardType)
    {
        if (cardType == CardType.Modifier)
        {
            _deckModifier.Remove(card);
            _deckModifier.Add(card.UpgradedVersionCard);
        } else if (cardType == CardType.Movement)
        {
            _deckMovement.Remove(card);
            _deckMovement.Add(card.UpgradedVersionCard);
        }
    }
}
