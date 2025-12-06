using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.Utilities.Interfaces;
using StardewArchipelago.Archipelago.ApworldData;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System.Collections.Generic;

namespace StardewArchipelago.Locations.Jojapocalypse.Consequences
{
    internal class HatConsequences
    {
        private const double ROLL_DELAY = 0.25;
        private const int FRAME_DELAY = (int)(60 * ROLL_DELAY);


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
        public static void OnUpdateTicked(UpdateTickedEventArgs updateTickedEventArgs)
        {
            if (!updateTickedEventArgs.IsMultipleOf(FRAME_DELAY))
            {
                return;
            }

            if (Game1.player.hat.Value == null)
            {
                return;
            }

            var numberHatsPurchased = _jojaLocationChecker.CountCheckedLocationsWithTag(LocationTag.HATSANITY);
            if (numberHatsPurchased <= 0)
            {
                return;
            }

            var hatDifficultyFactors = new Dictionary<string, int>()
            {
                {LocationTag.HAT_EASY, 1},             // 20 -> 20
                {LocationTag.HAT_TAILORING, 1},        // 30 -> 30
                {LocationTag.HAT_MEDIUM, 3},           // 29 -> 87
                {LocationTag.HAT_DIFFICULT, 7},        // 12 -> 84
                {LocationTag.HAT_RNG, 10},             // 16 -> 160
                {LocationTag.HAT_NEAR_PERFECTION, 20}, // 11 -> 220
                {LocationTag.HAT_POST_PERFECTION, 40}, // 3  -> 120
                {LocationTag.HAT_IMPOSSIBLE, 100},     // 1  -> 100
            };                                         // Total: 823

            var purchasedHatsFactor = 0;
            foreach (var (tag, factor) in hatDifficultyFactors)
            {
                var numberPurchased = _jojaLocationChecker.CountCheckedLocationsWithTag(tag);
                purchasedHatsFactor += numberPurchased * factor;
            }

            if (JojapocalypseConsequencesPatcher.RollConsequenceChance(0.00005, purchasedHatsFactor, Game1.ticks))
            {
                var hatToDrop = Game1.player.Equip(null, Game1.player.hat);
                Game1.createItemDebris(hatToDrop, Game1.player.Position, Game1.player.FacingDirection, Game1.player.currentLocation, 0);
            }
        }
    }
}
