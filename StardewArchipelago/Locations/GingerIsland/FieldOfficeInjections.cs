using System;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using StardewModdingAPI;
using StardewValley;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;

namespace StardewArchipelago.Locations.GingerIsland
{
    public class FieldOfficeInjections
    {
        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(ILogger logger, IModHelper modHelper, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        // public static void AddCraftingRecipe(Event @event, string[] args, EventContext context)
        public static bool AddCraftingRecipe_OstrichIncubator_Prefix(Event @event, string[] args, EventContext context)
        {
            try
            {
                if (!ArgUtility.TryGetRemainder(args, 1, out var recipe, out _))
                {
                    return true; // run original logic
                }

                if (!recipe.Equals("Ostrich Incubator", StringComparison.OrdinalIgnoreCase))
                {
                    return true; // run original logic
                }

                _locationChecker.AddCheckedLocation("Complete Island Field Office");
                ++@event.CurrentCommand;
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(AddCraftingRecipe_OstrichIncubator_Prefix)}:\n{ex}");
                return true; // run original logic
            }
        }
    }
}
