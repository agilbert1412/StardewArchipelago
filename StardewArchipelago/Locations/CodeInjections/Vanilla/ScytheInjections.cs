using System;
using StardewValley;
using xTile.Dimensions;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Constants;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public static class ScytheInjections
    {
        private const string GRIM_REAPER_STATUE = "Grim Reaper statue";

        private static ILogger _logger;
        private static LocationChecker _locationChecker;

        public static void Initialize(ILogger logger, LocationChecker locationChecker)
        {
            _logger = logger;
            _locationChecker = locationChecker;
        }

        // public virtual bool performAction(string[] action, Farmer who, Location tileLocation)
        public static bool PerformAction_GoldenScythe_Prefix(GameLocation __instance, string[] action, Farmer who, Location tileLocation, ref bool __result)
        {
            try
            {
                if (action == null || !who.IsLocalPlayer || __instance.ShouldIgnoreAction(action, who, tileLocation))
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }
                if (!ArgUtility.TryGet(action, 0, out var actionName, out _, name: "string actionType") || string.IsNullOrEmpty(actionName))
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }
                
                if (actionName == "GoldenScythe")
                {
                    __result = true;
                    if (_locationChecker.IsLocationNotChecked(GRIM_REAPER_STATUE))
                    {
                        Game1.playSound("parry");
                        __instance.setMapTile(29, 4, 245, "Front", "mine");
                        __instance.setMapTile(30, 4, 246, "Front", "mine");
                        __instance.setMapTile(29, 5, 261, "Front", "mine");
                        __instance.setMapTile(30, 5, 262, "Front", "mine");
                        __instance.setMapTile(29, 6, 277, "Buildings", "mine");
                        __instance.setMapTile(30, 56, 278, "Buildings", "mine");
                        _locationChecker.AddCheckedLocation(GRIM_REAPER_STATUE);
                        return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                    }

                    Game1.changeMusicTrack("silence");
                    __instance.performTouchAction("MagicWarp Mine 67 10", Game1.player.getStandingPosition());
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(PerformAction_GoldenScythe_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }
    }
}
