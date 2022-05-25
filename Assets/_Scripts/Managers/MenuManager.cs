using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;

    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private GameObject cardDetailView;
    [SerializeField] private Image cardDetailMovementPlaceholder;
    [SerializeField] private Image cardDetailModifyPlaceholder;
    [SerializeField] private Text cardDetailMovementText;
    [SerializeField] private Text cardDetailModifierText;

    [SerializeField] private GameObject _floatingTileDetailView;
    [SerializeField] private GameObject _floatingUnitDetailView;
    [SerializeField] private GameObject _floatingTurnPhaseView;
    [SerializeField] private GameObject _floatingTurnTutorialView;

    private void Awake() {
        Instance = this;
        cardDetailView.SetActive(true);
        cardDetailView.SetActive(false);
    }

    public void ShowTurnPhase(GameState gameState) {
        _floatingTurnTutorialView.SetActive(false);
        string turnPhase = "";
        string turnTutorial = "";
        switch (gameState) {
            case GameState.GenerateGrid:
                turnPhase = "Generate Grid";
                break;
            case GameState.SpawnEnemies:
                turnPhase = "Spawn Enemies";
                break;
            case GameState.SpawnHazards:
                turnPhase = "Spawn Hazards";
                break;
            case GameState.SpawnItems:
                turnPhase = "Spawn Items";
                break;
            case GameState.PlaceHero:
                turnPhase = "Place Hero";
                turnTutorial = "Place Buck in the highlighted area";
                break;
            case GameState.CreateDeck:
                turnPhase = "Create Deck";
                break;
            case GameState.DrawHand:
                turnPhase = "Draw Hand";
                break;
            case GameState.HeroTurn:
                turnPhase = "Hero Turn";
                turnTutorial = "Play two cards";
                break;
            case GameState.EnemyTurn:
                turnPhase = "Enemy Turn";
                break;
        }
        _floatingTurnPhaseView.GetComponentInChildren<Text>().text = turnPhase;

        if (turnTutorial != "") {
            _floatingTurnTutorialView.SetActive(true);
            _floatingTurnTutorialView.GetComponentInChildren<Text>().text = turnTutorial;
        }

    }

    public void ShowSelectedUnit(BaseUnit unit) {
        if (unit == null) {
            _floatingUnitDetailView.SetActive(false);
            return;
        }

        _floatingUnitDetailView.SetActive(true);
        _floatingUnitDetailView.GetComponentInChildren<Text>().text = unit.UnitName;
    }

    public void ShowSelectedTile(Tile tile) {
        if (tile == null) {
            _floatingTileDetailView.SetActive(false);
            return;
        }
        _floatingTileDetailView.GetComponentInChildren<Text>().text = tile.TileName;
        _floatingTileDetailView.SetActive(true);
    }

    public void ShowCardDetailView(CombinedCard card) {
        cardDetailView.SetActive(true);

        // set the sprites
        cardDetailMovementPlaceholder.sprite = card.movementCard._spriteRenderer.sprite;
        cardDetailModifyPlaceholder.sprite = card.modifierCard._spriteRenderer.sprite;

        // set the effect descriptions
        cardDetailMovementText.text = card.movementCard.effectDescription;
        cardDetailModifierText.text = card.modifierCard.effectDescription;
    }

    public void HideCardDetailView() {
        cardDetailView.SetActive(false);
    }
}
