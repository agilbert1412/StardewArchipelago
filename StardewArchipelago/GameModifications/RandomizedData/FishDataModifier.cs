using Force.DeepCloner;
using KaitoKid.Utilities.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.SlotData.SlotEnums.SlotDataRandomization;
using StardewArchipelago.Constants;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Locations;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile.Dimensions;

namespace StardewArchipelago.GameModifications.RandomizedData
{
    public class FishDataModifier
    {
        private static readonly string[] _immuneLocations = { "Default", "fishingGame", "Temp" };

        private ILogger _logger;
        private IModHelper _modHelper;
        private readonly StardewArchipelagoClient _archipelago;
        private readonly StardewItemManager _itemManager;
        private readonly DataRandomization _dataRandomization;

        public FishDataModifier(ILogger logger, IModHelper modHelper, StardewArchipelagoClient archipelago, StardewItemManager itemManager, DataRandomization dataRandomization)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _itemManager = itemManager;
            _dataRandomization = dataRandomization;
        }

        public void OnFishDataRequested(object sender, AssetRequestedEventArgs e)
        {
            if (!e.NameWithoutLocale.IsEquivalentTo("Data/Fish"))
            {
                return;
            }

            e.Edit(asset =>
                {
                    var fishData = asset.AsDictionary<string, string>().Data;

                    foreach (var fishId in fishData.Keys.ToArray())
                    {
                        ModifyFishData(fishData, fishId);
                    }
                },
                AssetEditPriority.Late
            );
        }

        private void ModifyFishData(IDictionary<string, string> allFishData, string fishId)
        {
            var fishData = allFishData[fishId];
            var fishDataFields = fishData.Split("/");
            var fishName = fishDataFields[0];

            if (!_dataRandomization.FishData.ContainsKey(fishName))
            {
#if DEBUG
                if (_dataRandomization.FishData.Keys.Any(x => x.Replace(" ", "").Equals(fishName.Replace(" ", ""), StringComparison.InvariantCultureIgnoreCase)))
                {
                    var matchingFish = _dataRandomization.FishData.Keys.First(x => x.Replace(" ", "").Equals(fishName.Replace(" ", ""), StringComparison.InvariantCultureIgnoreCase));
                    throw new Exception($"Fish Randomized data contains `{matchingFish}` but game fish data has it as `{fishName}`");
                }
#endif
                return;
            }

            const int difficultyIndex = 1;
            var fishDifficultyOrTrap = fishDataFields[difficultyIndex];

            var randomizedData = _dataRandomization.FishData[fishName];

            var originalIsCrabPot = fishDifficultyOrTrap.Equals("trap", StringComparison.InvariantCultureIgnoreCase);

            var isCrabPot = randomizedData.Method == null ? originalIsCrabPot : randomizedData.Method.Equals(FishSpawnDataGenerator.CATCH_METHOD_CRAB_POT);
            var isFishingRod = randomizedData.Method == null ? !originalIsCrabPot : randomizedData.Method.Equals(FishSpawnDataGenerator.CATCH_METHOD_FISHING_ROD);

            if (isCrabPot)
            {
                fishDataFields = ModifyCrabPotFishDataFields(randomizedData, originalIsCrabPot, fishDataFields, fishName);
            }
            else if (isFishingRod)
            {
                fishDataFields = ModifyFishingRodFishDataFields(originalIsCrabPot, fishDataFields, fishName, randomizedData, difficultyIndex);
            }
            else
            {
                return;
            }

            allFishData[fishId] = string.Join("/", fishDataFields);
        }

        private static string[] ModifyCrabPotFishDataFields(RandomizedFishData randomizedData, bool originalIsCrabPot, string[] fishDataFields, string fishName)
        {
            const int minSizeIndex = 3;
            const int maxSizeIndex = 4;
            if (randomizedData.Location == null)
            {
                return fishDataFields;
            }
            var waterType = randomizedData.Location.Any(x => x.Contains("Freshwater", StringComparison.InvariantCultureIgnoreCase)) ? "freshwater" : "ocean";
            if (originalIsCrabPot)
            {
                const int waterTypeIndex = 4;
                fishDataFields[waterTypeIndex] = waterType;
            }
            else
            {
                fishDataFields = new[] { fishName, "trap", ".15", "684 .45", waterType, fishDataFields[minSizeIndex], fishDataFields[maxSizeIndex], "false" };
            }
            return fishDataFields;
        }

        private string[] ModifyFishingRodFishDataFields(bool originalIsCrabPot, string[] fishDataFields, string fishName, RandomizedFishData randomizedData, int difficultyIndex)
        {
            if (originalIsCrabPot)
            {
                const int minSizeIndex = 5;
                const int maxSizeIndex = 6;
                fishDataFields = new[] { fishName, "45", "mixed", fishDataFields[minSizeIndex], fishDataFields[maxSizeIndex], "600 2600", "spring summer fall winter", "both", "680 .25", "0", ".3", "0", "0", "false" };
            }
            if (randomizedData.Difficulty != null)
            {
                fishDataFields[difficultyIndex] = randomizedData.Difficulty.ToString();
            }
            if (randomizedData.Season != null)
            {
                const int seasonIndex = 6;
                fishDataFields[seasonIndex] = string.Join(" ", randomizedData.Season.Select(x => x.ToLower()));
            }
            else
            {
                SanitizeFishWithWrongSeasonInVanillaData(fishDataFields, fishName);
            }
            if (randomizedData.Weather != null)
            {
                const int weatherIndex = 7;
                fishDataFields[weatherIndex] = ConvertWeatherName(randomizedData.Weather);
            }

            const int maxDepthIndex = 9;
            const int depthMultiplierIndex = 11;
            var maxDepth = int.Parse(fishDataFields[maxDepthIndex]);
            var depthMultiplier = double.Parse(fishDataFields[depthMultiplierIndex]);
            if (maxDepth > 2)
            {
                maxDepth = 2;
                fishDataFields[maxDepthIndex] = maxDepth.ToString();
            }
            if (maxDepth > 1 && depthMultiplier < 0.25)
            {
                depthMultiplier = 0.25;
                fishDataFields[depthMultiplierIndex] = ".25";
            }


            return fishDataFields;
        }

        private static void SanitizeFishWithWrongSeasonInVanillaData(string[] fishDataFields, string fishName)
        {
            const int seasonIndex = 6;
            switch (fishName)
            {
                case "Crimsonfish":
                    fishDataFields[seasonIndex] = "summer";
                    return;
                case "Angler":
                    fishDataFields[seasonIndex] = "fall";
                    return;
                case "Legend":
                    fishDataFields[seasonIndex] = "spring";
                    return;
                case "Glacierfish":
                    fishDataFields[seasonIndex] = "winter";
                    return;
                case "Mutant Carp":
                case "Son of Crimsonfish":
                case "Ms. Angler":
                case "Legend II":
                case "Radioactive Carp":
                case "Glacierfish Jr.":
                    fishDataFields[seasonIndex] = "spring summer fall winter";
                    return;
            }
        }

        private string ConvertWeatherName(string[] weathers)
        {
            if (!weathers.Any())
            {
                throw new ArgumentException($"Fish needs some weathers");
            }
            if (weathers.Length >= 2)
            {
                return "both";
            }

            if (weathers[0] == "Rain")
            {
                return "rainy";
            }

            return "sunny";
        }

        public void OnLocationsDataRequested(object sender, AssetRequestedEventArgs e)
        {
            if (!e.NameWithoutLocale.IsEquivalentTo("Data/Locations"))
            {
                return;
            }

            e.Edit(asset =>
                {
                    try
                    {
                        var locationsData = asset.AsDictionary<string, LocationData>().Data;

                        SanitizeFishData(locationsData);
                        if (_dataRandomization.FishData == null || !_dataRandomization.FishData.Any())
                        {
                            return;
                        }

                        var originalFishEntries = GetOriginalFishEntries(locationsData);
                        var modifiedFishEntries = GetModifiedFishLocationEntries(originalFishEntries);
                        var originalFishEntriesGrouped = originalFishEntries.GroupBy(x => x.Key.FishName).OrderBy(x => x.Key).ToArray();
                        var modifiedFishEntriesGrouped = modifiedFishEntries.GroupBy(x => x.Key.FishName).OrderBy(x => x.Key).ToArray();
                        DeleteOriginalFishEntries(locationsData, modifiedFishEntries);
                        AddNewFishEntries(locationsData, modifiedFishEntries);
                        UpdateFishEntriesSeasons(locationsData);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Failed at editing fish assets. Message: {ex.Message}{Environment.NewLine}{Environment.NewLine}{ex.StackTrace}");
                    }
                },
                AssetEditPriority.Late
            );
        }

        private void SanitizeFishData(IDictionary<string, LocationData> allLocationData)
        {
            foreach (var (locationId, locationData) in allLocationData)
            {
                SanitizeRandomFishData(locationId, locationData);
            }
            AddMineshaftFishData(allLocationData["UndergroundMine"]);
        }

        private void SanitizeRandomFishData(string locationId, LocationData locationData)
        {
            if (_immuneLocations.Contains(locationId))
            {
                return;
            }

            for (var i = locationData.Fish.Count - 1; i >= 0; i--)
            {
                var spawnFishData = locationData.Fish[i];

                if (spawnFishData.RandomItemId == null || spawnFishData.RandomItemId.Count <= 0)
                {
                    continue;
                }

                for (var ri = spawnFishData.RandomItemId.Count - 1; ri >= 0; ri--)
                {
                    var randomItemId = spawnFishData.RandomItemId[ri];
                    if (!_itemManager.ObjectExistsById(randomItemId))
                    {
                        continue;
                    }

                    var fish = _itemManager.GetObjectById(randomItemId);
                    var fishName = fish.Name;
                    if (!_dataRandomization.FishData.ContainsKey(fishName))
                    {
                        continue;
                    }

                    spawnFishData.RandomItemId.RemoveAt(ri);
                    var randomFishSpawnData = spawnFishData.DeepClone();
                    randomFishSpawnData.RandomItemId = null;
                    randomFishSpawnData.Id = randomItemId;
                    randomFishSpawnData.ItemId = randomItemId;
                    locationData.Fish.Add(randomFishSpawnData);
                }

                if (!spawnFishData.RandomItemId.Any())
                {
                    locationData.Fish.RemoveAt(i);
                }
            }
        }

        private void AddMineshaftFishData(LocationData locationData)
        {
            var spawnDataGenerator = new FishSpawnDataGenerator(_logger, _itemManager, null);
            locationData.Fish.Add(spawnDataGenerator.CreateBlankSpawnData("Stonefish", GameStateConditionProvider.CreateMineFloorCondition(20)));
            locationData.Fish.Add(spawnDataGenerator.CreateBlankSpawnData("Ice Pip", GameStateConditionProvider.CreateMineFloorCondition(60)));
            locationData.Fish.Add(spawnDataGenerator.CreateBlankSpawnData("Lava Eel", GameStateConditionProvider.CreateMineFloorCondition(100)));
        }

        private Dictionary<FishLocation, List<SpawnFishData>> GetOriginalFishEntries(IDictionary<string, LocationData> allLocationData)
        {
            var fishEntries = new Dictionary<FishLocation, List<SpawnFishData>>();

            foreach (var (locationId, locationData) in allLocationData)
            {
                foreach (var spawnFishData in locationData.Fish)
                {
                    if (spawnFishData.ItemId == null)
                    {
                        continue;
                    }

                    var fishId = spawnFishData.ItemId;
                    if (!_itemManager.ObjectExistsById(fishId))
                    {
                        continue;
                    }

                    var fish = _itemManager.GetObjectById(fishId);
                    var fishKey = new FishLocation(fish.Name, locationId);
                    fishEntries.TryAdd(fishKey, new List<SpawnFishData>());
                    fishEntries[fishKey].Add(spawnFishData);
                }
            }

            return fishEntries;
        }

        private Dictionary<FishLocation, List<SpawnFishData>> GetModifiedFishLocationEntries(Dictionary<FishLocation, List<SpawnFishData>> originalFishEntries)
        {
            var modifiedFishEntries = new Dictionary<FishLocation, List<SpawnFishData>>();
            var spawnDataGenerator = new FishSpawnDataGenerator(_logger, _itemManager, originalFishEntries);
            foreach (var (fishName, randomizedFishData) in _dataRandomization.FishData)
            {
                if (randomizedFishData.Location == null || !randomizedFishData.Location.Any())
                {
                    continue;
                }

                var entriesByLocation = spawnDataGenerator.GetLocationSpawnFishDatas(randomizedFishData);
                foreach (var (fishLocation, entries) in entriesByLocation)
                {
                    modifiedFishEntries.TryAdd(fishLocation, new List<SpawnFishData>());
                    modifiedFishEntries[fishLocation].AddRange(entries);
                }
            }

            return modifiedFishEntries;
        }

        private void DeleteOriginalFishEntries(IDictionary<string, LocationData> locationsData, Dictionary<FishLocation, List<SpawnFishData>> modifiedFishEntries)
        {
            var modifiedFish = modifiedFishEntries.Select(x => x.Key.FishName).ToHashSet();
            var locationsToDeleteFrom = locationsData.Keys.Where(x => !_immuneLocations.Contains(x)).ToArray();
            foreach (var fishName in modifiedFish)
            {
                var fish = _itemManager.GetObjectByName(fishName);
                var fishId = fish.Id;
                var fishQualifiedId = fish.GetQualifiedId();

                foreach (var locationId in locationsToDeleteFrom)
                {
                    locationsData[locationId].Fish = locationsData[locationId].Fish.Where(x => !IsFishToDelete(x, fishQualifiedId)).ToList();
                }
            }
        }

        private bool IsFishToDelete(SpawnFishData x, string fishToDeleteQualifiedId)
        {
            if (x.ItemId != null)
            {
                if (QualifiedItemIds.QualifiedObjectId(x.ItemId).Equals(fishToDeleteQualifiedId))
                {
                    return true;
                }
            }
            if (x.Id != null)
            {
                if (QualifiedItemIds.QualifiedObjectId(x.Id).Equals(fishToDeleteQualifiedId))
                {
                    return true;
                }
            }
            if (x.RandomItemId != null && x.RandomItemId.Any())
            {
                if (x.RandomItemId.Any(y => QualifiedItemIds.QualifiedObjectId(y).Equals(fishToDeleteQualifiedId)))
                {
                    return true;
                }
            }
            return false;
        }

        private void AddNewFishEntries(IDictionary<string, LocationData> locationsData, Dictionary<FishLocation, List<SpawnFishData>> modifiedFishEntries)
        {
            foreach (var (fishLocation, newFishEntries) in modifiedFishEntries)
            {
                ModifyFishLocationsData(locationsData, fishLocation, newFishEntries);
            }
        }

        private void ModifyFishLocationsData(IDictionary<string, LocationData> allLocationData, FishLocation fishLocation, List<SpawnFishData> newFishEntries)
        {
            allLocationData[fishLocation.LocationId].Fish.AddRange(newFishEntries);
        }
        
        private void UpdateFishEntriesSeasons(IDictionary<string, LocationData> locationsData)
        {
            var fishData = DataLoader.Fish(Game1.content);
            foreach (var locationId in locationsData.Keys.ToArray())
            {
                if (_immuneLocations.Contains(locationId))
                {
                    continue;
                }

                var locationData = locationsData[locationId];
                foreach (var fishInLocation in locationData.Fish)
                {
                    var fishId = fishInLocation.ItemId;
                    if (!_itemManager.ObjectExistsById(fishId))
                    {
                        continue;
                    }
                    var fishName = _itemManager.GetObjectById(fishId).Name;
                    if (!_dataRandomization.FishData.ContainsKey(fishName))
                    {
                        continue;
                    }

                    var fishDataParts = fishData[QualifiedItemIds.UnqualifyId(fishId)].Split("/");
                    var seasons = new List<Season>();
                    if (fishDataParts[1] == "trap")
                    {
                        seasons.AddRange(new []{Season.Spring, Season.Summer, Season.Fall, Season.Winter});
                    }
                    else
                    {
                        var seasonStrings = fishDataParts[6].Split(" ");
                        seasons.AddRange(seasonStrings.Select(x => Enum.Parse<Season>(x, true)));
                    }

                    // This will be handled by logic after the beta async
                    if (locationId.Contains("Island") || locationId.Contains("Caldera") || locationId.Contains("Desert"))
                    {
                        seasons.Add(Season.Summer);
                    }
                    if (locationId.Contains("Submarine"))
                    {
                        seasons.Add(Season.Winter);
                    }

                    fishInLocation.Condition = GameStateConditionProvider.RemoveCondition(fishInLocation.Condition, GameStateCondition.LOCATION_SEASON);
                    fishInLocation.Season = null;

                    seasons = seasons.ToHashSet().ToList();
                    if (seasons.Count >= 4)
                    {
                        continue;
                    }
                    else if (seasons.Count <= 0)
                    {
                        throw new Exception($"Fish '{fishName}' has no seasons!");
                    }

                    fishInLocation.Condition = GameStateConditionProvider.ConcatenateConditions(new[] { fishInLocation.Condition, GameStateConditionProvider.CreateLocationSeasonsCondition(seasons.ToArray()) }, false);
                }
            }
        }
    }
}
