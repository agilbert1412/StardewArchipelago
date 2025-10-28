using System;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.Utilities.Interfaces;
using StardewArchipelago.Constants;
using StardewModdingAPI;
using StardewValley;

namespace StardewArchipelago.Locations.InGameLocations
{
    internal class ArchipelagoLocationsInjections
    {
        private static ILogger _logger;
        private static IModHelper _helper;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(ILogger logger, IModHelper helper, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _helper = helper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        // public void OnItemReceived(Item item, int countAdded, Item mergedIntoStack, bool hideHudNotification = false)
        public static bool OnItemReceived_PickUpACheck_Prefix(Farmer __instance, Item item, int countAdded, Item mergedIntoStack, bool hideHudNotification)
        {
            try
            {
                if (__instance == null || item == null)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (item is not ObtainableArchipelagoLocation apLocation)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                apLocation.SendCheck();
                __instance.removeItemFromInventory(item);
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(OnItemReceived_PickUpACheck_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public bool couldInventoryAcceptThisItem(string id, int stack, int quality = 0)
        public static bool CouldInventoryAcceptThisItemById_ChecksFlyingAround_Prefix(Farmer __instance, string id, int stack, int quality, ref bool __result)
        {
            try
            {
                if (id == null)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (id.Contains(IDProvider.AP_LOCATION, StringComparison.InvariantCultureIgnoreCase))
                {
                    __result = true;
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CouldInventoryAcceptThisItemById_ChecksFlyingAround_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public bool couldInventoryAcceptThisItem(Item item)
        public static bool CouldInventoryAcceptThisItemByItem_ChecksFlyingAround_Prefix(Farmer __instance, Item item, ref bool __result)
        {
            try
            {
                if (item is ObtainableArchipelagoLocation)
                {
                    __result = true;
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CouldInventoryAcceptThisItemByItem_ChecksFlyingAround_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }
    }
}
