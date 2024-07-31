using System;
using System.Collections.Generic;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using StardewArchipelago.Stardew;
using StardewValley;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.ArchipelagoUtilities.Net;
using StardewArchipelago.Archipelago;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public static class CookingInjections
    {
        public const string COOKING_LOCATION_PREFIX = "Cook ";

        private static ILogger _logger;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;
        private static StardewItemManager _itemManager;

        public static void Initialize(ILogger logger, ArchipelagoClient archipelago, LocationChecker locationChecker, StardewItemManager itemManager)
        {
            _logger = logger;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            _itemManager = itemManager;
        }

        // public void cookedRecipe(string yieldItemId)
        public static void CookedRecipe_CheckCooksanityLocation_Postfix(Farmer __instance, string itemId)
        {
            try
            {
                if (!_itemManager.ObjectExistsById(itemId))
                {
                    _logger.LogWarning($"Unrecognized cooked recipe: {itemId}");
                    return;
                }

                var cookedItem = _itemManager.GetObjectById(itemId);
                var cookedItemName = cookedItem.Name;
                if (_renamedItems.ContainsKey(itemId))
                {
                    cookedItemName = _renamedItems[itemId];
                }
                var apLocation = $"{COOKING_LOCATION_PREFIX}{cookedItemName}";
                if (_archipelago.GetLocationId(apLocation) > -1)
                {
                    _locationChecker.AddCheckedLocation(apLocation);
                    return;
                }

                _logger.LogError($"Unrecognized Cooksanity Location: {cookedItemName} [{itemId}]");
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CookedRecipe_CheckCooksanityLocation_Postfix)}:\n{ex}");
                return;
            }
        }

        private static readonly Dictionary<string, string> _renamedItems = new()
        {
            { "223", "Cookies" },
        };
    }
}
