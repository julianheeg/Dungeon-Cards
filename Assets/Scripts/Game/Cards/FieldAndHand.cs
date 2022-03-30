using Assets.Scripts.Game.Cards;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// a script that manages a player's card field
/// </summary>
public class FieldAndHand : MonoBehaviour {
    
    [SerializeField]
    CardPile deck, graveyard;
    [SerializeField]
    Hand hand;
    [SerializeField]
    Field field;

    public CardPile Deck        { get { return deck; } }
    public CardPile Graveyard   { get { return graveyard; } }
    public Hand Hand            { get { return hand; } }
    public Field Field          { get { return field; } }

    //TODO remove?
#pragma warning disable 0414
    int playerIndex;
#pragma warning restore 0414

    /// <summary>
    /// sets the player index who this game object belongs to and positions deck, graveyard and hand accordingly
    /// </summary>
    /// <param name="playerIndex">the player's index</param>
    public void Init(int playerIndex)
    {
        this.playerIndex = playerIndex;

        Deck.gameObject.transform.localPosition = new Vector3(0, 0, MapManager.IsHexGrid ? MapManager.Width / 2f : MapManager.Width / 4f); //right side of the map
        Graveyard.gameObject.transform.localPosition = new Vector3(0, 0, 0); //left side of the map
        Hand.gameObject.transform.localPosition = new Vector3(0, 0, MapManager.IsHexGrid ? MapManager.Width / 4f : MapManager.Width / 8f); //in the middle between deck and graveyard
    }

    /// <summary>
    /// takes a location and returns the corresponding card collection in this class
    /// </summary>
    /// <param name="location">the location that is queried</param>
    /// <returns>the card collection that corresponds to the location</returns>
    public CardCollection GetCardCollection(CardMeta.Location location)
    {
        switch (location)
        {
            case CardMeta.Location.Deck:
                return Deck;

            case CardMeta.Location.Hand:
                return Hand;

            case CardMeta.Location.Field:
                return Field;

            case CardMeta.Location.Graveyard:
                return Graveyard;

            default:
                throw new NotImplementedException();
        }
    }

    /// <summary>
    /// adds a card to the specified location
    /// </summary>
    /// <param name="card">the card to add</param>
    /// <param name="location">the location where this card should be added</param>
    /// <param name="animate">whether the card movement should be animated</param>
    public void Add(CardMeta card, CardMeta.Location location, bool animate)
    {
        CardCollection collection = GetCardCollection(location);
        collection.Add(card, animate);
    }

    /// <summary>
    /// removes a card from its location
    /// </summary>
    /// <param name="card">the card to remove</param>
    public void Remove(CardMeta card)
    {
        CardCollection collection = GetCardCollection(card.location);
        collection.Remove(card);
    }
}