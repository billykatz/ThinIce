using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Freya;
using UnityEngine;
using UnityEngine.InputSystem;

public class HandManager : MonoBehaviour
{

    public static HandManager Instance;

    private List<CombinedCard> cards = new List<CombinedCard>();
    
    public float arcRadius;
    public float itemRadius;
    
    [SerializeField] private Transform HandArcTransform;
    [SerializeField] private Transform MovementDeckTransform;
    [SerializeField] private Transform ModifierDeckTransform;
    
    
    [SerializeField] private InputActionReference DidSelect;
    [SerializeField] private InputActionReference MousePosition;
    [SerializeField] private InputActionReference DidHold;
    [SerializeField] private InputActionReference DidRightButton;

    [SerializeField] private FXView AnimateCardsView;
    [SerializeField] private FXView AnimateOptionAreaViews;
    [SerializeField] private GameAnimator _gameAnimator;

    [SerializeField] private GameObject _swordOptionArea;
    [SerializeField] private GameObject _shieldOptionArea;
    [SerializeField] private float _highlightOptionDistanceThreshold;
    [SerializeField] private Vector3 _swordOptionOrigin;
    [SerializeField] private Vector3 _shieldOptionOrigin;
    [SerializeField] private Vector3 ShowOptionAreaVector;
    [SerializeField] private Vector3 HighlightOptionAreaVector;
    private Dictionary<ModifyTarget, GameObject> _optionAreas;

    private int _selectedIndex;
    private int _hoveredIndex;
    private int _draggedIndex;
    private int _numPlayedCards = 0;

    private bool _holdStarted = false;

    private float DelayOptionAreTimer = 0.25f;
    private float _delayOptionAreTimer = 0.25f;

    private bool _playerIsPlayingCard = false;
    private bool _cardsChanged = false;

    private void Awake() {
        Instance = this;
        Debug.Log("Hand Manager Awake()");
        _selectedIndex = -1;
        _draggedIndex = -1;
        _hoveredIndex = -1;

        _optionAreas = new Dictionary<ModifyTarget, GameObject>();
        _optionAreas.Add(ModifyTarget.Armor,_shieldOptionArea);
        _optionAreas.Add(ModifyTarget.Attack,_swordOptionArea);
        _optionAreas.Add(ModifyTarget.None,null);
        _swordOptionOrigin = _swordOptionArea.transform.position;
        _shieldOptionOrigin = _shieldOptionArea.transform.position;
    }

    private void Start()
    {
        DidSelect.action.performed += ctx => DidClick();
        DidRightButton.action.performed += ctx => RightButtonPressed();
        DidHold.action.performed += ctx => Hold();
        DidHold.action.canceled += ctx => HoldCancelled();
    }

    private void RightButtonPressed()
    {
        DeselectAll();
    }
    private void OnValidate()
    {
        if (cards.Count > 0)
        {
            ShowHand();
        }
    }

    private void HoldCancelled()
    {
        if (_draggedIndex != -1 && _holdStarted)
        {
            DeselectAll();
            _holdStarted = false;
        }
    }

    private void Hold()
    {
        if (_selectedIndex == -1)
            return;

        Vector2 mousePos = MousePosition.action.ReadValue<Vector2>();
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i].DoesRayCollides(ray) == _selectedIndex)
            {
                _draggedIndex = _selectedIndex;
                _holdStarted = true;
            }
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
        if (itemRadii.Length <= 0)
        {
            return data;
        }
        
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
        
        HideOptionAreas();
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
        _hoveredIndex = -1;
        _draggedIndex = -1;
        ShowNormalHand();
        PlayerManager.Instance.HidePreview();
    }

    public void DidSelectCard(int index) {
        if (_playerIsPlayingCard) { return; }

        if (_selectedIndex == index)
            return;
        
        // instead of deselcting all we just wanna turn off the background highlight
        foreach(CombinedCard card in cards) {
            card.SetSelectedBackground(false);
        }
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
        if (_selectedIndex != -1)
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
        }
    }

    public void EndPlayerTurn() {
        DiscardHand();
        GameManager.Instance.EndGameState(GameState.HeroTurnCleanUp);
    }


    private void HideOptionAreas()
    {
        ModifyTarget[] targets = new ModifyTarget[2] { ModifyTarget.Armor, ModifyTarget.Attack };
        
        for (int i = 0; i < targets.Length; i++)
        {
            ModifyTarget target = targets[i];
            AnimationData data = new AnimationData();
            data.StartPosition = _optionAreas[target].transform.position;
            data.EndPosition = (target == ModifyTarget.Armor ? _shieldOptionOrigin : _swordOptionOrigin);
            _gameAnimator.Animate(_optionAreas[target], data, AnimateCardsView);
        }
    }

    private void ShowOptionAreas()
    {
        ModifyTarget target = ModifyTarget.None;
        
        // measure the distance from the pointer both option areas.
        Vector2 pointerPos = MousePosition.action.ReadValue<Vector2>();
        Vector3 pointerWorldPos = Camera.main.ScreenToWorldPoint(pointerPos);
        float distanceToShield = Vector2.Distance(pointerWorldPos, _shieldOptionOrigin);
        float distanceToSword = Vector2.Distance(pointerWorldPos, _swordOptionOrigin);

        if (distanceToShield < _highlightOptionDistanceThreshold)
        {
            target = ModifyTarget.Armor;
        } else if (distanceToSword < _highlightOptionDistanceThreshold)
        {
            target = ModifyTarget.Attack;
        }
        
        Dictionary<ModifyTarget, AnimationData> animate = CalculateOptionAreaAnimation(target);
        foreach (ModifyTarget key in animate.Keys)
        {
            GameObject animationParent = _optionAreas[key];
            if (animationParent)
            {
                // animationParent.transform.position = animate[key].EndPosition;
                _gameAnimator.Animate(animationParent, animate[key], AnimateOptionAreaViews);
            }
        }
    }

    private Dictionary<ModifyTarget, AnimationData> CalculateOptionAreaAnimation(ModifyTarget highlightOption)
    {
        Dictionary<ModifyTarget, AnimationData> animations = new Dictionary<ModifyTarget, AnimationData>();
        ModifyTarget[] targets = new ModifyTarget[2] { ModifyTarget.Armor, ModifyTarget.Attack };

        Vector3 showVector = ShowOptionAreaVector;
        Vector3 highlightVector = HighlightOptionAreaVector;
        for (int i = 0; i < targets.Length; i++)
        {
            ModifyTarget target = targets[i];
            AnimationData data = new AnimationData();
            data.StartPosition = _optionAreas[target].transform.position;
            if (targets[i] == highlightOption)
            {
                data.EndPosition = (target == ModifyTarget.Armor ? _shieldOptionOrigin - highlightVector : _swordOptionOrigin + highlightVector);
            }
            else
            {
                data.EndPosition = (target == ModifyTarget.Armor ? _shieldOptionOrigin - showVector : _swordOptionOrigin + showVector);
            }
            
            animations.Add(target, data);
        }
        

        return animations;
    }

    // Looks for clicks on the screen to handle deselection.  If the player clicks or taps on the UI then nothing is deselcted so the player can still tap on the card detail view.
    private void Update() {
        if (_playerIsPlayingCard) { return; }

        if (_holdStarted)
        {
            float z = cards[_draggedIndex].transform.position.z;
            Vector3 newPos = Camera.main.ScreenToWorldPoint(MousePosition.action.ReadValue<Vector2>());
            newPos.z = z;
            _gameAnimator.CancelAnimation(cards[_draggedIndex].cardParent);
            cards[_draggedIndex].cardParent.transform.position = newPos;
            cards[_draggedIndex].cardParent.transform.rotation = Quaternion.identity;

            // then animate the option areas in
            _delayOptionAreTimer += Time.deltaTime;
            if (_delayOptionAreTimer > DelayOptionAreTimer)
            {
                ShowOptionAreas();
                _delayOptionAreTimer = 0;
            }
            
        }
        // if (Input.GetMouseButtonDown(0)) {
        //     PointerEventData pointerEventData = new PointerEventData(m_EventSystem);
        //     pointerEventData.position = Input.mousePosition;
        //     var raycastResults = new List<RaycastResult>();
        //     EventSystem.current.RaycastAll(pointerEventData, raycastResults);
        //     if(raycastResults.Count > 0)
        //     {
        //         // purposefully left blank
        //     } else {
        //         Debug.Log("No UI hit - therefore we can handle deselection");
        //         Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //         RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity);
        //         if (hit.collider == null) {
        //             DeselectAll();
        //         }
        //     }
        //
        // }

        // if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) {
        //     DidSelectCard(0);
        // } else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) {
        //     DidSelectCard(1);
        // } else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) {
        //     if (_numPlayedCards < 1) {
        //         DidSelectCard(2);
        //     }
        // }
    }

    private void DidClick()
    {
        if (cards.Count <= 0)
            return;
        
        Vector3 mousePosition = MousePosition.action.ReadValue<Vector2>();
        RaycastHit[] raycastResults = Physics.RaycastAll(mousePosition, Vector3.forward);
        if(raycastResults.Length > 0)
        {
            // purposefully left blank
        } else {
            Debug.Log("No UI hit - therefore we can handle deselection");
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);
            int count = 0;
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i].DoesRayCollides(ray) != -1)
                {
                    count++;
                }
            }
            
            if (count == 0) {
                // DeselectAll();
            }
        }

        //     if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) {
        //     DidSelectCard(0);
        // } else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) {
        //     DidSelectCard(1);
        // } else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) {
        //     if (_numPlayedCards < 1) {
        //         DidSelectCard(2);
        //     }
        // }
    }




}
