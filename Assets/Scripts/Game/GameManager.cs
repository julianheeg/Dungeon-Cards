using Assets.Scripts.Game.Cards.CardTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// a script that handles the game flow
/// </summary>
public class GameManager : MonoBehaviour {

    static GameManager instance;
    public static int NumberOfPlayers { get; private set; }

    int turnPlayer;
    int TurnPlayer
    {
        get
        {
            return turnPlayer;
        }
        set
        {
            Debug.Log("changing turn to " + value);
            turnPlayer = value;
            //TODO play animation or so
        }
    }

    private void Awake()
    {
        DontDestroyOnLoad(this);
        instance = this;
    }

    /// <summary>
    /// resets all fields and calls all the other singletons to clear their fields. Then they are initialized with some serialized meta data
    /// </summary>
    /// <param name="serializedMetaData">the meta data</param>
    public static void SetMetaData(byte[] serializedMetaData)
    {
        //deserialize map meta data and pass it to map manager
        int index = 2;
        int length = BitConverter.ToInt32(Endianness.FromBigEndian(serializedMetaData, index), index);
        index += 4;
        int width = BitConverter.ToInt32(Endianness.FromBigEndian(serializedMetaData, index), index);
        index += 4;
        bool hexagonal = BitConverter.ToBoolean(serializedMetaData, index);
        index++;

        //deserialize card meta data and pass it to card manager, set own player index
        Player.ownIndex = -1;
        int numberOfPlayers = (serializedMetaData.Length - index) / 8;
        if (!Config.allowedNumbersOfPlayers.Contains(numberOfPlayers)) //verify correct number of players
        {
            throw new ArgumentException();
        }
        NumberOfPlayers = numberOfPlayers;

        //this is needed here because the FieldAndHandClass that is updated by Mapmanager.Init() uses the numbersOfPlayers property
        MapManager.Init(length, width, hexagonal);


        int[] deckSizes = new int[numberOfPlayers];
        for (int i = 0; i < numberOfPlayers; i++)
        { 
            int playerID = BitConverter.ToInt32(Endianness.FromBigEndian(serializedMetaData, index), index);
            index += 4;

            if (playerID == Player.own.id)
            {
                Player.ownIndex = i;
            }
            int deckSize = BitConverter.ToInt32(Endianness.FromBigEndian(serializedMetaData, index), index);
            index += 4;
            deckSizes[i] = deckSize;
        }
        CardManager.Init(deckSizes);
    }

    private enum ClientGameStateChange { TurnChange, CardMovement, MonsterSpawn }

    /// <summary>
    /// parses a message when the first byte of it has been clientTopLevel.GameState
    /// </summary>
    /// <param name="data">the data to parse</param>
    public static void ParseGameStateLevel(byte[] data)
    {
        switch ((ClientGameStateChange)data[1])
        {
            case ClientGameStateChange.TurnChange:
                if (data.Length == 6)
                {
                    int turnPlayer = BitConverter.ToInt32(Endianness.FromBigEndian(data, 2), 2);
                    instance.TurnPlayer = turnPlayer;
                }
                else
                {
                    Debug.LogError("Parse error on Client GameState change. Second byte was TurnChange, but received " + data.Length + " bytes (expected 6)");
                }
                break;

            case ClientGameStateChange.CardMovement:
                if (data.Length == 11)
                {
                    int cardInstanceID = BitConverter.ToInt32(Endianness.FromBigEndian(data, 2), 2);
                    int destinationBoard = BitConverter.ToInt32(Endianness.FromBigEndian(data, 6), 6);
                    CardMeta.Location destinationLocation = (CardMeta.Location)data[10];

                    CardManager.MoveCard(cardInstanceID, destinationBoard, destinationLocation);
                }
                else
                {
                    Debug.LogError("Parse error on Client GameState change. Second byte was CardMovement, but received " + data.Length + " bytes (expected 11)");
                }
                break;

            case ClientGameStateChange.MonsterSpawn:
                if (data.Length == 18)
                {
                    int cardInstanceID = BitConverter.ToInt32(Endianness.FromBigEndian(data, 2), 2);
                    CardManager.MoveCard(cardInstanceID, -1, CardMeta.Location.Field); //update card manager

                    //get values for monster spawn
                    CardMeta card = CardManager.InstanceIDToCard(cardInstanceID);
                    Assert.IsTrue(card.face is MonsterCard);

                    int monsterInstanceID = BitConverter.ToInt32(Endianness.FromBigEndian(data, 6), 6);
                    int index = 10;
                    Cell cell = new Cell(data, ref index);

                    //spawn monster
                    MonsterManager.SpawnMonster((MonsterCard)card.face, monsterInstanceID, cell);
                }
                else
                {
                    Debug.LogError("Parse error on Client GameState change. Second byte was MonsterSpawn, but received " + data.Length + " bytes (expected 14)");
                }
                break;

            default:
                Debug.LogError("Parse error on Client Gamestate Level. Second byte was " + data[1]);
                break;
        }
    }


}