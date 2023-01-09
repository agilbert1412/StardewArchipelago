using System;
using StardewModdingAPI;
using StardewValley;
using xTile.Dimensions;

namespace StardewArchipelago.Locations.CodeInjections
{
    public static class ScytheInjections
    {
        private const string GOT_GOLDEN_SCYTHE_KEY = "Got_GoldenScythe_Key";

        private static IMonitor _monitor;
        private static LocationChecker _locationChecker;
        private static ModPersistence _modPersistence;

        public static void Initialize(IMonitor monitor, LocationChecker locationChecker)
        {
            _monitor = monitor;
            _locationChecker = locationChecker;
            _modPersistence = new ModPersistence();
        }

        public static bool PerformAction_GoldenScythe_Prefix(GameLocation __instance, string action, Farmer who, Location tileLocation, ref bool __result)
        {
            try
            {
                if (action == null || !who.IsLocalPlayer)
                {
                    return true; // run original logic
                }

                var actionParts = action.Split(' ');
                var actionName = actionParts[0];
                if (actionName == "GoldenScythe")
                {
                    __result = true;
                    var modData = Game1.getFarm().modData;
                    _modPersistence.InitializeModDataValue(GOT_GOLDEN_SCYTHE_KEY, "0");

                    if (modData[GOT_GOLDEN_SCYTHE_KEY] == "0")
                    {
                        Game1.playSound("parry");
                        __instance.setMapTileIndex(29, 4, 245, "Front");
                        __instance.setMapTileIndex(30, 4, 246, "Front");
                        __instance.setMapTileIndex(29, 5, 261, "Front");
                        __instance.setMapTileIndex(30, 5, 262, "Front");
                        __instance.setMapTileIndex(29, 6, 277, "Buildings");
                        __instance.setMapTileIndex(30, 56, 278, "Buildings");
                        _locationChecker.AddCheckedLocation("Grim Reaper statue");
                        modData[GOT_GOLDEN_SCYTHE_KEY] = "1";
                        return false; // don't run original logic
                    }

                    Game1.changeMusicTrack("none");
                    __instance.performTouchAction("MagicWarp Mine 67 10", Game1.player.getStandingPosition());
                    return false; // don't run original logic
                }

                return true; // run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(PerformAction_GoldenScythe_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }
    }
}
