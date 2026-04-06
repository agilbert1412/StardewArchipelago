using System;
using System.Collections.Generic;
using System.Linq;
using Force.DeepCloner;
using KaitoKid.Utilities.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.SlotData.SlotEnums.SlotDataRandomization;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Locations;
using StardewValley.GameData.Shops;

namespace StardewArchipelago.GameModifications.RandomizedData
{
    public class FishDataModifier
    {
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

        public void OnLocationsDataRequested(object sender, AssetRequestedEventArgs e)
        {
            if (!e.NameWithoutLocale.IsEquivalentTo("Data/Locations"))
            {
                return;
            }

            e.Edit(asset =>
                {
                    var locationsData = asset.AsDictionary<string, LocationData>().Data;

                    var originalFishEntries = GetOriginalFishEntries(locationsData);
                    var modifiedFishEntries = GetModifiedFishEntries();

                    foreach (var locationId in locationsData.Keys.ToArray())
                    {
                        ModifyFishLocationsData(locationsData, locationId, originalFishEntries, modifiedFishEntries);
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

            if (randomizedData.Method.Equals(RandomizedFishData.CATCH_METHOD_CRAB_POT))
            {
                fishDataFields = ModifyCrabPotFishDataFields(randomizedData, originalIsCrabPot, fishDataFields, fishName);
            }
            else if (randomizedData.Method.Equals(RandomizedFishData.CATCH_METHOD_FISHING_ROD))
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

            var waterType = randomizedData.Location.Any(x => x.Contains("Freshwater", StringComparison.InvariantCultureIgnoreCase)) ? "freshwater" : "ocean";
            if (originalIsCrabPot)
            {
                const int waterTypeIndex = 4;
                fishDataFields[waterTypeIndex] = waterType;
            }
            else
            {
                const int minSizeIndex = 3;
                const int maxSizeIndex = 4;
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
                fishDataFields = new[] { fishName, "50", "mixed", fishDataFields[minSizeIndex], fishDataFields[maxSizeIndex], "600 2600", "spring summer fall winter", "both", "680 .25", "0", ".3", "0", "0", "false" };
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
            if (randomizedData.Weather != null)
            {
                const int weatherIndex = 7;
                fishDataFields[weatherIndex] = ConvertWeatherName(randomizedData.Weather);
            }

            return fishDataFields;
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

        private Dictionary<string, List<SpawnFishData>> GetOriginalFishEntries(IDictionary<string, LocationData> allLocationData)
        {
            var fishEntries = new Dictionary<string, List<SpawnFishData>>();

            foreach (var (locationId, locationData) in allLocationData)
            {
                for (var i = locationData.Fish.Count - 1; i >= 0; i--)
                {
                    var spawnFishData = locationData.Fish[i];
                    if (spawnFishData.ItemId == null)
                    {
                        if (spawnFishData.RandomItemId == null || spawnFishData.RandomItemId.Count <= 0)
                        {
                            continue;
                        }

                        var atLeastOne = false;
                        foreach (var fishId in spawnFishData.RandomItemId)
                        {
                            if (!_itemManager.ObjectExistsById(fishId))
                            {
                                continue;
                            }

                            var fish = _itemManager.GetObjectById(fishId);
                            if (!_dataRandomization.FishData.ContainsKey(fish.Name))
                            {
                                continue;
                            }

                            fishEntries.TryAdd(fish.Name, new List<SpawnFishData>());
                            fishEntries[fish.Name].Add(spawnFishData);
                            atLeastOne = true;
                        }
                        if (atLeastOne)
                        {
                            locationData.Fish.RemoveAt(i);
                        }
                    }
                    else
                    {
                        var fishId = spawnFishData.ItemId;
                        if (!_itemManager.ObjectExistsById(fishId))
                        {
                            continue;
                        }

                        var fish = _itemManager.GetObjectById(fishId);
                        if (!_dataRandomization.FishData.ContainsKey(fish.Name))
                        {
                            continue;
                        }

                        fishEntries.TryAdd(fish.Name, new List<SpawnFishData>());
                        fishEntries[fish.Name].Add(spawnFishData);
                        locationData.Fish.RemoveAt(i);
                    }
                }
            }

            return fishEntries;
        }

        private Dictionary<string, List<SpawnFishData>> GetModifiedFishEntries()
        {
            var modifiedFishEntries = new Dictionary<string, List<SpawnFishData>>();
            foreach (var (fishName, randomizedFishData) in _dataRandomization.FishData)
            {
                var entriesByLocation = randomizedFishData.GetSpawnFishDatas(_itemManager);
                foreach (var (locationName, entries) in entriesByLocation)
                {
                    modifiedFishEntries.TryAdd(locationName, new List<SpawnFishData>());
                    modifiedFishEntries[locationName].AddRange(entries);
                }
            }

            return modifiedFishEntries;
        }

        private void ModifyFishLocationsData(IDictionary<string, LocationData> allLocationData, string locationId, Dictionary<string, List<SpawnFishData>> originalFishEntries, Dictionary<string, List<SpawnFishData>> modifiedFishEntries)
        {
            var locationData = allLocationData[locationId];

            if (!modifiedFishEntries.ContainsKey(locationId))
            {
                return;
            }

            var modifiedEntriesForLocation = modifiedFishEntries[locationId];

            foreach (var spawnFishData in modifiedEntriesForLocation)
            {
                var fishItem = _itemManager.GetItemByQualifiedId(spawnFishData.ItemId);
                var fishName = fishItem.Name;
                if (originalFishEntries.ContainsKey(fishName))
                {
                    var originalEntriesForThisFish = originalFishEntries[fishName];
                    var newSpawnFishData = originalEntriesForThisFish[0];
                    newSpawnFishData = MergeSpawnFishData(newSpawnFishData, originalEntriesForThisFish);
                    newSpawnFishData = MergeSpawnFishData(newSpawnFishData, spawnFishData);
                    locationData.Fish.Add(newSpawnFishData);
                }
                else
                {
                    locationData.Fish.Add(spawnFishData);
                }
            }
        }

        private SpawnFishData MergeSpawnFishData(SpawnFishData spawnFishData, List<SpawnFishData> ModifiedSpawnFishDatas)
        {
            var newSpawnFishData = spawnFishData.DeepClone();
            foreach (var modifiedSpawnFishData in ModifiedSpawnFishDatas)
            {
                newSpawnFishData = MergeSpawnFishData(spawnFishData, modifiedSpawnFishData);
            }

            return newSpawnFishData;
        }

        private SpawnFishData MergeSpawnFishData(SpawnFishData spawnFishData, SpawnFishData modifiedSpawnFishData)
        {
            var newSpawnFishData = spawnFishData.DeepClone();
            newSpawnFishData.FishAreaId = modifiedSpawnFishData.FishAreaId;
            newSpawnFishData.PlayerPosition = modifiedSpawnFishData.PlayerPosition;
            newSpawnFishData.BobberPosition = modifiedSpawnFishData.BobberPosition;
            newSpawnFishData.Condition = modifiedSpawnFishData.Condition;
            newSpawnFishData.RequireMagicBait = modifiedSpawnFishData.RequireMagicBait;
            newSpawnFishData.ItemId = modifiedSpawnFishData.ItemId;
            newSpawnFishData.Id = modifiedSpawnFishData.Id;
            newSpawnFishData.RandomItemId = modifiedSpawnFishData.RandomItemId;

            return newSpawnFishData;
        }
    }
}
