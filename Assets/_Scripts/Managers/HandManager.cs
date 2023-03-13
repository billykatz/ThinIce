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

    [SerializeField] private FXView AnimateCardsView;
    [SerializeField] private GameAnimator _gameAnimator;

    private int _selectedIndex;
    private int _hoveredIndex;
    private int _numPlayedCards = 0;

    private bool _playerIsPlayingCard = false;
    private bool _cardsChanged = false;

    private void Awake() {
        Instance = this;
        Debug.Log("Hand Manager Awake()");
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
        while (cards.Count < 3) {
            
            CombinedCard drawnCard = DeckManager.Instance.DrawCard();
            drawnCard.cardParent.transform.position = MovementDeckTransform.position;
            drawnCard.SetIndex(cards.Count);
            cards.Add(drawnCard);
            ShowDrawCard(drawnCard, itemRadius, arcRadius);
            
            await Task.Delay(500);
        }
        Debug.Log("HandManager: Finishing drawing a card");

        GameManager.Instance.EndGameState(GameState.DrawHand);
    }

    private  AnimationData[] CalculateCardAnimationData(float[] itemRadii, float arcRadius)
    {
        
        float[] iRadii = new float[cards.Count];
        float[] aRadii = new float[cards.Count];
        for (int i = 0; i < cards.Count; i++)
        {
            iRadii[i] = itemRadius;
            aRadii[i] = arcRadius;
        }
        
        return CalculateCardAnimationData(iRadii, aRadii);

    }

    public AnimationData[] CalculateCardAnimationData(float[] itemRadii, float[] arcRadii)
    {

        AnimationData[] data = new AnimationData[cards.Count];
        
        float[] anglesBetween = new float[itemRadii.Length-1];
        float totalSeparation = 0;
        for (int i = 0; i < anglesBetween.Length; i++)
        {
            float a = itemRadii[i] + itemRadii[i+1];
            float b = arcRadii[i];
            
            float separationAngRad = Mathfs.Acos(1f - (a * a) / (2f * b * b));
            totalSeparation += separationAngRad;

            anglesBetween[i] = separationAngRad;
        }

        float angleBetweenRad = Mathfs.TAU * 0.25f - (totalSeparation / 2f);
        for (int i = 0; i < cards.Count; i++)
        {
            Vector3 startPosition = cards[i].cardParent.transform.position;
            Quaternion startRotation = cards[i].cardParent.transform.rotation;

            // cards[i].cardParent.transform.position = HandArcTransform.position;
            cards[i].cardParent.transform.rotation = Quaternion.identity;

            Vector3 itemCenter = Mathfs.AngToDir(angleBetweenRad) * arcRadii[i];
            Vector3 endPosition = HandArcTransform.position + itemCenter;
            
            Quaternion rotationAdd = Quaternion.AngleAxis(Mathfs.Rad2Deg * (angleBetweenRad - Mathfs.TAU * 0.25f), Vector3.forward);
            Quaternion endRotation = cards[i].cardParent.transform.rotation * rotationAdd;
            
            AnimationData startData = new AnimationData();
            startData.StartPosition = startPosition;
            startData.StartRotation = startRotation;
            
            startData.EndPosition = endPosition;
            startData.EndRotation = endRotation;
            
            data[i] = startData;
            
            if (i < anglesBetween.Length)
            {
                angleBetweenRad += anglesBetween[i];
            }

        }

        return data;
    }

    public void ShowNormalHand()
    {
        float[] itemRadii = new float[cards.Count];
        for (int i = 0; i < cards.Count; i++)
        {
            itemRadii[i] = itemRadius;
        }
        
        AnimationData[] data = CalculateCardAnimationData(itemRadii, arcRadius);
        
        for (int i = 0; i < data.Length; i++)
        {
            _gameAnimator.Animate(cards[i].cardParent, data[i], AnimateCardsView);
        }
    }

    public void ShowDrawCard(CombinedCard card, float itemRadius, float arcRadius)
    {

        float[] itemRadii = new float[cards.Count];
        for (int i = 0; i < cards.Count; i++)
        {
            itemRadii[i] = itemRadius;
        }
        
        AnimationData[] data = CalculateCardAnimationData(itemRadii, arcRadius);
        
        for (int i = 0; i < data.Length; i++)
        {
            if (card.index == i)
            {
                data[i].StartPosition = MovementDeckTransform.position;
                data[i].StartRotation = Quaternion.identity;
            }
            _gameAnimator.Animate(cards[i].cardParent, data[i], AnimateCardsView);
        }
    }

    /// <summary>
    /// Highlights the hovered/selected card by 
    /// </summary>
    /// <param name="highlightedCard"></param>
    public void HighlightCard(CombinedCard highlightedCard)
    {
        float[] itemRadii = new float[cards.Count];
        float[] arcRadii = new float[cards.Count];
        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i] == highlightedCard)
            {
                itemRadii[i] = itemRadius * 1.1f;
                arcRadii[i] = arcRadius * 1.02f;
            }
            else
            {
                itemRadii[i] = itemRadius * 0.9f;
                arcRadii[i] = arcRadius;
            }
        }
        
        AnimationData[] data = CalculateCardAnimationData(itemRadii, arcRadii);
        
        for (int i = 0; i < data.Length; i++)
        {
            if (highlightedCard.index == i)
            {
                // data[i].EndPosition = data[i].StartPosition;
                data[i].EndRotation = data[i].StartRotation;
            }
            _gameAnimator.Animate(cards[i].cardParent, data[i], AnimateCardsView);
        }
        
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
        ShowNormalHand();
        PlayerManager.Instance.HidePreview();
    }

    public void DidSelectCard(int index) {
        if (_playerIsPlayingCard) { return; }

        if (_selectedIndex == index)
            return;
        
        DeselectAll();
        _selectedIndex = index;
        cards[index].SetSelectedBackground(true);
        HighlightCard(cards[index]);
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
        
        ShowNormalHand();

    }

    public void DiscardCard(CombinedCard card) {
        DeckManager.Instance.DiscardCard(card);
        DeselectAll();

        card.DestroyCard();
    }

    public void DidHoverOverCard(int index) {
        if (_playerIsPlayingCard) { return; }
        if (_hoveredIndex == index)
            return;
        if (_hoveredIndex == -1)
        {
            _hoveredIndex = index;
            MenuManager.Instance.ShowCardDetailView(cards[index]);
            cards[index].SetSelectedBackground(true);
            HighlightCard(cards[index]);
        }
    }

    public void DidStopHoverOverCard(int index) {
        if (_playerIsPlayingCard) { return; }

        if (_selectedIndex == index)
        {
            // dont do anything if they move away from the selected (and hovered) card
            return;
        }

        if (_hoveredIndex == index)
        {
            _hoveredIndex = -1;
            DeselectAll();
            MenuManager.Instance.HideCardDetailView();
            // cards[index].SetSelectedBackground(false);
            // ShowNormalHand();
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
