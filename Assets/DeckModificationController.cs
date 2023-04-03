using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class DeckModificationController : MonoBehaviour
{

    // this is the parent object that we will add items to
    [SerializeField] private WorldMapController _worldMapController;
    [SerializeField] private GameObject _gridParent;
    [SerializeField] private GameObject _cardInModalView;
    [SerializeField] private GameObject _titleTextView;
    [SerializeField] private TMP_Text _instructionsTextView;
    [SerializeField] private GameObject _deckManagementView;
    [SerializeField] private DeckManager _deckManager;

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

    private List<CardInDeckView> _cardViews;

    private void Awake()
    {
        _cardIndexesRemoved = new List<int>();
        _cardViews = new List<CardInDeckView>();
        _deckWeAreEditing = new List<ScriptableCard>();
    }

    public void Configure(ScriptableLevelRules levelRules)
    {
        _levelRules = levelRules;
        _shopType = levelRules.ShopType;

        if (_shopType == ShopType.Add)
        {
            ConfigureAdd(levelRules);
        } else if (_shopType == ShopType.Remove)
        {
            ConfigureRemove(levelRules);
            _deckWeAreEditing = _deckManager.GetDeck(CardType.Movement);
            ConfigureGrid(_deckWeAreEditing);
            
        } else if (_shopType == ShopType.Upgrade)
        {
            ConfigureUpgrade(levelRules);
        }
        
        _deckManagementView.SetActive(true);
    }

    public void ConfigureGrid(List<ScriptableCard> scriptableCards) 
    {
        for (int i = 0; i < scriptableCards.Count; i++)
        {
            GameObject newCard = Instantiate(_cardInModalView, _gridParent.transform);
            CardInDeckView deckViewCard = newCard.GetComponent<CardInDeckView>();
            deckViewCard.Configure(scriptableCards[i], i, _shopType, DidSelectItem, DidDeselectItem, _raycaster, _eventSystem);
            _cardViews.Add(deckViewCard);
        }
    }

    public void DidDeselectItem(int index)
    {
        if (_shopType == ShopType.Remove)
        {
            _cardIndexesRemoved.Remove(index);
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
        }
    }
    
    public void ConfigureUpgrade(ScriptableLevelRules levelRules)
    {
        _numCardsToUpgrade = levelRules.NumberCardsToUpgrade;
    }

    public void ConfigureRemove(ScriptableLevelRules levelRules)
    {
        _numCardsToRemove = levelRules.NumberCardsToRemove;
    }

    public void ConfigureAdd(ScriptableLevelRules levelRules)
    {
        _cardsToAdd = levelRules.CardsToAdd;
    }
    
    public void ResetAllElements()
    {
        
    }

    public void DidSelectConfirm()
    {
        if (_shopType == ShopType.Remove)
        {
            if (_cardIndexesRemoved.Count != _numCardsToRemove)
            {
                _instructionsTextView.transform.DOShakePosition(1.0f);
                _instructionsTextView.DOColor(Color.red, 1f).OnComplete(() =>
                {
                    _instructionsTextView.DOColor(Color.white, 1f);
                });
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
        }
    }

    public void DidCompleteShop()
    {
        _deckManagementView.SetActive(false);
        
        // updates the current level index
        _progressManager.DidCompleteLevel();
        
        // make sure we call this after update progress manager
        _worldMapController.DidCompleteLevel();
    }
}
