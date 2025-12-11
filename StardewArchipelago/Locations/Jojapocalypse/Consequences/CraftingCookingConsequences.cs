using System;
using System.Collections.Generic;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.Utilities.Interfaces;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.ApworldData;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Inventories;
using StardewValley.Menus;

namespace StardewArchipelago.Locations.Jojapocalypse.Consequences
{
    internal class CraftingCookingConsequences
    {
        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static StardewArchipelagoClient _archipelago;
        private static JojaLocationChecker _jojaLocationChecker;

        public static void Initialize(ILogger logger, IModHelper modHelper, StardewArchipelagoClient archipelago, JojaLocationChecker jojaLocationChecker)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _jojaLocationChecker = jojaLocationChecker;
        }

        // private void clickCraftingRecipe(ClickableTextureComponent c, bool playSound = true)
        public static bool ClickCraftingRecipe_ChanceOfFailingCraft_Prefix(CraftingPage __instance, ClickableTextureComponent c, bool playSound)
        {
            try
            {
                var recipe = __instance.pagesOfCraftingRecipes[__instance.currentCraftingPage][c];
                if (__instance.cooking)
                {
                    return ClickCookingRecipePrefix(__instance, recipe, playSound);
                }
                else
                {
                    return ClickCraftingRecipePrefix(__instance, recipe, playSound);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(ClickCraftingRecipe_ChanceOfFailingCraft_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static bool ClickCookingRecipePrefix(CraftingPage craftingPage, CraftingRecipe recipe, bool playSound)
        {
            try
            {
                var numberCooksanityPurchased = _jojaLocationChecker.CountCheckedLocationsWithTag(LocationTag.COOKSANITY);
                var numberChefsanityPurchased = _jojaLocationChecker.CountCheckedLocationsWithTag(LocationTag.CHEFSANITY);
                if (numberCooksanityPurchased <= 0 && numberChefsanityPurchased <= 0)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (JojapocalypseConsequencesPatcher.RollConsequenceChance(0.04, numberChefsanityPurchased, Game1.ticks))
                {
                    ConsumeRandomUnrelatedIngredient(craftingPage, recipe);
                }

                if (JojapocalypseConsequencesPatcher.RollConsequenceChance(0.01, numberCooksanityPurchased, Game1.ticks))
                {
                    var extraIngredientsToUse = new List<KeyValuePair<string, int>>();
                    extraIngredientsToUse.Add(new KeyValuePair<string, int>("917", 1));
                    if (!CraftingRecipe.DoesFarmerHaveAdditionalIngredientsInInventory(extraIngredientsToUse, GetContainerContents(craftingPage)))
                    {
                        extraIngredientsToUse = null;
                    }
                    recipe.consumeIngredients(craftingPage._materialContainers);
                    if (playSound)
                    {
                        Game1.playSound("coin");
                    }
                    if (extraIngredientsToUse != null)
                    {
                        if (playSound)
                        {
                            Game1.playSound("breathin");
                        }
                        CraftingRecipe.ConsumeAdditionalIngredients(extraIngredientsToUse, craftingPage._materialContainers);
                        if (!CraftingRecipe.DoesFarmerHaveAdditionalIngredientsInInventory(extraIngredientsToUse, GetContainerContents(craftingPage)))
                        {
                            Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Seasoning_UsedLast"));
                        }
                    }

                    Game1.chatBox.addMessage($"You burned this recipe... You must be out of practice.", Color.Red);
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(ClickCookingRecipePrefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static bool ClickCraftingRecipePrefix(CraftingPage craftingPage, CraftingRecipe recipe, bool playSound)
        {
            try
            {
                var numberCraftsanityCraftPurchased = _jojaLocationChecker.CountCheckedLocationsWithTag(LocationTag.CRAFTSANITY_CRAFT); // Total 150
                var numberCraftsanityRecipePurchased = _jojaLocationChecker.CountCheckedLocationsWithTag(LocationTag.CRAFTSANITY_RECIPE); // Total 30
                if (numberCraftsanityCraftPurchased <= 0 && numberCraftsanityRecipePurchased <= 0)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (JojapocalypseConsequencesPatcher.RollConsequenceChance(0.04, numberCraftsanityRecipePurchased, Game1.ticks))
                {
                    ConsumeRandomUnrelatedIngredient(craftingPage, recipe);
                }

                if (JojapocalypseConsequencesPatcher.RollConsequenceChance(0.008, numberCraftsanityCraftPurchased, Game1.ticks / 2))
                {
                    recipe.consumeIngredients(craftingPage._materialContainers);
                    if (playSound)
                    {
                        Game1.playSound("coin");
                    }

                    Game1.chatBox.addMessage($"You broke this {recipe.DisplayName}... You must be out of practice.", Color.Red);
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(ClickCraftingRecipePrefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static void ConsumeRandomUnrelatedIngredient(CraftingPage craftingPage, CraftingRecipe recipe)
        {
            var recipeIngredients = new HashSet<string>();
            recipeIngredients.AddRange(recipe.recipeList.Select(x => x.Key));

            var candidateIngredients = new List<Tuple<IInventory, Item>>();
            candidateIngredients.AddRange(Game1.player.Items.Where(x => x != null && x.Stack > 1 && !recipeIngredients.Contains(x.ItemId)).Select(x => new Tuple<IInventory, Item>(Game1.player.Items, x)));
            if (craftingPage._materialContainers != null)
            {
                foreach (var materialContainer in craftingPage._materialContainers)
                {
                    candidateIngredients.AddRange(materialContainer.Where(x => x != null && x.Stack > 1 && !recipeIngredients.Contains(x.ItemId)).Select(x => new Tuple<IInventory, Item>(materialContainer, x)));
                }
            }

            if (candidateIngredients.Count <= 0)
            {
                return;
            }

            var random = new Random();
            var index = random.Next(candidateIngredients.Count);
            var ingredientToConsume = candidateIngredients[index];
            ingredientToConsume.Item1.Reduce(ingredientToConsume.Item2, 1);
            Game1.chatBox.addMessage($"You misremembered the recipe and accidentally used {ingredientToConsume.Item2.Name}...", Color.Red);
        }

        private static IList<Item> GetContainerContents(CraftingPage craftingPage)
        {
            if (craftingPage._materialContainers == null)
            {
                return null;
            }
            var containerContents = new List<Item>();
            foreach (var materialContainer in craftingPage._materialContainers)
            {
                containerContents.AddRange(materialContainer);
            }
            return containerContents;
        }
    }
}
