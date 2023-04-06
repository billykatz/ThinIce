using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance;

    [SerializeField] private GameObject CombinedCardPrefab;
    [SerializeField] private ProgressController _progressController;
    private List<ScriptableCard> _startingDeck;
    public List<ScriptableCard> _discardMovement = new List<ScriptableCard>();
    public List<ScriptableCard> _discardModifier = new List<ScriptableCard>();
    public List<ScriptableCard> _deckMovement = new List<ScriptableCard>();
    public List<ScriptableCard> _deckModifier = new List<ScriptableCard>();
    
    private List<ScriptableCard> _tutorialDeckMovement = new List<ScriptableCard>();
    private List<ScriptableCard> _tutorialDeckModifier = new List<ScriptableCard>();
    private List<ScriptableCard> _tutorialDiscardMovement = new List<ScriptableCard>();
    private List<ScriptableCard> _tutorialDiscardModifier = new List<ScriptableCard>();

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

    private void Start()
    {
        if (_progressController.JustForTesting)
        {
            CreateStarterDeck();
        }
    }

    public void CreateDeck() {
        if (IsTutorial)
        {
            CreateTutorialDeck();
        } else if (ShouldCreateStarterDeck)
        {
            CreateStarterDeck();
        }
        ShuffleEverything();
        GameManager.Instance.EndGameState(GameState.CreateDeck);
    }

    public List<ScriptableCard> GetDeck(CardType cardType)
    {
        if (cardType == CardType.Modifier)
        {
            if (IsTutorial)
            {
                return _tutorialDeckModifier;
            } else
            {
                return _deckModifier;
            }
        } else
        {
            if (IsTutorial)
            {
                return _tutorialDeckMovement;
            }
            else
            {
                return _deckMovement;
            }
        }
    }
    
    public List<ScriptableCard> GetDiscard(CardType cardType)
    {
        if (cardType == CardType.Modifier)
        {
            if (IsTutorial)
            {
                return _tutorialDiscardModifier;
            } else
            {
                return _discardModifier;
            }
        } else
        {
            if (IsTutorial)
            {
                return _tutorialDiscardMovement;
            }
            else
            {
                return _discardMovement;
            }
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
        _tutorialDeckMovement = ShuffleDiscardIntoDeck(tutorialCards.Where(t=>t.CardType == CardType.Movement).ToList());
        _tutorialDeckModifier = ShuffleDiscardIntoDeck(tutorialCards.Where(t=>t.CardType == CardType.Modifier).ToList());
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
    
    private List<ScriptableCard> ShuffleDeckAndDiscard(List<ScriptableCard> deck, List<ScriptableCard> discardedCards)
    {
        List<ScriptableCard> shuffledDeck = deck;
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
        GetDeck(CardType.Movement).RemoveAt(0);
        GetDeck(CardType.Modifier).RemoveAt(0);

        return drawnCard;
    }

    public void ShuffleEverything()
    {
        _deckMovement = ShuffleDeckAndDiscard(_deckMovement, GetDiscard(CardType.Movement));
        _discardMovement = new List<ScriptableCard>();
        _deckModifier = ShuffleDeckAndDiscard(_deckModifier, GetDiscard(CardType.Modifier));
        _discardModifier = new List<ScriptableCard>();

    }

    public CombinedCard CreateTopCard()
    {
        Debug.Log("Create the top");
        
        // make sure neither deck is empty
        if (GetDeck(CardType.Movement).Count == 0)
        {
            Debug.Log("Shuffling Movement");
            if (IsTutorial)
            {
                _tutorialDeckMovement = ShuffleDiscardIntoDeck(GetDiscard(CardType.Movement));
                _tutorialDiscardMovement = new List<ScriptableCard>();
            }
            else
            {
                _deckMovement = ShuffleDiscardIntoDeck(GetDiscard(CardType.Movement));
                _discardMovement = new List<ScriptableCard>();
            }
        }

        if (GetDeck(CardType.Modifier).Count == 0)
        {
            Debug.Log("Shuffling MODIFIER");
            if (IsTutorial)
            {
                _tutorialDeckModifier = ShuffleDiscardIntoDeck(GetDiscard(CardType.Modifier));
                _tutorialDiscardModifier = new List<ScriptableCard>();
            }
            else
            {
                _deckModifier = ShuffleDiscardIntoDeck(GetDiscard(CardType.Modifier));
                _discardModifier = new List<ScriptableCard>();
            }
        }

        // create the movement and modifier cards
        MovementCard movementCard = (MovementCard)GetDeck(CardType.Movement)[0].BaseCard;
        movementCard.ScriptableCard = GetDeck(CardType.Movement)[0];
        ModifierCard modifierCard = (ModifierCard)GetDeck(CardType.Modifier)[0].BaseCard;
        modifierCard.ScriptableCard = GetDeck(CardType.Modifier)[0];
        
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
        GetDiscard(CardType.Movement).Add(card.movementCard.ScriptableCard);
        GetDiscard(CardType.Modifier).Add(card.modifierCard.ScriptableCard);
    }

    public void AddCardToDeck(ScriptableCard card, CardType cardType)
    {
        if (cardType == CardType.Modifier)
        {
            GetDiscard(CardType.Modifier).Add(card);
        } else if (cardType == CardType.Movement)
        {
            GetDiscard(CardType.Movement).Add(card);
        }
    }
    
    public void RemoveCardFromDeck(ScriptableCard card)
    {
        if (card.CardType == CardType.Modifier)
        {
            GetDeck(CardType.Modifier).Remove(card);
        } else if (card.CardType == CardType.Movement)
        {
            GetDeck(CardType.Movement).Remove(card);
        }
    }

    public void UpgradeCard(ScriptableCard card, CardType cardType)
    {
        if (cardType == CardType.Modifier)
        {
            GetDeck(CardType.Modifier).Remove(card);
            GetDeck(CardType.Modifier).Add(card.UpgradedVersionCard);
        } else if (cardType == CardType.Movement)
        {
            GetDeck(CardType.Movement).Remove(card);
            GetDeck(CardType.Movement).Add(card.UpgradedVersionCard);
        }
    }
}
