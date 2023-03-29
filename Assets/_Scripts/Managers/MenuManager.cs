using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;

    [SerializeField] private Canvas mainCanvas;

    [SerializeField] private GameObject _floatingTileDetailView;
    [SerializeField] private GameObject _floatingUnitDetailView;
    [SerializeField] private GameObject _floatingTurnPhaseView;
    [SerializeField] private GameObject _floatingTurnTutorialView;

    [SerializeField] private GameObject movementHelperText;


    private CombinedCard selectedCard;
    private ModifyTarget selectedTarget;


    private void Awake() {
        Instance = this;
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
            case GameState.HeroTurnPlayCardOne:
                turnPhase = "Hero Turn";
                turnTutorial = "Play your first card (you will play 2 cards total)";
                break;
            case GameState.HeroTurnPlayCardTwo:
                turnPhase = "Hero Turn";
                turnTutorial = "Play your second card (the unplayed card is discarded)";
                break;
            case GameState.HeroTurnCleanUp:
                turnPhase = "Hero Turn Clean Up";
                break;
            case GameState.EnemyTurn:
                turnPhase = "Enemy Turn";
                break;
            case GameState.EndTurn:
                turnPhase = "Ending Turn";
                break;

        }
        _floatingTurnPhaseView.GetComponentInChildren<Text>().text = turnPhase;

        if (turnTutorial != "") {
            _floatingTurnTutorialView.SetActive(true);
            _floatingTurnTutorialView.GetComponentInChildren<Text>().text = turnTutorial;
        }

    }

    public void SetTextForEnemyTurn(string unitName, string intentions) {
        _floatingTurnPhaseView.SetActive(true);
        _floatingTurnPhaseView.GetComponentInChildren<Text>().text = $"{unitName} takes a turn";
        _floatingTurnTutorialView.SetActive(true);
        _floatingTurnTutorialView.GetComponentInChildren<Text>().text = $"{intentions}";
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


    private bool playingCardIgnoreInput = false;
    public void PlayCardInstructions(CombinedCard card, int moveIndex) {
        playingCardIgnoreInput = true;
        _floatingTurnPhaseView.GetComponentInChildren<Text>().text = "Move your Hero";
        _floatingTurnTutorialView.GetComponentInChildren<Text>().text = $"{card.movementCard.MovementTutorialText(moveIndex)}";
    }


}
