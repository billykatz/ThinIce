using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardDetailViewScript : MonoBehaviour
{
    [SerializeField] GameObject parentObject;

    [SerializeField] Image movementPlaceholder;
    [SerializeField] Image modifierPlaceholder;

    [SerializeField] Text movementText;
    [SerializeField] Text modifierText;

    [SerializeField] Button armorButton;
    [SerializeField] Button attackButton;
    [SerializeField] Button playButton;
    [SerializeField] Button cancelButton;

    private void Awake() {
        armorButton.onClick.AddListener(ArmorButtonClicked);
        attackButton.onClick.AddListener(AttackButtonClicked);
        playButton.onClick.AddListener(PlayButtonClicked);
        cancelButton.onClick.AddListener(CancelButtonClicked);


        parentObject.SetActive(false);
    }

    public void SetCard(CombinedCard card) {
        
    }

    public void ArmorButtonClicked() {

    }

    public void AttackButtonClicked() {
        
    }

    public void PlayButtonClicked() {
        
    }

    public void CancelButtonClicked() {
        
    }
}
