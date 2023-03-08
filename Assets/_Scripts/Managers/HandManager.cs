using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using Freya;
using UnityEngine.EventSystems;
using UnityEngine;

public class HandManager : MonoBehaviour
{

    public static HandManager Instance;

    private List<CombinedCard> cards = new List<CombinedCard>();
    
    public float arcRadius;
    public float itemRadius;
    
    [SerializeField] private Transform HandArcTransform;
    [SerializeField] private Transform MovementDeckTransform;
    [SerializeField] private Transform ModifierDeckTransform;

    [SerializeField] EventSystem m_EventSystem;

    // [SerializeField] private GameAnimatorReference _gameAnimatorReference;

    [SerializeField] private FXView AnimateCardsView;
    [SerializeField] private GameAnimator _gameAnimator;

    private int _selectedIndex;
    private int _numPlayedCards = 0;

    private bool _playerIsPlayingCard = false;
    private bool _cardsChanged = false;

    private void Awake() {
        Instance = this;
        Debug.Log("Hand Manager Awake()");
        _selectedIndex = -1;
        // _gameAnimator = _gameAnimatorReference.Animator;
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
        while (cards.Count < 3) {
            
            CombinedCard drawnCard = DeckManager.Instance.DrawCard();
            drawnCard.cardParent.transform.position = MovementDeckTransform.position;
            drawnCard.SetIndex(cards.Count);
            cards.Add(drawnCard);
            ShowDrawCard(drawnCard);
            
            await Task.Delay(1000);
            
            ShowHand();

            await Task.Delay(1000);
        }
        Debug.Log("HandManager: Finishing drawing a card");

        GameManager.Instance.EndGameState(GameState.DrawHand);
    }

    public void ShowDrawCard(CombinedCard card)
    {
        Vector3 startPosition = MovementDeckTransform.position;
        Vector3 endPosition = Vector3.zero;
        
        Quaternion startRotation = Quaternion.identity;
        Quaternion endRotation = Quaternion.identity;
        
        float a = itemRadius*2;
        float b = arcRadius;
        
        float separationAngRad = Mathfs.Acos(1f - (a * a) / (2 * b * b));
        float totalSeparation = separationAngRad * (cards.Count - 1f);
        float angRad = Mathfs.TAU * 0.25f - totalSeparation / 2;

        for (int i = 0; i < cards.Count; i++)
        {
            // cards[i].cardParent.transform.position = HandArcTransform.position;
            // cards[i].cardParent.transform.rotation = Quaternion.identity;
            //
            Vector3 itemCenter = Mathfs.AngToDir(angRad) * arcRadius;
            // cards[i].cardParent.transform.position = HandArcTransform.position + itemCenter;
            Quaternion rotationAdd = Quaternion.AngleAxis(Mathfs.Rad2Deg * (angRad - Mathfs.TAU * 0.25f), Vector3.forward);
            endRotation = cards[i].cardParent.transform.rotation * rotationAdd; 
            angRad += separationAngRad;

            endPosition = HandArcTransform.position + itemCenter;
        }

        AnimationData startData = new AnimationData();
        startData.position = startPosition;
        startData.rotation = startRotation;

        AnimationData endData = new AnimationData();
        endData.position = endPosition;
        endData.rotation = endRotation;

        _gameAnimator.Animate(card.cardParent, startData, endData, AnimateCardsView);

        
    }

    public void ShowHand()
    {

        if (cards.Count <= 0)
        {
            return;
        }
        
        float a = itemRadius*2;
        float b = arcRadius;
        
        float separationAngRad = Mathfs.Acos(1f - (a * a) / (2 * b * b));
        float totalSeparation = separationAngRad * (cards.Count - 1f);
        float angRad = Mathfs.TAU * 0.25f - totalSeparation / 2;

        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].cardParent.transform.position = HandArcTransform.position;
            cards[i].cardParent.transform.rotation = Quaternion.identity;
     
            Vector3 itemCenter = Mathfs.AngToDir(angRad) * arcRadius;
            cards[i].cardParent.transform.position = HandArcTransform.position + itemCenter;
            Quaternion rotationAdd = Quaternion.AngleAxis(Mathfs.Rad2Deg * (angRad - Mathfs.TAU * 0.25f), Vector3.forward);
            cards[i].cardParent.transform.rotation *= rotationAdd; 
            angRad += separationAngRad;
        }
        
        
        // _gameAnimator.Animate(drawnCard, );
    }

    public void DiscardHand() {
        foreach(CombinedCard card in cards) {
            DeckManager.Instance.DiscardCard(card);
            card.DestroyCard();
            cards = new List<CombinedCard>();
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
