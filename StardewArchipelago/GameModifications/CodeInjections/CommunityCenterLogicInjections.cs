﻿using System;
using StardewArchipelago.Locations;
using StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles;
using StardewModdingAPI;
using StardewValley;

namespace StardewArchipelago.GameModifications.CodeInjections
{
    public static class CommunityCenterLogicInjections
    {

        private static IMonitor _monitor;
        private static LocationChecker _locationChecker;

        public static void Initialize(IMonitor monitor, LocationChecker locationChecker)
        {
            _monitor = monitor;
            _locationChecker = locationChecker;
        }

        public static bool HasCompletedCommunityCenter_CheckGameStateInsteadOfLetters_Prefix(Farmer __instance, ref bool __result)
        {
            try
            {
                var allAreasCompleteAccordingToAp =
                    _locationChecker.IsLocationChecked(CommunityCenterInjections.AP_LOCATION_PANTRY) &&
                    _locationChecker.IsLocationChecked(CommunityCenterInjections.AP_LOCATION_BOILER_ROOM) &&
                    _locationChecker.IsLocationChecked(CommunityCenterInjections.AP_LOCATION_BULLETIN_BOARD) &&
                    _locationChecker.IsLocationChecked(CommunityCenterInjections.AP_LOCATION_CRAFTS_ROOM) &&
                    _locationChecker.IsLocationChecked(CommunityCenterInjections.AP_LOCATION_FISH_TANK) &&
                    _locationChecker.IsLocationChecked(CommunityCenterInjections.AP_LOCATION_VAULT);

                var allAreasCompleteLocally =
                    __instance.hasOrWillReceiveMail("apccPantry") &&
                    __instance.hasOrWillReceiveMail("apccCraftsRoom") &&
                    __instance.hasOrWillReceiveMail("apccFishTank") &&
                    __instance.hasOrWillReceiveMail("apccBoilerRoom") &&
                    __instance.hasOrWillReceiveMail("apccVault") &&
                    __instance.hasOrWillReceiveMail("apccBulletin");

                __result = allAreasCompleteAccordingToAp && allAreasCompleteLocally;

                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(HasCompletedCommunityCenter_CheckGameStateInsteadOfLetters_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }
    }
}
