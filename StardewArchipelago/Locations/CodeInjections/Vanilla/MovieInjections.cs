using System;
using KaitoKid.ArchipelagoUtilities.Net;
using StardewValley;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public static class MovieInjections
    {

        private static ILogger _logger;
        private static StardewArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(ILogger logger, StardewArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        // 
        public static void Method_Patch_Postfix(GameLocation __instance)
        {
            try
            {

                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(Method_Patch_Postfix)}:\n{ex}");
                return;
            }
        }
    }
}
