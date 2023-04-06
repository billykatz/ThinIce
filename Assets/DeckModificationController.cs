using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

[Serializable]
public class DeckImprovementModel
{
    public CardType CardType;
    public ShopType ShopType;
    public string ShopInstructionTitle;
    public string ShopInstructionText;
    public int NumberCardsToAdd;
    public int NumberCardsToRemove;
    public int NumberCardsToUpgrade;
    public string StatType;
    public string StatAmount;
    public ScriptableCard[] CardChoicesToAdd;

}

public class DeckModificationController : MonoBehaviour
{

    [SerializeField] private WorldMapController _worldMapController;
    // this is the parent object that we will add items to
    [SerializeField] private GameObject _gridParent;
    [SerializeField] private GameObject _cardInModalView;
    [SerializeField] private TMP_Text _titleTextView;
    [SerializeField] private TMP_Text _instructionsTextView;
    [SerializeField] private GameObject _deckManagementView;
    [SerializeField] private DeckManager _deckManager;
    [SerializeField] private CanvasGroup _deckManagmentCanvasGroup;

    [SerializeField] private GraphicRaycaster _raycaster;
    [SerializeField] private EventSystem _eventSystem;

    [SerializeField] private ProgressController _progressManager;

    private ScriptableLevelRules _levelRules;
    private ShopType _shopType;
    private ScriptableCard[] _cardsToAdd;
    private List<ScriptableCard> _deckWeAreEditing;
    private int _numCardsToRemove;
    private List<int> _cardIndexesRemoved;
    private int _numCardsToUpgrade;
    private List<int> _cardIndexesUpgraded;
    private int _numCardsToAdd;
    private List<int> _cardIndexesAdded;

    private List<CardInDeckView> _cardViews;
    private List<GameObject> _cardGameObjects;

    private DeckImprovementModel[] _deckImprovements;
    private int _deckImprovementIndex;

    private void Awake()
    {
        _cardIndexesRemoved = new List<int>();
        _cardIndexesUpgraded = new List<int>();
        _cardIndexesAdded = new List<int>();
        _cardViews = new List<CardInDeckView>();
        _cardGameObjects = new List<GameObject>();
        _deckWeAreEditing = new List<ScriptableCard>();

        _deckManager = FindObjectOfType<DeckManager>();
    }

    public void Configure(ScriptableLevelRules levelRules)
    {
        _levelRules = levelRules;
        _deckImprovements = _levelRules.DeckImprovementModels;
        _deckImprovementIndex = 0;
        ShowShop(_deckImprovements[_deckImprovementIndex]);
    }

    public void ShowShop(DeckImprovementModel model)
    {
        _deckManagmentCanvasGroup.DOFade(1, 0.25f);
        
        _deckWeAreEditing = new List<ScriptableCard>();

        _titleTextView.text = model.ShopInstructionTitle;
        _instructionsTextView.text = model.ShopInstructionText;
        _shopType = model.ShopType;

        if (_shopType == ShopType.Add)
        {
            ConfigureAdd(model);
        } else if (_shopType == ShopType.Remove)
        {
            ConfigureRemove(model);
            
        } else if (_shopType == ShopType.Upgrade)
        {
            ConfigureUpgrade(model);
        }
    }

    public void ConfigureGrid(List<ScriptableCard> scriptableCards)
    {
        _cardGameObjects = new List<GameObject>();
        _cardViews = new List<CardInDeckView>();
        for (int i = 0; i < scriptableCards.Count; i++)
        {
            GameObject newCard = Instantiate(_cardInModalView, _gridParent.transform);
            CardInDeckView deckViewCard = newCard.GetComponent<CardInDeckView>();
            deckViewCard.Configure(scriptableCards[i], i, _shopType, DidSelectItem, DidDeselectItem, _raycaster, _eventSystem);
            _cardGameObjects.Add(newCard);
            _cardViews.Add(deckViewCard);
        }
    }

    public void DidDeselectItem(int index)
    {
        if (_shopType == ShopType.Remove)
        {
            _cardIndexesRemoved.Remove(index);
        } else if (_shopType == ShopType.Upgrade)
        {
            _cardIndexesUpgraded.Remove(index);
        } else if (_shopType == ShopType.Add)
        {
            _cardIndexesAdded.Remove(index);
        }
    }

    public void DidSelectItem(int index)
    {
        if (_shopType == ShopType.Remove)
        {
            _cardIndexesRemoved.Add(index);
            if (_cardIndexesRemoved.Count > _numCardsToRemove)
            {
                int lastIndexed = _cardIndexesRemoved[0];
                _cardIndexesRemoved.RemoveAt(0);
                _cardViews[lastIndexed].DidSelectRemove();
            }
        } else if (_shopType == ShopType.Upgrade)
        {
            _cardIndexesUpgraded.Add(index);
            if (_cardIndexesUpgraded.Count > _numCardsToUpgrade)
            {
                int lastIndex = _cardIndexesUpgraded[0];
                _cardIndexesUpgraded.RemoveAt(0);
                _cardViews[lastIndex].DidSelectUpgrade();
            }
        } else if (_shopType == ShopType.Add)
        {
            _cardIndexesAdded.Add(index);
            if (_cardIndexesAdded.Count > _numCardsToAdd)
            {
                int lastIndex = _cardIndexesAdded[0];
                _cardIndexesAdded.RemoveAt(0);
                _cardViews[lastIndex].DidSelectAdd();
            }
        }
    }
    
    public void ConfigureUpgrade(DeckImprovementModel model)
    {
        _numCardsToUpgrade = model.NumberCardsToUpgrade;
        _deckWeAreEditing = DeckManager.Instance.GetDeck(model.CardType);
        ConfigureGrid(_deckWeAreEditing);

    }

    public void ConfigureRemove(DeckImprovementModel model)
    {
        _numCardsToRemove = model.NumberCardsToRemove;
        _deckWeAreEditing = DeckManager.Instance.GetDeck(model.CardType);
        ConfigureGrid(_deckWeAreEditing);

    }

    public void ConfigureAdd(DeckImprovementModel model)
    {
        _numCardsToAdd = model.NumberCardsToAdd;
        _deckWeAreEditing = new List<ScriptableCard>(model.CardChoicesToAdd);
        ConfigureGrid(_deckWeAreEditing);
    }
    
    public void ResetAllElements()
    {
        _cardIndexesRemoved = new List<int>();
        _cardIndexesUpgraded = new List<int>();
        _cardIndexesAdded = new List<int>();
        for (int i = 0; i < _cardViews.Count ; i++)
        {
            Destroy(_cardGameObjects[i]);
            _cardViews[i].DestroyCard();
        }
    }

    public void DidSelectConfirm()
    {
        if (_shopType == ShopType.Remove)
        {
            if (_cardIndexesRemoved.Count != _numCardsToRemove)
            {
                ShakeText();
            }
            else
            {
                // remove the cards from the deck
                for (int i = 0; i < _cardIndexesRemoved.Count; i++)
                {
                    ScriptableCard card = _deckWeAreEditing[_cardIndexesRemoved[i]];
                    _deckManager.RemoveCardFromDeck(card);
                }
                
                DidCompleteShop();
            }
        } else if (_shopType == ShopType.Upgrade)
        {
            if (_cardIndexesUpgraded.Count != _numCardsToUpgrade)
            {
                ShakeText();
            }
            else
            {
                // upgrade the cards from the deck
                for (int i = 0; i < _cardIndexesUpgraded.Count; i++)
                {
                    ScriptableCard card = _deckWeAreEditing[_cardIndexesUpgraded[i]];
                    _deckManager.UpgradeCard(card, _deckImprovements[_deckImprovementIndex].CardType);
                }
                
                DidCompleteShop();
            }
        } else if (_shopType == ShopType.Add)
        {
            if (_cardIndexesAdded.Count != _numCardsToAdd)
            {
                ShakeText();
            }
            else
            {
                // add the cards from the deck
                for (int i = 0; i < _cardIndexesAdded.Count; i++)
                {
                    ScriptableCard card = _deckWeAreEditing[_cardIndexesAdded[i]];
                    _deckManager.AddCardToDeck(card, _deckImprovements[_deckImprovementIndex].CardType);
                }
                
                DidCompleteShop();
            }
        }
    }

    public void ShakeText()
    {
        _instructionsTextView.transform.DOShakePosition(1.0f);
        _instructionsTextView.DOColor(Color.red, 1f).OnComplete(() =>
        {
            _instructionsTextView.DOColor(Color.white, 1f);
        });
    }

    public void DidCompleteShop()
    {
        _deckManagmentCanvasGroup.DOFade(0, 0.25f).OnComplete(() =>
        {
            ResetAllElements();
            _deckImprovementIndex++;

            if (_deckImprovementIndex >= _deckImprovements.Length)
            {
                // updates the current level index
                _progressManager.DidCompleteLevel();

                // make sure we call this after update progress manager
                _worldMapController.DidCompleteLevel();

            }
            else
            {
                ShowShop(_deckImprovements[_deckImprovementIndex]);
            }
        });

    }
}
