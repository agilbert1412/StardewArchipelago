using System;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using StardewValley;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago;
using StardewValley.Locations;

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

        // public void RequestEndMovie(long uid)
        public static bool RequestEndMovie_SendMoviesanityLocations_Prefix(MovieTheater __instance, long uid)
        {
            try
            {
                // Now we can run code here. This code will run BEFORE the original method, every time it is called.

                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(RequestEndMovie_SendMoviesanityLocations_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }
    }
}
