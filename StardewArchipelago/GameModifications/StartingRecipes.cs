using Netcode;
using StardewArchipelago.Archipelago;
using StardewValley;
using StardewValley.Network;

namespace StardewArchipelago.GameModifications
{
    public class StartingRecipes
    {
        private static ArchipelagoClient _archipelago;

        public StartingRecipes(ArchipelagoClient archipelago)
        {
            _archipelago = archipelago;
        }

        public void SynchronizeStartingRecipes(Farmer farmer)
        {
            SynchronizeStartingCraftingRecipesWithArchipelago(farmer);
            SynchronizeStartingCookingRecipesWithArchipelago(farmer);
        }

        private void SynchronizeStartingCraftingRecipesWithArchipelago(Farmer __instance)
        {
            if (_archipelago.SlotData.Craftsanity == Craftsanity.None)
            {
                return;
            }
            var startingCraftingRecipes = new[]
            {
                "Chest",
                "Wood Fence",
                "Gate",
                "Torch",
                "Campfire",
                "Wood Path",
                "Cobblestone Path",
                "Gravel Path",
                "Wood Sign",
                "Stone Sign",
            };
            SynchronizeStartingRecipesWithArchipelago(startingCraftingRecipes, __instance.craftingRecipes);
        }

        private void SynchronizeStartingCookingRecipesWithArchipelago(Farmer __instance)
        {
            if (_archipelago.SlotData.Chefsanity == Chefsanity.Vanilla)
            {
                return;
            }
            var startingCookingRecipes = new[]
            {
                "Fried Egg",
            };
            SynchronizeStartingRecipesWithArchipelago(startingCookingRecipes, __instance.cookingRecipes);
        }

        private void SynchronizeStartingRecipesWithArchipelago(string[] startingRecipes, NetStringDictionary<int, NetInt> knownRecipes)
        {
            foreach (var startingRecipe in startingRecipes)
            {
                SynchronizeStartingRecipeWithArchipelago(knownRecipes, startingRecipe);
            }
        }

        private void SynchronizeStartingRecipeWithArchipelago(NetStringDictionary<int, NetInt> knownRecipes, string startingRecipe)
        {
            var knowsRecipe = knownRecipes.ContainsKey(startingRecipe);
            var shouldKnowRecipe = _archipelago.HasReceivedItem($"{startingRecipe} Recipe");
            if (knowsRecipe == shouldKnowRecipe)
            {
                return;
            }

            if (shouldKnowRecipe)
            {
                knownRecipes.Add(startingRecipe, 0);
            }
            else
            {
                knownRecipes.Remove(startingRecipe);
            }
        }
    }
}
