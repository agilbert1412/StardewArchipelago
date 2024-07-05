using System.Collections.Generic;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Items.Mail;
using StardewValley;
using StardewValley.TokenizableStrings;

namespace StardewArchipelago.Stardew
{
    public abstract class StardewRecipe
    {
        public string RecipeName { get; protected set; }
        public Dictionary<string, int> Ingredients { get; private set; }
        public StardewItem YieldItem { get; private set; }
        public int YieldItemAmount { get; private set; }
        public string UnlockConditions { get; private set; }
        public string DisplayName { get; private set; }

        protected StardewRecipe(string recipeName, Dictionary<string, int> ingredients, StardewItem yieldItem, int yieldItemAmount, string unlockConditions, string displayName)
        {
            RecipeName = recipeName;
            Ingredients = ingredients;
            YieldItem = yieldItem;
            YieldItemAmount = yieldItemAmount;
            UnlockConditions = unlockConditions;
            DisplayName = displayName;
        }

        public abstract void TeachToFarmer(Farmer farmer);

        public abstract LetterAttachment GetAsLetter(ReceivedItem receivedItem);
    }
}
