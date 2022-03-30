using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// a script that handles a collection of cards, for example a pile or the hand cards
/// </summary>
public abstract class CardCollection : MonoBehaviour {

    protected List<CardMeta> cards = new List<CardMeta>();
    [SerializeField]
    protected Vector3 cardAddPosition = Vector3.zero;
    [SerializeField]
    protected Vector3 cardDefaultRotationEulerAngles;
    protected Quaternion cardDefaultRotation;

    private void Awake()
    {
        cardDefaultRotation = Quaternion.Euler(cardDefaultRotationEulerAngles);
    }

    /// <summary>
    /// amount of cards in this collection
    /// </summary>
    public int Count
    {
        get
        {
            return cards.Count;
        }
    }

    /// <summary>
    /// adds a card to the collection
    /// </summary>
    /// <param name="card">the card to put on this pile</param>
    public abstract void Add(CardMeta card, bool animate);

    /// <summary>
    /// sets the card's transform when it is initially put onto the field (called only once per card)
    /// </summary>
    /// <param name="card"></param>
    public void InitialCardPositionAndRotation(CardMeta card)
    {
        card.gameObject.transform.position = cardAddPosition;
        card.gameObject.transform.rotation = cardDefaultRotation;
    }

    /// <summary>
    /// removes a card from the collection
    /// </summary>
    /// <param name="card">the card to remove</param>
    public virtual void Remove(CardMeta card)
    {
        if (!cards.Remove(card))
        {
            Debug.LogError("CardCollection.Remove(...): card can't be taken because it is not in this collection. (cardAddPosition = " + cardAddPosition.ToString() + ", cardDefaultRotation = " + cardDefaultRotation.ToString() + ", instanceID = " + card.instanceID +")");
        }
    }
}