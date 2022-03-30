using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// represents a card pile (deck or graveyard)
/// </summary>
public class CardPile : CardCollection {
    
    /// <summary>
    /// the logic to execute for when a card is added
    /// sets the parent, adds the card to the pile and starts rendering the movement
    /// </summary>
    /// <param name="card"></param>
    public override void Add(CardMeta card, bool animate)
    {
        cards.Add(card);
        card.gameObject.transform.SetParent(this.transform);
        if (animate)
        {
            AnimationController.QueueAnimation(new AnimationController.EventAnimation(card.gameObject, gameObject.transform.TransformPoint(cardAddPosition), gameObject.transform.rotation * cardDefaultRotation));
        }
        else
        {
            card.transform.localPosition = cardAddPosition;
            card.transform.localRotation = cardDefaultRotation;
        }

        cardAddPosition.y += Config.cardThickness;
    }
}