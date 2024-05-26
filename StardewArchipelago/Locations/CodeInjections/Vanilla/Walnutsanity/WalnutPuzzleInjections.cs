using System;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants;
using StardewArchipelago.Constants.Vanilla;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Minigames;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Walnutsanity
{
    public static class WalnutPuzzleInjections
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

        // public override void receiveLeftClick(int x, int y, bool playSound = true)
        public static bool ReceiveLeftClick_CrackGoldenCoconut_Prefix(GeodeMenu __instance, int x, int y, bool playSound)
        {
            try
            {
                if (__instance.waitingForServerResponse || !__instance.geodeSpot.containsPoint(x, y) || __instance.heldItem == null ||
                    !Utility.IsGeode(__instance.heldItem) || Game1.player.Money < 25 || __instance.geodeAnimationTimer > 0 ||
                    (Game1.player.freeSpotsInInventory() <= 1 && __instance.heldItem.Stack > 1) || Game1.player.freeSpotsInInventory() < 1 ||
                    __instance.heldItem.QualifiedItemId != QualifiedItemIds.GOLDEN_COCONUT)
                {
                    return true; // run original logic
                }

                var goldenCoconutLocation = $"Open Golden Coconut";
                if (Game1.netWorldState.Value.GoldenCoconutCracked && _locationChecker.IsLocationMissing(goldenCoconutLocation))
                {
                    // Just in case
                    _locationChecker.AddCheckedLocation(goldenCoconutLocation);
                    return true; // run original logic
                }

                __instance.waitingForServerResponse = true;
                Game1.player.team.goldenCoconutMutex.RequestLock(() =>
                {
                    __instance.waitingForServerResponse = false;
                    var itemToSpawnId = $"(AP){IDProvider.AP_LOCATION} {goldenCoconutLocation}";
                    __instance.geodeTreasureOverride = ItemRegistry.Create(itemToSpawnId);
                    Game1.netWorldState.Value.GoldenCoconutCracked = true;
                    __instance.startGeodeCrack();
                }, () =>
                {
                    __instance.waitingForServerResponse = false;
                    __instance.startGeodeCrack();
                });

                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(ReceiveLeftClick_CrackGoldenCoconut_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }
    }
}
