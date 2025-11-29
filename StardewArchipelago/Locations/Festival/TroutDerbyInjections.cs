using System;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using StardewModdingAPI;
using StardewValley;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.Utilities.Interfaces;

namespace StardewArchipelago.Locations.Festival
{
    internal class TroutDerbyInjections
    {
        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(ILogger logger, IModHelper modHelper, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        // public virtual bool answerDialogueAction(string questionAndAnswer, string[] questionParams)
        public static bool AnswerDialogueAction_TroutDerbyRewards_Prefix(GameLocation __instance, string questionAndAnswer, string[] questionParams, ref bool __result)
        {
            try
            {
                if (!questionAndAnswer.Equals("TroutDerbyBooth_Rewards", StringComparison.InvariantCultureIgnoreCase) || Game1.player.Items.CountId("TroutDerbyTag") <= 0)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                __result = true;
                var numberOfThisTag = 1;
                var locationName = string.Format(FestivalLocationNames.TROUT_DERBY_REWARD_PATTERN, numberOfThisTag);
                while (_archipelago.LocationExists(locationName) && _locationChecker.IsLocationChecked(locationName))
                {
                    numberOfThisTag++;
                    locationName = string.Format(FestivalLocationNames.TROUT_DERBY_REWARD_PATTERN, numberOfThisTag);
                }

                if (_locationChecker.IsLocationMissing(locationName))
                {
                    Game1.stats.Increment("GoldenTagsTurnedIn");
                    Game1.player.Items.ReduceId("TroutDerbyTag", 1);
                    _locationChecker.AddCheckedLocation(locationName);
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(AnswerDialogueAction_TroutDerbyRewards_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }
    }
}
