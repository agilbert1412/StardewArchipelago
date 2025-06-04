using System;
using System.Collections.Generic;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.ApworldData;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.Extensions;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Inventories;
using StardewValley.Menus;
using StardewValley.Tools;
using Object = StardewValley.Object;

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
                var numberPurchased = _jojaLocationChecker.CountCheckedLocationsWithTag(LocationTag.COOKSANITY);
                if (numberPurchased <= 0)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (JojapocalypseConsequencesPatcher.RollConsequenceChance(0.01, numberPurchased, Game1.ticks))
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
                _logger.LogError($"Failed in {nameof(ClickCraftingRecipePrefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
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

        private static bool ClickCraftingRecipePrefix(CraftingPage craftingPage, CraftingRecipe recipe, bool playSound)
        {
            try
            {
                var numberPurchased = _jojaLocationChecker.CountCheckedLocationsWithTag(LocationTag.CRAFTSANITY);
                if (numberPurchased <= 0)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (JojapocalypseConsequencesPatcher.RollConsequenceChance(0.01, numberPurchased, Game1.ticks))
                {
                    recipe.consumeIngredients(craftingPage._materialContainers);
                    if (playSound)
                    {
                        Game1.playSound("coin");
                    }

                    Game1.chatBox.addMessage($"You broke this recipe... You must be out of practice.", Color.Red);
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
    }
}
