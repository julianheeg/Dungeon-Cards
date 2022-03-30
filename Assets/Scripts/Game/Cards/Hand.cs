using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// a script that manages the hand cards of the player
/// </summary>
public class Hand : CardCollection
{
    public override void Add(CardMeta card, bool animate)
    {
        cards.Add(card);
        card.gameObject.transform.SetParent(this.transform);

        cardAddPosition.z += 1.75f/2f;
        Realign();
    }

    /// <summary>
    /// centers the cards (for use after a card has left or has been added to the hand)
    /// </summary>
    private void Realign()
    {
        float padding = CalculatePadding();

        AnimationController.EventAnimation[] eventAnimations = new AnimationController.EventAnimation[cards.Count];
         
        for(int i = 0; i < cards.Count; i++)
        {
            eventAnimations[i] = new AnimationController.EventAnimation(
                cards[i].gameObject,
                gameObject.transform.TransformPoint(Vector3.forward * ((i - (cards.Count - 1) / 2f) * (1 + padding))),
                gameObject.transform.rotation * cardDefaultRotation);
        }

        AnimationController.QueueAnimation(eventAnimations);
    }

    /// <summary>
    /// calculates the distance of space that should be between the cards in the hand
    /// </summary>
    /// <returns>the distance between the cards</returns>
    private float CalculatePadding()
    {
        return 0.75f;
    }
}