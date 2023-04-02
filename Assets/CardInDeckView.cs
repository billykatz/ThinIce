using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardInDeckView : MonoBehaviour
{

    [SerializeField] private Image _cardImage;
    [SerializeField] private Image _upgradedCardImage;
    
    [SerializeField] private GameObject _cardGO;
    [SerializeField] private GameObject _upgradedCardGO;
    
    [SerializeField] private GameObject _addButton;
    [SerializeField] private GameObject _removeButton;
    [SerializeField] private GameObject _upgradeButton;
    [SerializeField] private GameObject _addOrUpgradeBackground;
    [SerializeField] private GameObject _fullyUpgradeOverlay;
    [SerializeField] private GameObject _removeCardOverlay;
    
    private Action<int> _didSelectButton;
    private ScriptableCard _card;
    private int _index;
    private ShopType _shopType;

    public void Configure(ScriptableCard card, int index, ShopType shopType, Action<int> didSelectButton, GraphicRaycaster raycaster, EventSystem eventSystem)
    {
        _card = card;
        _index = index;
        _didSelectButton = didSelectButton;
        _shopType = shopType;

        ResetOverlaysAndBackgrounds();
        SetupButtons(raycaster, eventSystem);
        
        // set up the sprite 
        _cardImage.sprite = card.CardSprite;
        if (card.UpgradedVersionCard)
        {
            _upgradedCardImage.sprite = card.UpgradedVersionCard.CardSprite;
        }
        
        // Setup the buttons
        if (_shopType == ShopType.Add)
        {
            SetActiveButton(_addButton);
        } else if (_shopType == ShopType.Remove)
        {
            SetActiveButton(_removeButton);
        } else if (_shopType == ShopType.Upgrade)
        {
            SetActiveButton(_upgradeButton);
            if (card.UpgradedVersionCard == null)
            {
                ActivateOverlayBackground(_fullyUpgradeOverlay);
            }
        }
    }

    private void SetupButtons(GraphicRaycaster rayCaster, EventSystem eventSystem)
    {
        _addButton.GetComponent<ThinIceCanvasButton>()._raycaster = rayCaster;
        _addButton.GetComponent<ThinIceCanvasButton>()._eventSystem = eventSystem;
        _removeButton.GetComponent<ThinIceCanvasButton>()._raycaster = rayCaster;
        _removeButton.GetComponent<ThinIceCanvasButton>()._eventSystem = eventSystem;
        _upgradeButton.GetComponent<ThinIceCanvasButton>()._raycaster = rayCaster;
        _upgradeButton.GetComponent<ThinIceCanvasButton>()._eventSystem = eventSystem;
    }

    private void SetActiveButton(GameObject button)
    {
        _addButton.SetActive(false);
        _removeButton.SetActive(false);
        _upgradeButton.SetActive(false);
        button.SetActive(true);
    }

    private void DidSelectButton()
    {
        _didSelectButton?.Invoke(_index);
    }

    public void ResetOverlaysAndBackgrounds()
    {
        _cardGO.SetActive(true);
        _upgradedCardGO.SetActive(false);
        _removeCardOverlay.SetActive(false);
        _addOrUpgradeBackground.SetActive(false);
        _fullyUpgradeOverlay.SetActive(false);
    }

    private void ActivateOverlayBackground(GameObject overlayBackground)
    {
        ResetOverlaysAndBackgrounds();
        overlayBackground.SetActive(true);
    }
    
    public void DidSelectRemove()
    {
        ActivateOverlayBackground(_removeCardOverlay);
        DidSelectButton();
    }

    public void DidSelectUpgrade()
    {
        ActivateOverlayBackground(_addOrUpgradeBackground);
        _upgradedCardGO.SetActive(true);
        DidSelectButton();
    }
    
    public void DidSelectAdd()
    {
        ActivateOverlayBackground(_addOrUpgradeBackground);
        DidSelectButton();
    }
}
