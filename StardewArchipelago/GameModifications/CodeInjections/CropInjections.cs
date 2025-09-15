using System;
using System.Collections.Generic;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.Serialization;
using StardewArchipelago.Stardew;
using StardewValley;
using StardewValley.GameData.Crops;

namespace StardewArchipelago.GameModifications.CodeInjections
{
    public class CropInjections
    {
        private const int SPRING_SEEDS = 495;
        private const int SUMMER_SEEDS = 496;
        private const int FALL_SEEDS = 497;
        private const int WINTER_SEEDS = 498;

        private static readonly string[] _flowerSeeds =
        {
            ObjectIds.TULIP_BULB, ObjectIds.JAZZ_SEEDS, ObjectIds.SPANGLE_SEEDS, ObjectIds.POPPY_SEEDS,
            ObjectIds.SUNFLOWER_SEEDS, ObjectIds.FAIRY_SEEDS,
        };
        private static readonly string[] _overpoweredSeeds = { ObjectIds.ANCIENT_SEEDS, ObjectIds.RARE_SEED };

        private static ILogger _logger;
        private static ArchipelagoClient _archipelago;
        private static StardewItemManager _stardewItemManager;
        private static ArchipelagoWalletDto _wallet;

        public static void Initialize(ILogger logger, ArchipelagoClient archipelago, StardewItemManager stardewItemManager, ArchipelagoWalletDto wallet)
        {
            _logger = logger;
            _archipelago = archipelago;
            _stardewItemManager = stardewItemManager;
            _wallet = wallet;
        }

        // public static string ResolveSeedId(string yieldItemId, GameLocation location)
        public static bool ResolveSeedId_MixedSeedsBecomesUnlockedCrop_Prefix(string itemId, GameLocation location, ref string __result)
        {
            try
            {
                if (itemId == ObjectIds.MIXED_FLOWER_SEEDS)
                {
                    var randomSeed = GetWeigthedRandomUnlockedFlower(Game1.season);
                    __result = randomSeed;
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                if (itemId == ObjectIds.MIXED_SEEDS)
                {
                    var randomSeed = GetWeigthedRandomUnlockedCrop(Game1.season);
                    __result = randomSeed;
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(ResolveSeedId_MixedSeedsBecomesUnlockedCrop_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static string GetWeigthedRandomUnlockedFlower(Season season)
        {
            var flowerCandidates = _flowerSeeds.ToDictionary(x => x, x => _stardewItemManager.GetObjectById(x).PrepareForGivingToFarmer());
            var location = Game1.currentLocation;
            var cropData = DataLoader.Crops(Game1.content);
            var flowerSeedCandidates = _flowerSeeds.Where(x => SeedCanBePlantedHere(flowerCandidates[x], location, season, cropData) && _archipelago.HasReceivedItem(flowerCandidates[x].Name)).ToArray();

            if (!flowerSeedCandidates.Any())
            {
                return ObjectIds.INVALID;
            }

            var randomIndex = Game1.random.Next(flowerSeedCandidates.Length);
            var randomSeed = flowerSeedCandidates[randomIndex];
            return randomSeed;
        }

        private static string GetWeigthedRandomUnlockedCrop(Season season)
        {
            var cropData = DataLoader.Crops(Game1.content);
            var cropCandidates = cropData.ToDictionary(x => x.Key, x => _stardewItemManager.GetObjectById(x.Key).PrepareForGivingToFarmer());
            var location = Game1.currentLocation;
            var mixedSeedCandidates = cropData.Where(kvp => !_flowerSeeds.Contains(kvp.Key) &&
                                                            SeedCanBePlantedHere(cropCandidates[kvp.Key], location, season, cropData) && _archipelago.HasReceivedItem(cropCandidates[kvp.Key].Name))
                .Select(x => x.Key)
                .ToList();

            switch (season)
            {
                case Season.Spring:
                    mixedSeedCandidates.Add(_stardewItemManager.GetItemByName("Spring Seeds").Id);
                    break;
                case Season.Summer:
                    mixedSeedCandidates.Add(_stardewItemManager.GetItemByName("Summer Seeds").Id);
                    break;
                case Season.Fall:
                    mixedSeedCandidates.Add(_stardewItemManager.GetItemByName("Fall Seeds").Id);
                    break;
                case Season.Winter:
                    mixedSeedCandidates.Add(_stardewItemManager.GetItemByName("Winter Seeds").Id);
                    break;
            }

            var weightedSeeds = new List<string>();
            foreach (var seed in mixedSeedCandidates)
            {
                if (_overpoweredSeeds.Contains(seed))
                {
                    weightedSeeds.Add(seed);
                }
                else if (SeedRegrows(seed, cropData))
                {
                    weightedSeeds.AddRange(Enumerable.Repeat(seed, 10));
                }
                else
                {
                    weightedSeeds.AddRange(Enumerable.Repeat(seed, 100));
                }
            }

            var randomIndex = Game1.random.Next(weightedSeeds.Count);
            var randomSeed = weightedSeeds[randomIndex];
            return randomSeed;
        }

        private static bool SeedCanBePlantedHere(Item x, GameLocation location, Season season, Dictionary<string, CropData> cropData)
        {
            if (!cropData.ContainsKey(x.ItemId))
            {
                return false;
            }

            if (location.SeedsIgnoreSeasonsHere())
            {
                return true;
            }

            var seedSeasons = cropData[x.ItemId].Seasons;
            return seedSeasons.Contains(season);
        }

        private static bool SeedRegrows(string seedId, Dictionary<string, CropData> cropData)
        {
            if (!cropData.ContainsKey(seedId))
            {
                return false;
            }

            return cropData[seedId].RegrowDays != -1;
        }

        // public virtual void newDay(int state)
        public static void NewDay_CountDeadCrops_Prefix(Crop __instance, int state)
        {
            try
            {
                var currentLocation = __instance.currentLocation;
                if (!currentLocation.IsOutdoors || __instance.IsInSeason(currentLocation))
                {
                    return;
                }

                if (__instance.dead.Value)
                {
                    return;
                }
                var cropId = __instance.indexOfHarvest.Value;
                if (cropId == null)
                {
                    return;
                }
                _wallet.DeadCropsById.TryAdd(cropId, 0);
                _wallet.DeadCropsById[cropId]++;
                _logger.LogDebug($"Dead {cropId}: {_wallet.DeadCropsById[cropId]}");
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(NewDay_CountDeadCrops_Prefix)}:\n{ex}");
                return;
            }
        }
    }
}
