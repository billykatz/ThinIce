using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DeckModificationController : MonoBehaviour
{

    // this is the parent object that we will add items to
    [SerializeField] private GameObject _gridParent;
    [SerializeField] private GameObject _cardInModalView;
    [SerializeField] private GameObject _titleTextView;
    [SerializeField] private GameObject _instructionsTextView;
    [SerializeField] private GameObject _deckManagementView;
    [SerializeField] private DeckManager _deckManager;

    [SerializeField] private GraphicRaycaster _raycaster;
    [SerializeField] private EventSystem _eventSystem;
    
    

    private ShopType _shopType;

    private ScriptableCard[] _cardsToAdd;
    private int _numCardsToRemove;
    private int _numCardsToUpgrade;

    public void Configure(ScriptableLevelRules levelRules)
    {
        
        _shopType = levelRules.ShopType;

        if (_shopType == ShopType.Add)
        {
            ConfigureAdd(levelRules);
        } else if (_shopType == ShopType.Remove)
        {
            ConfigureRemove(levelRules);
            ConfigureGrid(_deckManager.GetDeck(CardType.Movement));
            
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
            deckViewCard.Configure(scriptableCards[i], i, _shopType, DidSelectItem, _raycaster, _eventSystem);
        }
    }

    public void DidSelectItem(int index)
    {
        
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
        _deckManagementView.SetActive(false);
    }
}
