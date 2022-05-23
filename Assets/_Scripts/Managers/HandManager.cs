using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class HandManager : MonoBehaviour
{
    public static HandManager Instance;

    private List<CombinedCard> cards;

    [SerializeField] private GameObject MovementCardDeck;
    [SerializeField] private GameObject ModifierCardDeck;

    [SerializeField] private List<Transform> MovementCardSlotPlaceholders;
    [SerializeField] private List<Transform> ModiferCardSlotPlaceholders;

    private void Awake() {
        Instance = this;

        this.cards = new List<CombinedCard>();
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

}
