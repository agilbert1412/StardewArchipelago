using System;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using StardewModdingAPI;
using StardewValley;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public static class FarmCaveInjections
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

        // public void answerDialogue(string questionKey, int answerChoice)
        public static bool AnswerDialogue_SendFarmCaveCheck_Prefix(Event __instance, string questionKey, int answerChoice)
        {
            try
            {
                if (questionKey != "cave")
                {
                    return true; // run original logic
                }

                _locationChecker.AddCheckedLocation("Demetrius's Breakthrough");
                return false; // run original logic
            }
            catch (Exception ex)
            {
                _logger.Log($"Failed in {nameof(AnswerDialogue_SendFarmCaveCheck_Prefix)}:\n{ex}",
                    LogLevel.Error);
                return true; // run original logic
            }
        }
    }
}
