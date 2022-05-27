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
    [SerializeField] private ThinIceToggle cardDetailArmorToggle;
    [SerializeField] private ThinIceToggle cardDetailAttackToggle;
    [SerializeField] private Button cardDetailPlayButton;
    [SerializeField] private Button cardDetailCancelButton;


    [SerializeField] private Image cardDetailArmorButtonImage;
    [SerializeField] private Image cardDetailAttackButtonImage;

    [SerializeField] private Sprite cardDetailButtonOriginalImage;
    [SerializeField] private Sprite cardDetailButtonFlashImage;

    [SerializeField] private GameObject _floatingTileDetailView;
    [SerializeField] private GameObject _floatingUnitDetailView;
    [SerializeField] private GameObject _floatingTurnPhaseView;
    [SerializeField] private GameObject _floatingTurnTutorialView;

    [SerializeField] private GameObject movementHelperText;


    private CombinedCard selectedCard;
    private ModifyTarget selectedTarget;


    private void Awake() {
        Instance = this;
        cardDetailView.SetActive(true);
        cardDetailView.SetActive(false);

        selectedTarget = ModifyTarget.None;

        cardDetailAttackToggle.SetToggleOn(false);
        cardDetailArmorToggle.SetToggleOn(false);

        cardDetailPlayButton.onClick.AddListener(PlayButtonSelected);
        cardDetailCancelButton.onClick.AddListener(CancelButtonSelected);
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
        Debug.Log($"MenuManager - ShowCardDetail {card.movementCard}");
        selectedCard = card;
        selectedTarget = ModifyTarget.None;
        cardDetailView.SetActive(true);

        // toggle this stuff off
        cardDetailAttackToggle.SetToggleOn(false);
        cardDetailArmorToggle.SetToggleOn(false);

        // toggle these things correctly
        movementHelperText.SetActive(false);
        cardDetailPlayButton.gameObject.SetActive(true);
        cardDetailCancelButton.gameObject.SetActive(true);

        // set the sprites
        cardDetailMovementPlaceholder.sprite = card.movementCard._spriteRenderer.sprite;
        cardDetailModifyPlaceholder.sprite = card.modifierCard._spriteRenderer.sprite;

        // set the effect descriptions
        cardDetailMovementText.text = card.movementCard.effectDescription;
        cardDetailModifierText.text = card.modifierCard.effectDescription;
    }

    public void HideCardDetailView() {
        Debug.Log($"MenuManager - HideCardDetail");
        cardDetailView.SetActive(false);
        selectedCard = null;
        playingCardIgnoreInput = false;
    }

    public void ArmorButtonSelected() {
        if (playingCardIgnoreInput) { return; }
        selectedTarget = ModifyTarget.Armor;
        HandManager.Instance.DidSelectModifyTargetCard(selectedCard, selectedTarget);
        cardDetailArmorToggle.SetToggleOn(true);
        cardDetailAttackToggle.SetToggleOn(false);
    }

    public void AttackButtonSelected() {
        if (playingCardIgnoreInput) { return; }
        selectedTarget = ModifyTarget.Attack;
        HandManager.Instance.DidSelectModifyTargetCard(selectedCard, selectedTarget);
        cardDetailAttackToggle.SetToggleOn(true);
        cardDetailArmorToggle.SetToggleOn(false);
    }

    public void PlayButtonSelected() {
        if (playingCardIgnoreInput) { return; }
        if (selectedTarget != ModifyTarget.None) {
            HandManager.Instance.DidPlayCard(selectedCard, selectedTarget);
            movementHelperText.SetActive(true);
            cardDetailPlayButton.gameObject.SetActive(false);
            cardDetailCancelButton.gameObject.SetActive(false);
        } else {
            StartCoroutine(FlashButtons());
        }
    }

    private IEnumerator FlashButtons() {
        var delay = 0.20f;

        cardDetailAttackButtonImage.sprite = cardDetailButtonFlashImage;
        cardDetailArmorButtonImage.sprite = cardDetailButtonFlashImage;

        yield return new WaitForSeconds(delay);

        cardDetailAttackButtonImage.sprite = cardDetailButtonOriginalImage;
        cardDetailArmorButtonImage.sprite = cardDetailButtonOriginalImage;

        yield return new WaitForSeconds(delay);

        cardDetailAttackButtonImage.sprite = cardDetailButtonFlashImage;
        cardDetailArmorButtonImage.sprite = cardDetailButtonFlashImage;
        
        yield return new WaitForSeconds(delay);

        cardDetailAttackButtonImage.sprite = cardDetailButtonOriginalImage;
        cardDetailArmorButtonImage.sprite = cardDetailButtonOriginalImage;
    }

    public void CancelButtonSelected() {
        if (playingCardIgnoreInput) { return; }
        selectedCard = null;
        selectedTarget = ModifyTarget.None;
        cardDetailAttackToggle.SetToggleOn(false);
        cardDetailArmorToggle.SetToggleOn(false);
        HandManager.Instance.DeselectAll();
    }

    private void Update() {
        if (this.isActiveAndEnabled) {
            if (Input.GetKeyDown(KeyCode.Z)) {
                ArmorButtonSelected();
            } else if (Input.GetKeyDown(KeyCode.X)) {
                AttackButtonSelected();
            } else if (Input.GetKeyDown(KeyCode.Space)) {
                PlayButtonSelected();
            } else if (Input.GetKeyDown(KeyCode.Escape)) {
                CancelButtonSelected();
            }
        }
    }

    private bool playingCardIgnoreInput = false;
    public void PlayCardInstructions(CombinedCard card, int moveIndex) {
        playingCardIgnoreInput = true;
        _floatingTurnPhaseView.GetComponentInChildren<Text>().text = "Move your Hero";
        _floatingTurnTutorialView.GetComponentInChildren<Text>().text = $"{card.movementCard.MovementTutorialText(moveIndex)}";
    }


}
