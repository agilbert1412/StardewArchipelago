﻿using System;
using System.Collections.Generic;
using System.Linq;
using StardewArchipelago.Constants.Locations;
using StardewArchipelago.Constants.Vanilla;
using StardewModdingAPI;
using StardewValley;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public static class RecipeFriendshipInjections
    {
        private static ILogger _logger;
        private static IModHelper _helper;
        private static StardewArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;
        private static Dictionary<string, string> _allRecipes;

        public static void Initialize(ILogger logger, IModHelper helper, StardewArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _helper = helper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;

            _allRecipes = DataLoader.CookingRecipes(Game1.content);
        }

        // public void grantConversationFriendship(Farmer who, int amount = 20)
        public static void SendFriendshipRecipeChecks(NPC npc, Farmer player)
        {
            try
            {
                if (!player.friendshipData.ContainsKey(npc.Name))
                {
                    return;
                }

                var friendship = player.friendshipData[npc.Name];
                var currentHearts = friendship.Points / 250;
                CheckCookingRecipeLocations(npc.Name, currentHearts);
                CheckCraftingRecipeLocations(npc.Name, currentHearts);

                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(SendFriendshipRecipeChecks)}:\n{ex}");
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
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                ++@event.CurrentCommand;
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(AddCookingRecipe_SkipLearningCookies_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
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
