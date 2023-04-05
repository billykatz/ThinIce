using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;


    [SerializeField] private GameObject _floatingTileDetailView;
    [SerializeField] private GameObject _floatingUnitDetailView;
    [SerializeField] private GameObject _floatingTurnPhaseView;
    [SerializeField] private TMP_Text _floatingTurnTextView;
    [SerializeField] private TMP_Text _floatingTileDetailTextView;
    [SerializeField] private TMP_Text _floatingTileDetailUnitView;

    private CombinedCard selectedCard;
    private ModifyTarget selectedTarget;


    private void Awake() {
        Instance = this;
    }

    public void ShowTurnPhase(GameState gameState) {
        string turnPhase = "";
        switch (gameState) {
            case GameState.GenerateGrid:
                turnPhase = "Generating Grid";
                break;
            case GameState.SpawnEnemies:
                turnPhase = "Spawning Enemies";
                break;
            case GameState.SpawnHazards:
                turnPhase = "Spawning Hazards";
                break;
            case GameState.SpawnItems:
                turnPhase = "Spawning Items";
                break;
            case GameState.PlaceHero:
                turnPhase = "Select a starting tile";
                break;
            case GameState.CreateDeck:
                turnPhase = "Create Deck";
                break;
            case GameState.DrawHand:
                turnPhase = "Draw Hand";
                break;
            case GameState.HeroTurnPlayCardOne:
                turnPhase = "Play a card. (0/2)";
                break;
            case GameState.HeroTurnPlayCardTwo:
                turnPhase = "Play another card. (1/2)";
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
        _floatingTurnTextView.text = turnPhase;

    }

    public void SetTextForEnemyTurn(string unitName, string intentions) {
        _floatingTurnPhaseView.SetActive(true);
        _floatingTurnTextView.text = $"{unitName} takes a turn and {intentions}";
    }

    public void ShowSelectedUnit(BaseUnit unit) {
        if (unit == null) {
            _floatingUnitDetailView.SetActive(false);
            return;
        }
        
        ShowItemDetail(unit.UnitName);
    }
    
    public void ShowSelectedHazard(BaseHazard hazard) {
        if (hazard == null) {
            _floatingUnitDetailView.SetActive(false);
            return;
        }
        ShowItemDetail(hazard.Name);
    }

    
    public void ShowSelectedItem(BaseItem item) {
        if (item == null) {
            _floatingUnitDetailView.SetActive(false);
            return;
        } 
        ShowItemDetail(item.Name);
    }

    private void ShowItemDetail(string name)
    {
        _floatingUnitDetailView.SetActive(true);
        _floatingTileDetailUnitView.text = name;
    }

    public void ShowSelectedTile(Tile tile) {
        if (tile == null) {
            _floatingTileDetailView.SetActive(false);
            _floatingUnitDetailView.SetActive(false);
            return;
        }
        _floatingTileDetailTextView.text = tile.TileName;
        _floatingTileDetailView.SetActive(true);
    }
    
    public void PlayCardInstructions(CombinedCard card, int moveIndex) {
        _floatingTurnTextView.text =  $"{card.movementCard.MovementTutorialText(moveIndex)}";
    }


}
