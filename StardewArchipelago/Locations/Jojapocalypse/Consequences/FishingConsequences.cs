using System;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.ApworldData;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.Extensions;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;
using Object = StardewValley.Object;

namespace StardewArchipelago.Locations.Jojapocalypse.Consequences
{
    internal class FishingConsequences
    {
        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static StardewArchipelagoClient _archipelago;
        private static JojaLocationChecker _jojaLocationChecker;

        public static void Initialize(ILogger logger, IModHelper modHelper, StardewArchipelagoClient archipelago, JojaLocationChecker jojaLocationChecker)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _jojaLocationChecker = jojaLocationChecker;
        }

        // public void pullFishFromWater(string fishId, int fishSize, int fishQuality, int fishDifficulty, bool treasureCaught, bool wasPerfect, bool fromFishPond, string setFlagOnCatch, bool isBossFish, int numCaught)
        public static bool PullFishFromWater_ReplaceWithTrash_Prefix(FishingRod __instance, ref string fishId, int fishSize, int fishQuality, int fishDifficulty, bool treasureCaught, bool wasPerfect, bool fromFishPond, string setFlagOnCatch, bool isBossFish, int numCaught)
        {
            try
            {
                var numberPurchased = _jojaLocationChecker.CountCheckedLocationsWithTag(LocationTag.FISHSANITY);
                if (numberPurchased <= 0)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                var fish = ItemRegistry.Create<Object>(fishId);
                var fishName = fish.Name;
                var hasPurchasedThatFish = _jojaLocationChecker.HasCheckedLocation($"Fishsanity: {fishName}");
                if (!hasPurchasedThatFish)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (JojapocalypseConsequencesPatcher.RollConsequenceChance(0.015, numberPurchased, Game1.currentLocation.Name.GetHash(), Game1.timeOfDay, $"{fishId}_{numCaught}_{fishSize}".GetHash()))
                {
                    fishId = ObjectIds.TRASH;
                }

                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(PullFishFromWater_ReplaceWithTrash_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }
    }
}
