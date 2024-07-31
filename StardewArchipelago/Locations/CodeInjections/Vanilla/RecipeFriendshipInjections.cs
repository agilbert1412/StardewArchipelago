﻿using System;
using System.Collections.Generic;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using StardewArchipelago.Constants.Locations;
using StardewArchipelago.Constants.Vanilla;
using StardewModdingAPI;
using StardewValley;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public static class RecipeFriendshipInjections
    {
        private static ILogger _logger;
        private static IModHelper _helper;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;
        private static Dictionary<string, string> _allRecipes;

        public static void Initialize(ILogger logger, IModHelper helper, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _helper = helper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;

            _allRecipes = DataLoader.CookingRecipes(Game1.content);
        }

        // public void grantConversationFriendship(Farmer who, int amount = 20)
        public static void GrantConversationFriendship_SendFriendshipRecipeChecks_Postfix(NPC __instance, Farmer who, int amount = 20)
        {
            try
            {
                if (!who.friendshipData.ContainsKey(__instance.Name))
                {
                    return;
                }

                var friendship = who.friendshipData[__instance.Name];
                var currentHearts = friendship.Points / 250;
                CheckCookingRecipeLocations(__instance.Name, currentHearts);
                CheckCraftingRecipeLocations(__instance.Name, currentHearts);

                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(GrantConversationFriendship_SendFriendshipRecipeChecks_Postfix)}:\n{ex}");
                return;
            }
        }

        private static void CheckCookingRecipeLocations(string friendName, int currentHearts)
        {
            if (!_archipelago.SlotData.Chefsanity.HasFlag(Chefsanity.Friendship))
            {
                return;
            }

            foreach (var (recipeName, recipeData) in _allRecipes)
            {
                var recipeFields = recipeData.Split("/");
                if (recipeFields.Length < 4)
                {
                    continue;
                }

                var unlockConditions = recipeFields[3];
                var unlockConditionFields = unlockConditions.Split(":").Last().Split(" ");
                if (unlockConditionFields.Length != 3 ||
                    !unlockConditionFields[0].Equals("f", StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                var recipeFriendName = unlockConditionFields[1];
                if (!recipeFriendName.Equals(friendName, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                var hearts = int.Parse(unlockConditionFields[2]);
                if (hearts > currentHearts)
                {
                    continue;
                }

                var aliasedRecipeName = GetAliased(recipeName);

                _locationChecker.AddCheckedLocation($"{aliasedRecipeName}{Suffix.CHEFSANITY}");
            }
        }

        // public static void AddCookingRecipe(Event @event, string[] args, EventContext context)
        public static bool AddCookingRecipe_SkipLearningCookies_Prefix(Event @event, string[] args, EventContext context)
        {
            try
            {
                if (!@event.eventCommands[@event.CurrentCommand].Contains("Cookies"))
                {
                    return true; // run original logic
                }

                ++@event.CurrentCommand;
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(AddCookingRecipe_SkipLearningCookies_Prefix)}:\n{ex}");
                return true; // run original logic
            }
        }

        private static string GetAliased(string recipeName)
        {
            var aliasedRecipeName = recipeName;

            foreach (var (internalName, realName) in NameAliases.RecipeNameAliases)
            {
                if (aliasedRecipeName.Contains(internalName))
                {
                    aliasedRecipeName = aliasedRecipeName.Replace(internalName, realName);
                }
            }

            return aliasedRecipeName;
        }

        private static void CheckCraftingRecipeLocations(string friendName, int currentHearts)
        {
            // There are no crafting recipe learning checks yet
        }
    }
}
