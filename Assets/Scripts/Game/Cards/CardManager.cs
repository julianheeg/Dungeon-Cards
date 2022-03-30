using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// a script that manages everything that happens to and with the cards in play
/// </summary>
public class CardManager : MonoBehaviour {

    [SerializeField]
    CardMeta CardPrefab;

    static Dictionary<int, CardMeta> cardDictionary;
    static int[] deckSizes;

    public static FieldAndHandHolder FieldsAndHands { private get; set; }

    static CardManager instance;

    //static CardManager instance;
    private void Awake()
    {
        instance = this;
        cardDictionary = new Dictionary<int, CardMeta>();
    }

    /// <summary>
    /// initializes the deck sizes for the current game
    /// </summary>
    /// <param name="deckSizes">the sizes of the decks that the players use</param>
    public static void Init(int[] deckSizes)
    {
        cardDictionary.Clear();

        CardManager.deckSizes = new int[deckSizes.Length];
        for (int i = 0; i < deckSizes.Length; i++)
        {
            CardManager.deckSizes[i] = deckSizes[i];
            //TODO Cube that resembles a deck
        }
    }

    /// <summary>
    /// registers a card with the card manager
    /// </summary>
    /// <param name="owner">the player the card belongs to</param>
    /// <param name="instanceID">the instance ID of the card</param>
    /// <param name="location">the location</param>
    public static void InitCard(int instanceID, int owner, CardMeta.Location location)
    {
        //assert that fields and hands has been set by the game object itself already
        Assert.IsNotNull(FieldsAndHands);

        //check if card is initialized
        if (!cardDictionary.ContainsKey(instanceID))
        {
            CardMeta card = Instantiate(instance.CardPrefab);
            card.Init(instanceID, owner, location);
            cardDictionary.Add(instanceID, card);

            //put card onto field
            FieldsAndHands.AddCard(card);
        }
        else
        {
            Debug.LogError("CardManager.InitCard(...): card dictionary contains card already. (instanceID = " + instanceID + ")");
        }
    }

    /// <summary>
    /// visualizes the face of a card
    /// </summary>
    /// <param name="instanceID">the instance ID of the card</param>
    /// <param name="cardID">the card to visualize as</param>
    public static void InitCardFace(int instanceID, int cardID)
    {
        //get card
        CardMeta card;
        if (cardDictionary.TryGetValue(instanceID, out card))
        {
            //init
            CardFace.Attach(card, cardID);
        }
        else
        {
            Debug.LogError("CardManager.InitCardFace(...): card can't be initialized because instanceID doesn't exist. (instanceID = " + instanceID + ", cardID = " + cardID + ")");
        }
    }

    /// <summary>
    /// moves a card to another position/board/location
    /// </summary>
    /// <param name="instanceID">the instance id of the card to move</param>
    /// <param name="destinationBoard">the future owner of this card; use -1 for the same as was before (this is useful for monster spawns for example)</param>
    /// <param name="destinationLocation">the future location of the card</param>
    public static void MoveCard(int instanceID, int destinationBoard, CardMeta.Location destinationLocation)
    {
        //get card
        CardMeta card;
        if (cardDictionary.TryGetValue(instanceID, out card))
        {
            if (destinationBoard == -1)
            {
                destinationBoard = card.owner;
            }

            /*
            //get previous values
            int sourceBoard = card.owner;
            CardMeta.Location sourceLocation = card.location;
            */

            //move card
            FieldsAndHands.MoveCard(card, destinationBoard, destinationLocation);
        }
        else
        {
            Debug.LogError("CardManager.MoveCard(...): card can't be initialized because instanceID doesn't exist. (instanceID = " + instanceID + ")");
        }
    }

    /// <summary>
    /// turns an instance id into a card id
    /// </summary>
    /// <param name="instanceID">the instance id to query</param>
    /// <returns>the associated card id</returns>
    public static CardMeta InstanceIDToCard(int instanceID)
    {
        CardMeta card;
        if (cardDictionary.TryGetValue(instanceID, out card))
        {
            return card;
        }
        else
        {
            throw new ArgumentException("CardManager.InstanceIDToCardID(instanceID): there is no card with instanceID " + instanceID);
        }
    }

    /*
#region Events

    public delegate void CardMovementEventHandler(object sender, CardMovementEventArgs args);
    public static event CardMovementEventHandler CardMoved;

    public static void OnCardMoved(Card card, int sourceBoard, Card.Location sourceLocation, int destinationBoard, Card.Location destinationLocation)
    {
        if (CardMoved != null)
        {
            CardMoved(card, new CardMovementEventArgs(card, sourceBoard, sourceLocation, destinationBoard, destinationLocation));
        }
    }



    public class CardMovementEventArgs : EventArgs
    {
        public readonly Card card;
        public readonly Card.Location sourceLoc, destLoc;
        public readonly int sourceBoard, destBoard;
        //TODO source and destination

        public CardMovementEventArgs(Card card, int sourceBoard, Card.Location sourceLocation, int destinationBoard, Card.Location destinationLocation)
        {
            this.card = card;
            this.sourceBoard = sourceBoard;
            this.sourceLoc = sourceLocation;
            this.destBoard = destinationBoard;
            this.destLoc = destinationLocation;
        }
    }
    
#endregion
*/
}