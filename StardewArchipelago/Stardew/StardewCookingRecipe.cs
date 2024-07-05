using System.Collections.Generic;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Items.Mail;
using StardewValley;

namespace StardewArchipelago.Stardew
{
    public class StardewCookingRecipe : StardewRecipe
    {
        public StardewCookingRecipe(string recipeName, Dictionary<string, int> ingredients, string yieldItemId, int yieldItemAmount, string unlockConditions, string displayName) : base(recipeName, ingredients, yieldItemId, yieldItemAmount, unlockConditions, displayName)
        {
        }

        public override void TeachToFarmer(Farmer farmer)
        {
            farmer.cookingRecipes.Add(RecipeName, 0);
        }

        public override LetterAttachment GetAsLetter(ReceivedItem receivedItem)
        {
            return new LetterCookingRecipeAttachment(receivedItem, RecipeName);
        }
    }
}
