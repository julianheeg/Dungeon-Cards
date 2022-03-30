using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// represents the card field of a player
/// </summary>
public class Field : CardCollection
{
    public override void Add(CardMeta card, bool animate)
    {
        cards.Add(card);
        card.gameObject.SetActive(false);
    }

    public override void Remove(CardMeta card)
    {
        base.Remove(card);
        card.gameObject.SetActive(true);
    }
}
