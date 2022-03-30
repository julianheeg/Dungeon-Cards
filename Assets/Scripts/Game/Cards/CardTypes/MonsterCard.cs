using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Game.Cards.CardTypes
{
    public class MonsterCard : CardFace
    {
        public int health;
        public int damage;
        public int movementRange;

        protected override void InitCardTypeSpecific(CardTemplate template)
        {
            health = ((MonsterCardTemplate)template).health;
            damage = ((MonsterCardTemplate)template).damage;
            movementRange = ((MonsterCardTemplate)template).movementRange;
        }

        protected override string AdditionalToStringAttributes()
        {
            return ", health: " + health + ", damage: " + damage;
        }
    }
}
