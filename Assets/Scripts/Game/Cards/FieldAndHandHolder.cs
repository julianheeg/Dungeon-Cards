using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// a script that manages card movement between the players' hands, fields, graveyards and decks
/// </summary>
public class FieldAndHandHolder : MonoBehaviour {

    [SerializeField]
    FieldAndHand FieldAndHandPrefab;    //prefabs can't be readonly

    //transforms (positions and rotations) for each player field
    public static Vector3[] positions;
    public static Quaternion[] rotations;

    FieldAndHand[] cardBoard;

    /// <summary>
    /// initialization and instantiation of the FieldAndHand objects
    /// </summary>
    private void Awake()
    {
        CardManager.FieldsAndHands = this;
        CalculateTransforms();

        cardBoard = new FieldAndHand[GameManager.NumberOfPlayers];
        for (int i = 0; i < GameManager.NumberOfPlayers; i++)
        {
            cardBoard[i] = Instantiate(FieldAndHandPrefab);
            cardBoard[i].gameObject.transform.SetParent(this.transform);
            cardBoard[i].gameObject.transform.position = positions[i];
            cardBoard[i].gameObject.transform.rotation = rotations[i];
            cardBoard[i].Init(i);
        }
    }

    /// <summary>
    /// calculates the positions of all players depending on the number of players in the game
    /// </summary>
    private void CalculateTransforms()
    {
        if (GameManager.NumberOfPlayers == 2)
        {
            positions = new Vector3[] { new Vector3(1, 0, 0), new Vector3(-(MapManager.Length - 1) * Mathf.Sqrt(3) * 0.5f - 1, 0, MapManager.Width / 2) };
            rotations = new Quaternion[] { Quaternion.identity, Quaternion.Euler(0, 180, 0) };
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// brings a new card into play and puts it onto the according collection
    /// </summary>
    /// <param name="card">the card to bring into play. Must be initialized with instance first</param>
    public void AddCard(CardMeta card)
    {
        FieldAndHand fieldAndHand = cardBoard[card.owner];

        Assert.IsNotNull(fieldAndHand);
        Assert.IsNotNull(fieldAndHand.Deck);

        CardCollection dest = fieldAndHand.GetCardCollection(card.location);

        dest.InitialCardPositionAndRotation(card);
        dest.Add(card, false);
    }

    /// <summary>
    /// moves a card to the specified destionation
    /// </summary>
    /// <param name="card">the card to move</param>
    /// <param name="destinationBoard">the player whose board to move to</param>
    /// <param name="destinationLocation">the next location of the card</param>
    internal void MoveCard(CardMeta card, int destinationBoard, CardMeta.Location destinationLocation)
    {
        FieldAndHand sourceBoard = cardBoard[card.owner];
        sourceBoard.Remove(card);

        card.owner = destinationBoard;
        card.location = destinationLocation;

        FieldAndHand destBoard = cardBoard[destinationBoard];
        destBoard.Add(card, destinationLocation, true);


    }
}