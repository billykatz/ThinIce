using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Freya;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class HandManager : MonoBehaviour
{
    public static HandManager Instance;

    private List<CombinedCard> cards;
    private List<GameObject> cardGOs = new List<GameObject>();
    [SerializeField] private List<GameObject> testers;
    
    public float arcRadius;
    public float itemRadius;
    
    //[SerializeField] private float itemRadius = 2f;
    [SerializeField] private Transform handArc;

    [SerializeField] private GameObject MovementCardDeck;
    [SerializeField] private GameObject ModifierCardDeck;

    [SerializeField] private List<Transform> MovementCardSlotPlaceholders;
    [SerializeField] private List<Transform> ModiferCardSlotPlaceholders;

    [SerializeField] EventSystem m_EventSystem;

    private int _selectedIndex;
    private int _numPlayedCards = 0;

    private bool _playerIsPlayingCard = false;
    private bool _cardsChanged = false;

    private void Awake() {
        Instance = this;
        Debug.Log("Hand Manager Awake()");

        this.cards = new List<CombinedCard>();
        _selectedIndex = -1;

    }

    private void OnValidate()
    {
        if (cards.Count > 0)
        {
            ShowHand();
        }
    }

    public async void DrawHand() {
        Debug.Log("HandManager: Start drawing a card");
        _numPlayedCards = 0;
        int index = 0;
        while (cards.Count < 3) {
            CombinedCard drawnCard = DeckManager.Instance.DrawCard();
            cards.Add(drawnCard);

            Transform movementPlaceHolder = MovementCardSlotPlaceholders[index];
            Transform modifyPlaceHolder = ModiferCardSlotPlaceholders[index];

            GameObject combinedCard = drawnCard.InstantiateCombindCard(index);

            combinedCard.transform.position = movementPlaceHolder.position;
            cardGOs.Add(combinedCard);

            index++;

            ShowHand();

            await Task.Delay(200);
        }
        Debug.Log("HandManager: Finishing drawing a card");

        GameManager.Instance.EndGameState(GameState.DrawHand);
    }

    public void ShowHand()
    {

        if (cardGOs.Count <= 0)
        {
            return;
        }
        
        float a = itemRadius*2;
        float b = arcRadius;
        
        float separationAngRad = Mathfs.Acos(1f - (a * a) / (2 * b * b));
        float totalSeparation = separationAngRad * (cards.Count - 1f);
        float angRad = Mathfs.TAU * 0.25f - totalSeparation / 2;

        for (int i = 0; i < cardGOs.Count; i++)
        {
            cardGOs[i].transform.position = handArc.transform.position;
            cardGOs[i].transform.rotation = Quaternion.identity;
        }
        
        for (int i = 0; i < cardGOs.Count; i++)
        {
            Vector3 itemCenter = Mathfs.AngToDir(angRad) * arcRadius;
            // testers[i].transform.parent = handArc;
            cardGOs[i].transform.position += (itemCenter);
            Quaternion rotationAdd = Quaternion.AngleAxis(Mathfs.Rad2Deg * (angRad - Mathfs.TAU * 0.25f), Vector3.forward);
            cardGOs[i].transform.rotation *= rotationAdd; 
            angRad += separationAngRad;
        }
    }

    public void DiscardHand() {
        foreach(CombinedCard card in cards) {
            DeckManager.Instance.DiscardCard(card);
            card.DestroyCard();
            cards = new List<CombinedCard>();
            cardGOs = new List<GameObject>();
        }
    }

    public void DeselectAll() {
        if (_playerIsPlayingCard) { return; }
        foreach(CombinedCard card in cards) {
            card.SetSelectedBackground(false);
        }
        MenuManager.Instance.HideCardDetailView();
        _selectedIndex = -1;
        PlayerManager.Instance.HidePreview();
    }

    public void DidSelectCard(int index) {
        if (_playerIsPlayingCard) { return; }
        DeselectAll();
        _selectedIndex = index;
        cards[index].SetSelectedBackground(true);
        MenuManager.Instance.ShowCardDetailView(cards[index]);
    }

    public void DidSelectModifyTargetCard(CombinedCard card, ModifyTarget target) {
        if (_playerIsPlayingCard) { return; }
        PlayerManager.Instance.ShowPreview(card, target);
    }
    public void DidPlayCard(CombinedCard card, ModifyTarget target) {
        _numPlayedCards++;
        _playerIsPlayingCard = true;

        // save the stat updates
        PlayerManager.Instance.PlayedCard(card, target);

        // let the player move their character
        Debug.Log($"HandManager: {card}");
        CardRuleManager.Instance.PlayCard(card, target);
    }

    public void DidFinishPlayingCard(CombinedCard card) {
        // update our cards
        Debug.Log($"HandManager: Did finish playing card");
        List<CombinedCard> newCards = new List<CombinedCard>();
        int shift = 0;
        for (int i = 0; i < cards.Count; i++) {
            if (_selectedIndex != i) {
                cards[i].index -= shift;
                newCards.Add(cards[i]);
            } else {
                shift++;
            }
        }

        MenuManager.Instance.HideCardDetailView();

        // move the card to the discard pile
        DiscardCard(card);

        this.cards = newCards;

        if (_numPlayedCards == 1)  {
            GameManager.Instance.EndGameState(GameState.HeroTurnPlayCardOne);
        } else if (_numPlayedCards == 2) {
            GameManager.Instance.EndGameState(GameState.HeroTurnPlayCardTwo);
        }

        _playerIsPlayingCard = false;

    }

    public void DiscardCard(CombinedCard card) {
        DeckManager.Instance.DiscardCard(card);
        DeselectAll();

        card.DestroyCard();
    }

    public void DidHoverOverCard(int index) {
        if (_playerIsPlayingCard) { return; }
        if (_selectedIndex == -1) {
            MenuManager.Instance.ShowCardDetailView(cards[index]);
            cards[index].SetSelectedBackground(true);
        }
    }

    public void DidStopHoverOverCard(int index) {
        if (_playerIsPlayingCard) { return; }
        if (_selectedIndex == -1) {
            MenuManager.Instance.HideCardDetailView();
            cards[index].SetSelectedBackground(false);
        }
    }

    public void EndPlayerTurn() {
        DiscardHand();
        // await Task.Run(DrawHand);
        GameManager.Instance.EndGameState(GameState.HeroTurnCleanUp);
    }

    // Looks for clicks on the screen to handle deselection.  If the player clicks or taps on the UI then nothing is deselcted so the player can still tap on the card detail view.
    private void Update() {
        if (_playerIsPlayingCard) { return; }
        if (Input.GetMouseButtonDown(0)) {
            PointerEventData pointerEventData = new PointerEventData(m_EventSystem);
            pointerEventData.position = Input.mousePosition;
            var raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerEventData, raycastResults);
            if(raycastResults.Count > 0)
            {
                // purposefully left blank
            } else {
                Debug.Log("No UI hit - therefore we can handle deselection");
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity);
                if (hit.collider == null) {
                    DeselectAll();
                }
            }

        }

        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) {
            DidSelectCard(0);
        } else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) {
            DidSelectCard(1);
        } else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) {
            if (_numPlayedCards < 1) {
                DidSelectCard(2);
            }
        }
    }




}
