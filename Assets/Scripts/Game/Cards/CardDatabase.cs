using Assets.Scripts.Game.Cards.CardTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Type = Assets.Scripts.Game.Cards.CardTypes.Type;

/// <summary>
/// a class that handles the parsing of cards from the card database file
/// </summary>
public static class CardDatabase {

    static Dictionary<int, CardTemplate> cardDicationary;
    static int previouslyParsedCardID;
    static readonly string filename = "Card Database.txt";

    /// <summary>
    /// gets the data of a card from its card id
    /// </summary>
    /// <param name="id">the card id</param>
    /// <returns>the card data</returns>
    public static CardTemplate GetCardData(int id)
    {
        CardTemplate data;
        if (!cardDicationary.TryGetValue(id, out data))
        {
            throw new ArgumentNullException("CardDatabase.GetCardData(id): There is no card with id " + id);
        }
        return data;
    }

    /// <summary>
    /// loads the cards from the database file into the card dictionary
    /// </summary>
    public static void Init()
    {
        cardDicationary = new Dictionary<int, CardTemplate>();
        previouslyParsedCardID = -1;

        try
        {
            using (StreamReader streamReader = new StreamReader(filename))
            {
                while (!streamReader.EndOfStream)
                {
                    String line = streamReader.ReadLine();
                    String[] tokens = line.Split(';');

                    CardTemplate template = CardTemplate.Instantiate(tokens, previouslyParsedCardID);
                    cardDicationary.Add(template.id, template);
                    previouslyParsedCardID = template.id;
                }
            }
        }
        catch (IOException e)
        {
            Debug.LogError("CardDatabase.LoadCards(): The card database file could not be read:\n" + e.Message);
            ErrorMessageDisplayer.DisplayMessage("CardDatabase.LoadCards(): The card database file could not be read.\nMake sure the file \"" + filename + "\" exists within your game folder.", true, false);
        }
        catch (OutOfMemoryException e)
        {
            Debug.LogError("CardDatabase.LoadCards(): The card database file could not be read because of insufficient memory:\n" + e.Message);
            ErrorMessageDisplayer.DisplayMessage("CardDatabase.LoadCards(): Your system ran out of memory while trying to read the card database file.", true, false);
        }
        catch (FormatException e)
        {
            Debug.LogError("CardDatabase.LoadCards(): The card database file is ill-formatted:\n" + e.Message);
            ErrorMessageDisplayer.DisplayMessage("CardDatabase.LoadCards(): The card database file is ill-formatted. Please make sure you have the newest version of the game.\nThe error message in particular:\n" + e.Message, true, false);
        }
    }

    /// <summary>
    /// returns the cards texture
    /// </summary>
    /// <param name="id">the id of the card of which the texture is requested</param>
    /// <returns>the texture or null, if there is no texture</returns>
    public static Texture LoadTexture(int id)
    {
        return null;
    }

    /// <summary>
    /// turns a card id into a card
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static T IDToTemplate<T>(int id) where T : CardTemplate
    {
        CardTemplate template;
        if (cardDicationary.TryGetValue(id, out template))
        {
            return (T)template;
        }
        else
        {
            Console.WriteLine(id);
            throw new ArgumentNullException("CardDatabase.GetCardData(id): There is no card with id " + id);
        }
    }
}