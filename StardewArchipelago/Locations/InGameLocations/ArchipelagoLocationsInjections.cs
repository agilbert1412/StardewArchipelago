using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewArchipelago.Archipelago;
using StardewModdingAPI;
using StardewValley;

namespace StardewArchipelago.Locations.InGameLocations
{
    internal class ArchipelagoLocationsInjections
    {
        private static IMonitor _monitor;
        private static IModHelper _helper;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(IMonitor monitor, IModHelper helper, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _monitor = monitor;
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
                    return true; // run original logic
                }

                if (item is not ArchipelagoLocation apLocation)
                {
                    return true; // run original logic
                }

                apLocation.SendCheck();
                __instance.removeItemFromInventory(item);
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(OnItemReceived_PickUpACheck_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }
    }
}
