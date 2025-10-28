using System;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using StardewModdingAPI;
using StardewValley.Locations;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.Utilities.Interfaces;
using Microsoft.Xna.Framework;

namespace StardewArchipelago.Locations.Festival
{
    internal class MermaidHouseInjections
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

        // public override void UpdateWhenCurrentLocation(GameTime time)

        public static void UpdateWhenCurrentLocation_SongFinished_Postfix(MermaidHouse __instance, GameTime time)
        {
            try
            {
                // private float oldStopWatchTime;
                var oldStopWatchTime = _modHelper.Reflection.GetField<float>(__instance, "oldStopWatchTime").GetValue();
                if (oldStopWatchTime < 65000)
                {
                    return;
                }

                _locationChecker.AddCheckedLocation(FestivalLocationNames.MERMAID_SHOW);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(UpdateWhenCurrentLocation_SongFinished_Postfix)}:\n{ex}");
                return;
            }
        }
    }
}
