using System.Collections.Generic;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Items.Mail;
using StardewValley;

namespace StardewArchipelago.Stardew
{
    public abstract class StardewRecipe
    {
        public string RecipeName { get; protected set; }
        public Dictionary<string, int> Ingredients { get; private set; }
        public string YieldItemId { get; private set; }
        public int YieldItemAmount { get; private set; }
        public string UnlockConditions { get; private set; }
        public string DisplayName { get; private set; }

        protected StardewRecipe(string recipeName, Dictionary<string, int> ingredients, string yieldItemId, int yieldItemAmount, string unlockConditions, string displayName)
        {
            Ingredients = ingredients;
            YieldItemId = yieldItemId;
            YieldItemAmount = yieldItemAmount;
            UnlockConditions = unlockConditions;
            DisplayName = displayName;

            // This uses many fields
            RecipeName = GetRecipeKey(recipeName);
        }

        public abstract void TeachToFarmer(Farmer farmer);

        public abstract LetterAttachment GetAsLetter(ReceivedItem receivedItem);

        private string GetRecipeKey(string recipeName)
        {
            if (!string.IsNullOrWhiteSpace(DisplayName))
            {
                return DisplayName;
            }

            //if (!string.IsNullOrWhiteSpace(YieldItemId))
            //{
            //    var yieldItem = _objectsById[recipe.YieldItemId];
            //    if (yieldItem?.Name != null)
            //    {
            //        return yieldItem.Name;
            //    }
            //}

            if (!string.IsNullOrWhiteSpace(RecipeName))
            {
                return RecipeName;
            }

            return recipeName;
        }
    }
}
