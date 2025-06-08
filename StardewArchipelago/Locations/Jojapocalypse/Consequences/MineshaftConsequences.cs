using System;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago.ApworldData;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;

namespace StardewArchipelago.Locations.Jojapocalypse.Consequences
{
    internal class MineshaftConsequences
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

        public static bool CanUseElevatorToday()
        {
            var numberElevatorsPurchased = _jojaLocationChecker.CountCheckedLocationsWithTag(LocationTag.ELEVATOR);
            if (JojapocalypseConsequencesPatcher.RollConsequenceChance(0.1, numberElevatorsPurchased))
            {
                return false;
            }
            return true;
        }
    }
}
