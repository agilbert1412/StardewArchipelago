using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Netcode;
using StardewArchipelago.Archipelago;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using xTile.Dimensions;
using xTile.Tiles;

namespace StardewArchipelago.Locations.CodeInjections
{
    public static class IsolatedEventInjections
    {
        public const string OLD_MASTER_CANNOLI_AP_LOCATION = "Old Master Cannoli";
        public const string BEACH_BRIDGE_AP_LOCATION = "Beach Bridge";

        private static IMonitor _monitor;
        private static IModHelper _helper;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(IMonitor monitor, IModHelper modHelper, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _monitor = monitor;
            _helper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        public static bool CheckAction_OldMasterCanolli_Prefix(Woods __instance, Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who, ref bool __result)
        {
            try
            {
                var tile = __instance.map.GetLayer("Buildings")
                    .PickTile(new Location(tileLocation.X * 64, tileLocation.Y * 64), viewport.Size);
                if (tile == null || !who.IsLocalPlayer || (tile.TileIndex != 1140 && tile.TileIndex != 1141) || !__instance.hasUnlockedStatue.Value)
                {
                    return true; // run original logic
                }

                if (__instance.hasUnlockedStatue.Value && !__instance.localPlayerHasFoundStardrop() && who.freeSpotsInInventory() > 0)
                {
                    _locationChecker.AddCheckedLocation(OLD_MASTER_CANNOLI_AP_LOCATION);
                    if (!Game1.player.mailReceived.Contains("CF_Statue"))
                        Game1.player.mailReceived.Add("CF_Statue");
                }
                __result = true;

                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(CheckAction_OldMasterCanolli_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        public static bool AnswerDialogueAction_BeachBridge_Prefix(Beach __instance, string questionAndAnswer, string[] questionParams, ref bool __result)
        {
            try
            {
                if (questionAndAnswer != "BeachBridge_Yes")
                {
                    return true; // run original logic
                }

                Game1.player.removeItemsFromInventory(388, 300);
                _locationChecker.AddCheckedLocation(BEACH_BRIDGE_AP_LOCATION);
                __result = true;
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(AnswerDialogueAction_BeachBridge_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        public static bool CheckAction_BeachBridge_Prefix(Beach __instance, Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who, ref bool __result)
        {
            try
            {
                if (tileLocation.X != 58 || tileLocation.Y != 13)
                {
                    return true; // run original logic
                }

                __result = true;
                if (_locationChecker.IsLocationChecked(BEACH_BRIDGE_AP_LOCATION))
                {
                    return false; // don't run original logic
                }

                if (who.hasItemInInventory(388, 300))
                {
                    __instance.createQuestionDialogue(
                        Game1.content.LoadString("Strings\\Locations:Beach_FixBridge_Question"),
                        __instance.createYesNoResponses(), "BeachBridge");
                    return false; // don't run original logic
                }

                Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Beach_FixBridge_Hint"));
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(CheckAction_BeachBridge_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }
    }
}
