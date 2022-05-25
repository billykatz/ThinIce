using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;

public class HandManager : MonoBehaviour
{
    public static HandManager Instance;

    private List<CombinedCard> cards;

    [SerializeField] private GameObject MovementCardDeck;
    [SerializeField] private GameObject ModifierCardDeck;

    [SerializeField] private List<Transform> MovementCardSlotPlaceholders;
    [SerializeField] private List<Transform> ModiferCardSlotPlaceholders;

    [SerializeField] EventSystem m_EventSystem;

    private int selectedIndex;

    private void Awake() {
        Instance = this;

        this.cards = new List<CombinedCard>();
        selectedIndex = -1;
    }

    public async void DrawHand() {
        Debug.Log("HandManager: Start drawing a card");
        int index = 0;
        while (cards.Count < 3) {
            CombinedCard drawnCard = DeckManager.Instance.DrawCard();
            cards.Add(drawnCard);

            Transform movementPlaceHolder = MovementCardSlotPlaceholders[index];
            Transform modifyPlaceHolder = ModiferCardSlotPlaceholders[index];

            GameObject combinedCard = drawnCard.InstantiateCombindCard(index);

            combinedCard.transform.position = movementPlaceHolder.position;

            index++;

            await Task.Delay(100);
        }
        Debug.Log("HandManager: Finishing drawing a card");
    }

    public void DiscardHand() {
        foreach(CombinedCard card in cards) {
            DeckManager.Instance.DiscardCard(card);
        }
    }

    public void DeselectAll() {
        foreach(CombinedCard card in cards) {
            card.SetSelectedBackground(false);
        }
        MenuManager.Instance.HideCardDetailView();
        selectedIndex = -1;
    }

    public void DidSelectCard(int index) {
        DeselectAll();
        selectedIndex = index;
        cards[index].SetSelectedBackground(true);
        MenuManager.Instance.ShowCardDetailView(cards[index]);
    }

    public void DidHoverOverCard(int index) {
        if (selectedIndex == -1) {
            MenuManager.Instance.ShowCardDetailView(cards[index]);
            cards[index].SetSelectedBackground(true);
        }
    }

    public void DidStopHoverOverCard(int index) {
        if (selectedIndex == -1) {
            MenuManager.Instance.HideCardDetailView();
            cards[index].SetSelectedBackground(false);
        }
    }

    // Looks for clicks on the screen to handle deselection.  If the player clicks or taps on the UI then nothing is deselcted so the player can still tap on the card detail view.
    private void Update() {
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
    }


}
