using System;
using System.Collections.Generic;
using System.Linq;
using Archipelago.MultiClient.Net.Models;
using StardewArchipelago.Archipelago;
using StardewModdingAPI;
using StardewValley;
using Object = StardewValley.Object;
using StardewArchipelago.Stardew.NameMapping;
using StardewArchipelago.Constants.Modded;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public static class CraftingInjections
    {
        public const string CRAFTING_LOCATION_PREFIX = "Craft ";

        private static IMonitor _monitor;
        private static IModHelper _helper;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;
        private static CompoundNameMapper _nameMapper;

        public static void Initialize(IMonitor monitor, IModHelper helper, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _monitor = monitor;
            _helper = helper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            _nameMapper = new CompoundNameMapper(archipelago.SlotData);
        }

        // public void checkForCraftingAchievements()
        public static void CheckForCraftingAchievements_CheckCraftsanityLocation_Postfix(Stats __instance)
        {
            try
            {
                var craftedRecipes = Game1.player.craftingRecipes;
                foreach (var recipe in craftedRecipes.Keys)
                {
                    if (craftedRecipes[recipe] <= 0)
                    {
                        continue;
                    }
                    var itemName = _nameMapper.GetItemName(recipe); // Some names are iffy
                    if (IgnoredModdedStrings.Craftables.Contains(itemName))
                    {
                        continue;
                    }
                    var location = $"{CRAFTING_LOCATION_PREFIX}{itemName}";
                    _locationChecker.AddCheckedLocation(location);
                }

                return;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(CheckForCraftingAchievements_CheckCraftsanityLocation_Postfix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }

        // public static void AddCraftingRecipe(Event @event, string[] args, EventContext context)
        public static bool AddCraftingRecipe_SkipLearning_Prefix(Event @event, string[] args, EventContext context)
        {
            try
            {
                if (!@event.eventCommands[@event.CurrentCommand].Contains("Furnace"))
                {
                    return true; // run original logic
                }

                ++@event.CurrentCommand;
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(AddCraftingRecipe_SkipLearning_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }
    }
}
