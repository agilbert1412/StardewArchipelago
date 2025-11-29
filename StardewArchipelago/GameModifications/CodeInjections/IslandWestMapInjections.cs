using System;
using System.Collections.Generic;
using HarmonyLib;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.Utilities.Interfaces;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley.Locations;

namespace StardewArchipelago.GameModifications.CodeInjections
{
    internal class IslandWestMapInjections
    {

        private static ILogger _logger;
        private static IModHelper _modHelper;

        public static void PatchMapInjections(ILogger logger, IModHelper helper, Harmony harmony)
        {
            _logger = logger;
            _modHelper = helper;
            harmony.Patch(
                original: AccessTools.Method(typeof(IslandWest), nameof(IslandWest.ApplyFarmHouseRestore)),
                prefix: new HarmonyMethod(typeof(IslandWestMapInjections), nameof(ApplyFarmHouseRestore_RestoreOnlyCorrectParts_Prefix)));
        }

        // public void ApplyFarmHouseRestore()
        public static bool ApplyFarmHouseRestore_RestoreOnlyCorrectParts_Prefix(IslandWest __instance)
        {
            try
            {
                if (__instance.map == null)
                {
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;;
                }

                // protected HashSet<string> _appliedMapOverrides;
                var appliedMapOverridesField = _modHelper.Reflection.GetField<HashSet<string>>(__instance, "_appliedMapOverrides");
                var appliedMapOverrides = appliedMapOverridesField.GetValue();
                if (__instance.farmhouseRestored.Value && !appliedMapOverrides.Contains("Island_House_Restored"))
                {
                    __instance.ApplyMapOverride("Island_House_Restored", destination_rect: new Rectangle(74, 33, 7, 9));
                    __instance.ApplyMapOverride("Island_House_Bin", destination_rect: new Rectangle(__instance.shippingBinPosition.X, __instance.shippingBinPosition.Y - 1, 2, 2));
                    __instance.ApplyMapOverride("Island_House_Cave", destination_rect: new Rectangle(95, 30, 3, 4));
                }

                if (__instance.farmhouseMailbox.Value)
                {
                    __instance.setMapTile(81, 40, 771, "Buildings", "untitled tile sheet", "Mailbox");
                    __instance.setMapTile(81, 39, 739, "Front", "untitled tile sheet");
                }

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(ApplyFarmHouseRestore_RestoreOnlyCorrectParts_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;;
            }
        }
    }
}
