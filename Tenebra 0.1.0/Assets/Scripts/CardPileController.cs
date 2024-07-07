using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardPileController : MonoBehaviour
{
    public static CardPileController instance;

    private void Awake()
    {
        instance = this;
    }

    public List<CardSO> drawPile = new List<CardSO>();
    public List<CardSO> discardPile = new List<CardSO>();

    public bool isDrawPile;

    public GameObject cardSlotPrefab; // CardSlot prefab�n� bu de�i�kene atay�n
    public Transform cardSlotParent; // Scroll View Content objesinin transformu

    // Start is called before the first frame update
    void Start()
    {
        SetupPile();
    }

    // Kartlar� CardSlot prefablar�na yerle�tir
    private void CreateDrawPileCardSlots()
    {
        ClearCardSlots();
        foreach (CardSO card in drawPile)
        {
            GameObject newCardSlot = Instantiate(cardSlotPrefab, cardSlotParent);
            CardSlot cardSlot = newCardSlot.GetComponent<CardSlot>();
            cardSlot.SetupCardSlot(card);
        }
    }

    private void CreateDiscardPileCardSlots()
    {
        ClearCardSlots();
        foreach (CardSO card in discardPile)
        {
            GameObject newCardSlot = Instantiate(cardSlotPrefab, cardSlotParent);
            CardSlot cardSlot = newCardSlot.GetComponent<CardSlot>();
            cardSlot.SetupCardSlot(card);
        }
    }

    public void SetupPile()
    {
        if (isDrawPile)
        {
            drawPile = DeckController.instance.drawDeck;
            CreateDrawPileCardSlots();
        }
        else
        {
            CreateDiscardPileCardSlots();
        }
    }

    private void ClearCardSlots()
    {
        foreach (Transform child in cardSlotParent)
        {
            Destroy(child.gameObject);
        }
    }

    public void AddToDiscardPile(CardSO card)
    {
        discardPile.Add(card);
        CreateDiscardPileCardSlots();
    }

    public void DiscardToDraw()
    {
        drawPile = discardPile;
        discardPile.Clear();
        SetupPile();
    }
}
