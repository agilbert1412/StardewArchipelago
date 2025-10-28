using System;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.Utilities.Interfaces;
using StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles;
using StardewValley;

namespace StardewArchipelago.GameModifications.CodeInjections
{
    public static class CommunityCenterLogicInjections
    {

        private static ILogger _logger;
        private static LocationChecker _locationChecker;

        public static void Initialize(ILogger logger, LocationChecker locationChecker)
        {
            _logger = logger;
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

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(HasCompletedCommunityCenter_CheckGameStateInsteadOfLetters_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }
    }
}
