using System.Collections.Generic;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Items.Mail;
using StardewValley;
using StardewValley.Objects;

namespace StardewArchipelago.Stardew
{
    public class StardewCraftingRecipe : StardewRecipe
    {
        public bool BigCraftable { get; private set; }

        public StardewCraftingRecipe(string itemName, Dictionary<int, int> ingredients, int itemId, int itemAmount, bool bigCraftable, string unlockConditions, string displayName) : base(itemName, ingredients, itemId, itemAmount, unlockConditions, displayName)
        {
            BigCraftable = bigCraftable;
        }

        public override void TeachToFarmer(Farmer farmer)
        {
            farmer.craftingRecipes.Add(ItemName, 0);
        }

        public override LetterAttachment GetAsLetter(ReceivedItem receivedItem)
        {
            return new LetterCraftingRecipeAttachment(receivedItem, ItemName);
        }
    }
}
