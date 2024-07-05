using System.Collections.Generic;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Items.Mail;
using StardewValley;

namespace StardewArchipelago.Stardew
{
    public class StardewCraftingRecipe : StardewRecipe
    {
        public string BigCraftable { get; private set; }

        public StardewCraftingRecipe(string recipeName, Dictionary<string, int> ingredients, StardewItem yieldItem, int yieldItemAmount, string bigCraftable, string unlockConditions, string displayName) : base(recipeName, ingredients, yieldItem, yieldItemAmount, unlockConditions, displayName)
        {
            BigCraftable = bigCraftable;
        }

        public override void TeachToFarmer(Farmer farmer)
        {
            farmer.craftingRecipes.Add(RecipeName, 0);
        }

        public override LetterAttachment GetAsLetter(ReceivedItem receivedItem)
        {
            return new LetterCraftingRecipeAttachment(receivedItem, RecipeName);
        }
    }
}
