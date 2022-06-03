using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WinLoseManager : MonoBehaviour
{
    public static WinLoseManager Instance;

    [SerializeField] private GameObject youWonParent;
    [SerializeField] private GameObject youLoseParent;

    [SerializeField] private ThinIceCanvasButton heartReward;
    [SerializeField] private ThinIceCanvasButton shieldReward;
    [SerializeField] private ThinIceCanvasButton attackReward;
    [SerializeField] private Button continueButton;

    private ThinIceCanvasButton selectedButton;
    private List<ThinIceCanvasButton> buttons = new List<ThinIceCanvasButton>();

    private void Awake() {
        Instance = this;

        buttons.Add(heartReward);
        buttons.Add(shieldReward);
        buttons.Add(attackReward);
    }

    private void Start() {
        ThinIceCanvasButton.OnClicked += ButtonClicked;
        CheckContinueButton();
        youWonParent.SetActive(false);
        youLoseParent.SetActive(false);
    }

    private void OnDestroy() {
        ThinIceCanvasButton.OnClicked -= ButtonClicked;
    }

    public void GameWin() {
        youWonParent.SetActive(true);
    }

    public void GameLose() {
        youLoseParent.SetActive(true);
    }

    public void ButtonClicked(bool isSelected, string id) {
        Debug.Log($"id: {id}, isSelected: {isSelected}");
        buttons.ForEach(btn=>btn.ResetButton());
        selectedButton = null;

        if (id == "heart" && isSelected) {
            Debug.Log($"WinLoseManager: heart");
            heartReward.SelectButton();
            selectedButton = heartReward;
        } else if (id == "shield" && isSelected) {
            Debug.Log($"WinLoseManager: shield");
            shieldReward.SelectButton();
            selectedButton = shieldReward;
        } else if (id == "attack" && isSelected) {
            Debug.Log($"WinLoseManager: attack");
            attackReward.SelectButton();
            selectedButton = attackReward;
        } 

        CheckContinueButton();
    }

    private void CheckContinueButton() {
        string selectedButtonStr = (selectedButton == null) ? "nothing" : selectedButton.identifier;
        Debug.Log($"WinLoseManager: Selected button? {selectedButtonStr}");

        continueButton.interactable = (selectedButton != null);
    }

    public void ContinueButtonPressed() {
        string selectedButtonStr = (selectedButton == null) ? "nothing" : selectedButton.identifier;
        Debug.Log($"WinLoseManager: Selected button? {selectedButtonStr}");

        // save the data to the player

        // move to the next scene
    }

    public void MainMenuButtonPressed() {
        
    }



}
