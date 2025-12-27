using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.Utilities.Interfaces;
using StardewArchipelago.Archipelago.ApworldData;
using StardewModdingAPI;

namespace StardewArchipelago.Locations.Jojapocalypse.Consequences
{
    internal class TravelingCartConsequences
    {
        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;
        private static JojaLocationChecker _jojaLocationChecker;

        public static void Initialize(ILogger logger, IModHelper modHelper, ArchipelagoClient archipelago, JojaLocationChecker jojaLocationChecker)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _jojaLocationChecker = jojaLocationChecker;
        }

        public static bool CanMeetToday()
        {
            var numberTravelingCartPurchases = _jojaLocationChecker.CountCheckedLocationsWithTag(LocationTag.TRAVELING_MERCHANT);
            if (JojapocalypseConsequencesPatcher.RollConsequenceChance(0.1, numberTravelingCartPurchases))
            {
                return false;
            }
            return true;
        }
    }
}
