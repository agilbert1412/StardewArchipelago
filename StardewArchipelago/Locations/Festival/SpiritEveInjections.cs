using System;
using System.Linq;
using StardewArchipelago.Archipelago;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;

namespace StardewArchipelago.Locations.Festival
{
    internal class SpiritEveInjections
    {
        private static IMonitor _monitor;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(IMonitor monitor, IModHelper modHelper, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _monitor = monitor;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        public static bool CheckForAction_SpiritEveChest_Prefix(Chest __instance, Farmer who, bool justCheckingForActivity, ref bool __result)
        {
            try
            {
                if (justCheckingForActivity || __instance.giftbox.Value || __instance.playerChest.Value)
                {
                    return true; // run original logic
                }

                if (__instance.Items.Count <= 0 || __instance.Items.Count > 1 || __instance.Items.First().ParentSheetIndex != 373)
                {
                    return true; // run original logic
                }

                who.currentLocation.playSound("openChest");
                if (__instance.synchronized.Value)
                    __instance.GetMutex().RequestLock(() => __instance.openChestEvent.Fire());
                else
                    __instance.performOpenChest();

                if (!_archipelago.HasReceivedItem("Golden Pumpkin"))
                {
                    var obj = __instance.Items[0];
                    __instance.Items[0] = null;
                    __instance.Items.RemoveAt(0);
                    __result = true;
                }

                _locationChecker.AddCheckedLocation(FestivalLocationNames.GOLDEN_PUMPKIN);

                return false; // don't run original logic

            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(CheckForAction_SpiritEveChest_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }
    }
}
