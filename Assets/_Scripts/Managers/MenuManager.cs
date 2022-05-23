using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;

    [SerializeField] private GameObject _floatingTileDetailView;
    [SerializeField] private GameObject _floatingUnitDetailView;
    [SerializeField] private GameObject _floatingTurnPhaseView;

    private void Awake() {
        Instance = this;
    }

    public void ShowTurnPhase(GameState gameState) {
        string turnPhase = "";
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
                break;
            case GameState.CreateDeck:
                turnPhase = "Create Deck";
                break;
            case GameState.DrawHand:
                turnPhase = "Draw Hand";
                break;
            case GameState.HeroTurn:
                turnPhase = "Hero Turn";
                break;
            case GameState.EnemyTurn:
                turnPhase = "Enemy Turn";
                break;
        }
        _floatingTurnPhaseView.GetComponentInChildren<Text>().text = turnPhase;

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
}
