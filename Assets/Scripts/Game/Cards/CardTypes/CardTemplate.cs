using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Type = Assets.Scripts.Game.Cards.CardTypes.Type;

namespace Assets.Scripts.Game.Cards.CardTypes
{
    public class CardTemplate
    {
        public readonly int id;
        public readonly int cost;
        public readonly string cardName;
        public readonly string cardDescription;
        public readonly Type type;

        const int minTokens = 5;

        public static CardTemplate Instantiate(string[] tokens, int previouslyParsedCardID)
        {
            CheckMinTokens(tokens, previouslyParsedCardID);
            int tokenCounter = 0;

            //parse card id (parsed first for error handling) and card type (for switch to the constructor)
            int id = ParseCardID(tokens, ref tokenCounter, previouslyParsedCardID);
            Type type = ParseCardType(tokens, ref tokenCounter, id);

            //instantiate template
            CardTemplate template;
            switch (type)
            {
                case Type.Monster:
                    template = new MonsterCardTemplate(id, tokens, ref tokenCounter);
                    break;
                default:
                    throw new NotImplementedException();
            }

            //check parse and return
            CheckCorrectParse(tokens, tokenCounter, template.id);
            return template;
        }

        #region parsing

        private static void CheckMinTokens(string[] tokens, int previouslyParsedCardID)
        {
            if (tokens.Length < minTokens)
            {
                throw new FormatException("CardTemplate.ctor(): The card following the card with id " + previouslyParsedCardID + " has too few tokens (expected: at least " + minTokens + ")");
            }
        }

        private static int ParseCardID(string[] tokens, ref int tokenCounter, int previouslyParsedCardID)
        {
            int id;
            if (!Int32.TryParse(tokens[tokenCounter], out id))
            {
                throw new FormatException("CardTemplate.ctor(): In the line after card id " + previouslyParsedCardID + ": The string \"" + tokens[tokenCounter] + "\" is not an int (expected: valid card id)");
            }
            tokenCounter++;
            return id;
        }

        private static Type ParseCardType(string[] tokens, ref int tokenCounter, int id)
        {
            int type;
            if (!Int32.TryParse(tokens[tokenCounter], out type) || !Enum.IsDefined(typeof(Type), type))
            {
                throw new FormatException("CardTemplate.ctor(): The card with id " + id + " has \"" + tokens[tokenCounter] + "\" as type (expected: int between 0 and " + (Enum.GetNames(typeof(Type)).Length - 1) + ")");
            }
            tokenCounter++;
            return (Type)type;
        }

        private static void CheckCorrectParse(string[] tokens, int tokenCounter, int id)
        {
            //check if all tokens have been parsed
            if (tokenCounter != tokens.Length)
            {
                throw new FormatException("CardTemplate.CheckCorrectParse(): The card with id " + id + " has too many tokens (expected: " + tokenCounter + ")");
            }
        }

        protected CardTemplate(int id, string[] tokens, ref int tokenCounter)
        {
            this.id = id;

            #region parse title, description, cost

            //parse title
            string title = tokens[tokenCounter];
            if (title == "")
            {
                throw new FormatException("CardTemplate.ctor(): The card with id " + id + " has no name");
            }
            this.cardName = title;
            tokenCounter++;

            //parse description
            string description = tokens[tokenCounter];
            if (description == "")
            {
                throw new FormatException("CardTemplate.ctor(): The card with id " + id + " has no description");
            }
            this.cardDescription = description;
            tokenCounter++;

            //parse cost
            if (!Int32.TryParse(tokens[tokenCounter], out cost) || cost < 0)
            {
                throw new FormatException("CardTemplate.ctor(): The card with id " + id + " has \"" + tokens[tokenCounter] + "\" as cost (expected: valid cost)");
            }
            tokenCounter++;

            #endregion
        }

        #endregion

        //to string methods
        public override string ToString() { return "[id: " + id + ", cost: " + cost + ", type: " + type + GetAdditionalToStringAttributes() + "]"; }
        protected virtual string GetAdditionalToStringAttributes() { return ""; }
    }

    public class MonsterCardTemplate : CardTemplate
    {
        public readonly int health;
        public readonly int damage;
        public readonly int movementRange;

        public MonsterCardTemplate(int id, string[] tokens, ref int tokenCounter) : base(id, tokens, ref tokenCounter)
        {
            #region parse health, damage

            //parse health
            if (!Int32.TryParse(tokens[tokenCounter], out health) || health < 0)
            {
                throw new FormatException("MonsterCardTemplate.ctor(): The card with id " + id + " has \"" + tokens[tokenCounter] + "\" as health (expected: valid health value)");
            }
            tokenCounter++;

            //parse damage
            if (!Int32.TryParse(tokens[tokenCounter], out damage) || damage < 0)
            {
                throw new FormatException("MonsterCardTemplate.ctor(): The card with id " + id + " has \"" + tokens[tokenCounter] + "\" as damage (expected: valid damage value)");
            }
            tokenCounter++;

            //parse movement range
            if (!Int32.TryParse(tokens[tokenCounter], out movementRange) || movementRange < 0)
            {
                throw new FormatException("MonsterCardTemplate.ctor(): The card with id " + id + " has \"" + tokens[tokenCounter] + "\" as movement range (expected: valid damage value)");
            }
            tokenCounter++;

            #endregion
        }

        protected override string GetAdditionalToStringAttributes() { return ", health: " + health + ", damage: " + damage; }
    }
}
