using System;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.Utilities.Interfaces;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;

namespace StardewArchipelago.Locations.Secrets
{
    public class FishableSecretsInjections
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

        // public void doneHoldingFish(Farmer who, bool endOfNight = false)
        public static void DoneHoldingFish_FishableSecret_Postfix(FishingRod __instance, Farmer who, bool endOfNight)
        {
            try
            {
                var locationsMap = SecretsLocationNames.FISHABLE_QUALIFIED_ITEM_IDS_TO_LOCATIONS;
                var id = __instance.whichFish.QualifiedItemId;
                if (locationsMap.ContainsKey(id))
                {
                    _locationChecker.AddCheckedLocation(locationsMap[id]);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(DoneHoldingFish_FishableSecret_Postfix)}:\n{ex}");
                return;
            }
        }
    }
}
