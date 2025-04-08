﻿using System;
using System.Collections.Generic;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.SlotData;
using StardewArchipelago.Constants.Modded;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace StardewArchipelago.GameModifications
{
    public class RecipeDataRemover
    {
        private ILogger _logger;
        private IModHelper _helper;
        private readonly StardewArchipelagoClient _archipelago;

        public RecipeDataRemover(ILogger logger, IModHelper helper, StardewArchipelagoClient archipelago)
        {
            _logger = logger;
            _helper = helper;
            _archipelago = archipelago;
        }

        public void OnCookingRecipesRequested(object sender, AssetRequestedEventArgs e)
        {
            if (_archipelago.SlotData.Chefsanity == Chefsanity.Vanilla)
            {
                return;
            }

            if (!e.NameWithoutLocale.IsEquivalentTo("Data/CookingRecipes"))
            {
                return;
            }

            e.Edit(asset =>
                {
                    var cookingRecipesData = asset.AsDictionary<string, string>().Data;
                    RemoveObsoleteCookingLearnConditions(cookingRecipesData);
                },
                AssetEditPriority.Late
            );
        }

        public void OnCraftingRecipesRequested(object sender, AssetRequestedEventArgs e)
        {
            if (_archipelago.SlotData.Craftsanity == Craftsanity.None)
            {
                return;
            }

            if (!e.NameWithoutLocale.IsEquivalentTo("Data/CraftingRecipes"))
            {
                return;
            }

            e.Edit(asset =>
                {
                    var craftingRecipesData = asset.AsDictionary<string, string>().Data;
                    RemoveObsoleteCraftingLearnConditions(craftingRecipesData);
                },
                AssetEditPriority.Late
            );
        }

        private void RemoveObsoleteCookingLearnConditions(IDictionary<string, string> cookingRecipesData)
        {
            RemoveCookingRecipesStarterLearnConditions(cookingRecipesData);
            RemoveCookingRecipesSkillsLearnConditions(cookingRecipesData);
            RemoveCookingRecipesFriendshipLearnConditions(cookingRecipesData);
        }

        private void RemoveObsoleteCraftingLearnConditions(IDictionary<string, string> craftingRecipesData)
        {
            RemoveCraftingRecipesStarterLearnConditions(craftingRecipesData);
            RemoveCraftingRecipesSkillsLearnConditions(craftingRecipesData);
            RemoveCraftingRecipesFriendshipLearnConditions(craftingRecipesData);
        }

        private void RemoveCookingRecipesStarterLearnConditions(IDictionary<string, string> cookingRecipesData)
        {
            if (_archipelago.SlotData.Chefsanity == Chefsanity.Vanilla)
            {
                return;
            }

            foreach (var recipeName in cookingRecipesData.Keys.ToArray())
            {
                var recipeData = cookingRecipesData[recipeName];
                var recipeUnlockCondition = GetCookingRecipeUnlockCondition(recipeData);
                if (!recipeUnlockCondition.Equals("default", StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                var modifiedRecipe = recipeData.Replace(recipeUnlockCondition, "none");
                cookingRecipesData[recipeName] = modifiedRecipe;
                SynchronizeCookingRecipeState(recipeName);
            }
        }
        private void SynchronizeCookingRecipeState(string recipeName)
        {
            if (!_archipelago.MakeSureConnected())
            {
                return;
            }

            foreach (var farmer in Game1.getAllFarmers())
            {
                if (farmer == null) continue;
                if (farmer.cookingRecipes.ContainsKey(recipeName) && !_archipelago.HasReceivedItem($"{recipeName} Recipe"))
                {
                    farmer.cookingRecipes.Remove(recipeName);
                }
            }
        }

        private void RemoveCookingRecipesSkillsLearnConditions(IDictionary<string, string> cookingRecipesData)
        {
            if (!_archipelago.SlotData.Chefsanity.HasFlag(Chefsanity.Skills))
            {
                return;
            }

            foreach (var recipeName in cookingRecipesData.Keys.ToArray())
            {
                var recipeData = cookingRecipesData[recipeName];
                var recipeUnlockCondition = GetCookingRecipeUnlockCondition(recipeData);
                var unlockConditionParts = recipeUnlockCondition.Split(" ");
                var isVanillaSkillUnlock = unlockConditionParts.Length >= 3 && unlockConditionParts[0] == "s";
                var isBirbCoreSkillUnlock = IsBirbCoreSkillUnlock(unlockConditionParts);
                if (!isVanillaSkillUnlock && !isBirbCoreSkillUnlock)
                {
                    continue;
                }

                var modifiedRecipe = recipeData.Replace(recipeUnlockCondition, "none");
                cookingRecipesData[recipeName] = modifiedRecipe;
                SynchronizeSkillsCookingRecipesState(recipeName);
            }
        }
        private void SynchronizeSkillsCookingRecipesState(string recipeName)
        {
            if (!_archipelago.MakeSureConnected())
            {
                return;
            }

            var recipeKey = $"{recipeName} Recipe";
            var knowsRecipe = Game1.player.cookingRecipes.ContainsKey(recipeName);
            var shouldKnowRecipe = _archipelago.HasReceivedItem(recipeKey);
            if (knowsRecipe == shouldKnowRecipe)
            {
                return;
            }

            if (knowsRecipe)
            {
                Game1.player.cookingRecipes.Remove(recipeName);
            }
            else
            {
                Game1.player.cookingRecipes.Add(recipeName, 0);
            }
        }

        private static bool IsBirbCoreSkillUnlock(string[] unlockConditionParts)
        {
            var isBirbCoreSkillUnlock = unlockConditionParts.Length == 2 &&
                                        ArchipelagoSkillIds.ModdedSkillIds.Any(x =>
                                            unlockConditionParts[0].Equals($"{x}Skill", StringComparison.InvariantCultureIgnoreCase) ||
                                            unlockConditionParts[0].Equals(x, StringComparison.InvariantCultureIgnoreCase)) &&
                                        int.TryParse(unlockConditionParts[1], out _);
            return isBirbCoreSkillUnlock;
        }

        private void RemoveCookingRecipesFriendshipLearnConditions(IDictionary<string, string> cookingRecipesData)
        {
            if (!_archipelago.SlotData.Chefsanity.HasFlag(Chefsanity.Friendship))
            {
                return;
            }

            foreach (var recipeName in cookingRecipesData.Keys.ToArray())
            {
                var recipeData = cookingRecipesData[recipeName];
                var recipeUnlockCondition = GetCookingRecipeUnlockCondition(recipeData);
                var unlockConditionParts = recipeUnlockCondition.Split(" ");
                if (unlockConditionParts.Length < 3 || unlockConditionParts[0] != "f")
                {
                    continue;
                }

                var modifiedRecipe = recipeData.Replace(recipeUnlockCondition, $"none:{recipeUnlockCondition}");
                cookingRecipesData[recipeName] = modifiedRecipe;
                var npcName = unlockConditionParts[1];
                Game1.player?.RemoveMail($"{npcName}Cooking", true);
            }
        }

        private void RemoveCraftingRecipesStarterLearnConditions(IDictionary<string, string> craftingRecipesData)
        {
            if (_archipelago.SlotData.Craftsanity == Craftsanity.None)
            {
                return;
            }

            foreach (var recipeName in craftingRecipesData.Keys.ToArray())
            {
                var recipeData = craftingRecipesData[recipeName];
                var recipeUnlockCondition = GetCraftingRecipeUnlockCondition(recipeData);

                if (!recipeUnlockCondition.Equals("default", StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                var modifiedRecipe = recipeData.Replace(recipeUnlockCondition, "none");
                craftingRecipesData[recipeName] = modifiedRecipe;

                SynchronizeCraftingRecipesState(recipeName);
            }
        }

        private void SynchronizeCraftingRecipesState(string recipeName)
        {
            if (!_archipelago.MakeSureConnected())
            {
                return;
            }

            foreach (var farmer in Game1.getAllFarmers())
            {
                if (farmer == null) continue;
                var knowsRecipe = farmer.craftingRecipes.ContainsKey(recipeName);
                var shouldKnowRecipe = _archipelago.HasReceivedItem($"{recipeName} Recipe");
                if (knowsRecipe && !shouldKnowRecipe)
                {
                    farmer.craftingRecipes.Remove(recipeName);
                }
                else if (!knowsRecipe && shouldKnowRecipe && !farmer.mailForTomorrow.Any(x => x.Contains(recipeName)) && (!farmer.mailbox.Any(x => x.Contains(recipeName))))
                {
                    farmer.craftingRecipes.Add(recipeName, 0);
                }
            }
        }

        private void RemoveCraftingRecipesSkillsLearnConditions(IDictionary<string, string> craftingRecipesData)
        {
            if (_archipelago.SlotData.Craftsanity == Craftsanity.None)
            {
                return;
            }

            // This is not necessary, we let players learn recipes through level ups
        }

        private void RemoveCraftingRecipesFriendshipLearnConditions(IDictionary<string, string> craftingRecipesData)
        {
            if (_archipelago.SlotData.Craftsanity == Craftsanity.None)
            {
                return;
            }

            // This is not necessary, we let players learn recipes through friendships
        }

        private static string GetCookingRecipeUnlockCondition(string recipeData)
        {
            return GetRecipeUnlockCondition(recipeData, 3);
        }

        private static string GetCraftingRecipeUnlockCondition(string recipeData)
        {
            return GetRecipeUnlockCondition(recipeData, 4);
        }

        private static string GetRecipeUnlockCondition(string recipeData, int fieldIndex)
        {
            var recipeFields = recipeData.Split("/");
            var recipeUnlockCondition = recipeFields[fieldIndex];
            return recipeUnlockCondition;
        }
    }
}
