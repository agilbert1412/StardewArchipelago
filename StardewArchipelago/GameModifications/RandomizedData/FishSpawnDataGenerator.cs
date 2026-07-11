using System;
using Force.DeepCloner;
using KaitoKid.Utilities.Interfaces;
using StardewArchipelago.Archipelago.SlotData.SlotEnums.SlotDataRandomization;
using StardewArchipelago.Constants;
using StardewArchipelago.GameModifications.EntranceRandomizer;
using StardewArchipelago.Stardew;
using StardewValley;
using StardewValley.GameData.Locations;
using System.Collections.Generic;
using System.Linq;
using xTile;

namespace StardewArchipelago.GameModifications.RandomizedData
{
    public class FishSpawnDataGenerator
    {
        private const int DEFAULT_PRECEDENCE = 0;
        private const float DEFAULT_CHANCE = 1.0f;
        public const string CATCH_METHOD_CRAB_POT = "Crab Pot";
        public const string CATCH_METHOD_FISHING_ROD = "Fishing Rod";

        private readonly ILogger _logger;
        private readonly StardewItemManager _itemManager;
        private readonly Dictionary<FishLocation, List<SpawnFishData>> _originalFishEntries;

        public FishSpawnDataGenerator(ILogger logger, StardewItemManager itemManager, Dictionary<FishLocation, List<SpawnFishData>> originalFishEntries)
        {
            _logger = logger;
            _itemManager = itemManager;
            _originalFishEntries = originalFishEntries;
        }

        public Dictionary<FishLocation, List<SpawnFishData>> GetLocationSpawnFishDatas(RandomizedFishData randomizedFishData)
        {
            var spawnDatas = new Dictionary<FishLocation, List<SpawnFishData>>();
            var fishName = randomizedFishData.Name;
            var fish = _itemManager.GetObjectByName(fishName);

            if (FishIsCaughtInCrabPot(randomizedFishData, fish))
            {
                spawnDatas.Add(new FishLocation(fishName, "Default"), new List<SpawnFishData>());
                return spawnDatas;
            }

            if (randomizedFishData.Location == null || !randomizedFishData.Location.Any())
            {
                return spawnDatas;
            }

            var fishQualifiedId = fish.GetQualifiedId();

            var fishLocations = randomizedFishData.Location;
            foreach (var regionName in fishLocations)
            {
                var mapSpawnDatas = new Dictionary<string, List<SpawnFishData>>();
                switch (regionName)
                {
                    case "Forest River":
                        AddMapSpawnDatas(mapSpawnDatas, "Forest", "Sunfish");
                        break;
                    case "Forest Pond":
                        AddMapSpawnDatas(mapSpawnDatas, "Forest", "Smallmouth Bass");
                        break;
                    case "Forest Waterfall":
                        AddMapSpawnDatas(mapSpawnDatas, "Forest", "Goby");
                        break;
                    case "Island West Ocean":
                        AddMapSpawnDatas(mapSpawnDatas, "IslandWest", "Lionfish");
                        break;
                    case "Island West River":
                        AddMapSpawnDatas(mapSpawnDatas, "IslandWest", "Blue Discus");
                        AddMapSpawnDatas(mapSpawnDatas, "IslandNorth", "Blue Discus");
                        break;
                    case "Tide Pools":
                        AddMapSpawnDatas(mapSpawnDatas, "Beach", "Crimsonfish", true);
                        break;
                    case "Night Market":
                        AddMapSpawnDatas(mapSpawnDatas, "Beach", "Blobfish");
                        AddMapSpawnDatas(mapSpawnDatas, "Submarine", "Blobfish");
                        break;
                    case "The Mines - Floor 20":
                        AddMapSpawnDatasWithCondition(mapSpawnDatas, "UndergroundMine", "Stonefish", GameStateConditionProvider.CreateMineFloorCondition(20));
                        break;
                    case "The Mines - Floor 60":
                        AddMapSpawnDatasWithCondition(mapSpawnDatas, "UndergroundMine", "Ice Pip", GameStateConditionProvider.CreateMineFloorCondition(60));
                        break;
                    case "The Mines - Floor 100":
                        AddMapSpawnDatasWithCondition(mapSpawnDatas, "UndergroundMine", "Lava Eel", GameStateConditionProvider.CreateMineFloorCondition(100));
                        break;
                    default:
                        AddAnywhereSpawnDatas(mapSpawnDatas, GetMapName(regionName), fishName);
                        break;
                }

                var conditionsToKeep = new[] { GameStateCondition.PLAYER_SPECIAL_ORDER_RULE_ACTIVE };
                var originalSpawnDatas = CreateSpawnDataFrom(null, fishName, false, false).Where(x => !string.IsNullOrWhiteSpace(x.Condition) && conditionsToKeep.Any(y => x.Condition.Contains(y)));

                string conditionToKeep = null;
                foreach (var originalSpawnData in originalSpawnDatas)
                {
                    conditionToKeep = originalSpawnData.Condition;
                }

                foreach (var (map, addedSpawnDatas) in mapSpawnDatas)
                {
                    foreach (var addedSpawnData in addedSpawnDatas)
                    {
                        addedSpawnData.Id = fishQualifiedId;
                        addedSpawnData.ItemId = fishQualifiedId;
                        addedSpawnData.RandomItemId = null;
                        if (conditionToKeep != null && (addedSpawnData.Condition == null || !addedSpawnData.Condition.Contains(conditionToKeep)))
                        {
                            addedSpawnData.Condition = GameStateConditionProvider.ConcatenateConditions(new []{addedSpawnData.Condition, conditionToKeep}, false);
                        }
                    }
                    var key = new FishLocation(fishName, map);
                    spawnDatas.TryAdd(key, new List<SpawnFishData>());
                    spawnDatas[key].AddRange(addedSpawnDatas);
                }
            }

            return spawnDatas;
        }

        private void AddMapSpawnDatasWithCondition(Dictionary<string, List<SpawnFishData>> mapSpawnDatas, string map, string fish, string condition)
        {
            var spawnDatas = AddMapSpawnDatas(mapSpawnDatas, map, fish);
            foreach (var spawnData in spawnDatas)
            {
                if (spawnData.Condition == null)
                {
                    spawnData.Condition = "";
                }
                if (spawnData.Condition.Contains(condition))
                {
                    continue;
                }
                spawnData.Condition = GameStateConditionProvider.ConcatenateConditions(new[] { spawnData.Condition, condition }, false);
            }
        }

        private IEnumerable<SpawnFishData> AddMapSpawnDatas(Dictionary<string, List<SpawnFishData>> mapSpawnDatas, string map, string fish, bool removeConditions = false)
        {
            mapSpawnDatas.TryAdd(map, new List<SpawnFishData>());
            var spawnData = CreateSpawnDataFrom(map, fish, removeConditions, true);
            mapSpawnDatas[map].AddRange(spawnData);
            return spawnData;
        }

        private void AddAnywhereSpawnDatas(Dictionary<string, List<SpawnFishData>> mapSpawnDatas, string map, string fish, bool removeConditions = false)
        {
            mapSpawnDatas.TryAdd(map, new List<SpawnFishData>());
            mapSpawnDatas[map].AddRange(CreateSpawnDataFrom(null, fish, removeConditions, false));
        }

        private void AddMapSpawnDatasWithFestivalCondition(Dictionary<string, List<SpawnFishData>> mapSpawnDatas, string map, string fish)
        {
            mapSpawnDatas.TryAdd(map, new List<SpawnFishData>());
            mapSpawnDatas[map].AddRange(CreateSpawnDataWithFestivalConditionFrom(fish));
        }

        private IEnumerable<SpawnFishData> CreateSpawnDataFrom(string map, string fish, bool removeConditions, bool keepSpawnFields)
        {
            if (map == null)
            {
                foreach (var (fishLocation, fishEntries) in _originalFishEntries)
                {
                    if (fishLocation.FishName == fish && fishEntries.Any())
                    {
                        map = fishLocation.LocationId;
                        break;
                    }
                }
            }

            if (map == null)
            {
                return new[] { CreateBlankSpawnData(fish) };
            }

            var key = new FishLocation(fish, map);

            var entries = _originalFishEntries[key];
            if (entries.Count > 1)
            {
                _logger.LogWarning($"There are multiple matching fish for '{fish}' in '{map}'. First one will be used.");
            }

            var entry = entries.OrderBy(x => x.Condition?.Length ?? 0).First().DeepClone();
            if (removeConditions)
            {
                entry.Condition = null;
            }

            var newEntries = new List<SpawnFishData> { CleanEntry(entry, keepSpawnFields) };

            switch (fish)
            {
                case "Rainbow Trout":
                case "Squid":
                    newEntries.AddRange(CreateSpawnDataWithFestivalConditionFrom(fish));
                    break;
            }

            return newEntries;
        }

        private IEnumerable<SpawnFishData> CreateSpawnDataWithFestivalConditionFrom(string fish)
        {
            string map = null;
            foreach (var (fishLocation, fishEntries) in _originalFishEntries)
            {
                if (fishLocation.FishName == fish && fishEntries.Any(x => x.Condition != null && x.Condition.Contains(GameStateCondition.IS_PASSIVE_FESTIVAL_OPEN)))
                {
                    map = fishLocation.LocationId;
                    break;
                }
            }

            if (map == null)
            {
                throw new Exception($"Could not find a festival spawn data for {fish}");
            }

            var key = new FishLocation(fish, map);

            var entries = _originalFishEntries[key].Where(x => x.Condition != null && x.Condition.Contains(GameStateCondition.IS_PASSIVE_FESTIVAL_OPEN)).Select(x => x.DeepClone());
            if (!entries.Any())
            {
                throw new Exception($"Could not find a festival spawn data for {fish}");
            }

            return entries.Select(x => CleanEntry(x, false));
        }

        private static SpawnFishData CleanEntry(SpawnFishData entry, bool keepSpawnFields)
        {
            entry.Chance = DEFAULT_CHANCE;
            entry.Precedence = DEFAULT_PRECEDENCE;
            if (!keepSpawnFields)
            {
                if (entry.FishAreaId != null)
                {
                    entry.FishAreaId = null;
                }
                if (entry.RequireMagicBait)
                {
                    entry.RequireMagicBait = false;
                }
                if (entry.BobberPosition != null)
                {
                    entry.BobberPosition = null;
                }
                if (entry.PlayerPosition != null)
                {
                    entry.PlayerPosition = null;
                }
            }
            return entry;
        }

        private static bool FishIsCaughtInCrabPot(RandomizedFishData randomizedFishData, StardewObject fish)
        {
            if (randomizedFishData.Method == CATCH_METHOD_CRAB_POT)
            {
                return true;
            }

            return randomizedFishData.Method == null && DataLoader.Fish(Game1.content)[fish.Id].Split("/")[1].Equals("trap");
        }

        private string GetMapName(string regionName)
        {
            return EntranceManager.TurnAliased(regionName).Split("|").First();
        }

        public SpawnFishData CreateBlankSpawnData(string fishName, string condition = null)
        {
            var spawnData = CreateBlankSpawnData();
            var fish = _itemManager.GetObjectByName(fishName);
            var fishId = fish.GetQualifiedId();
            spawnData.ItemId = fishId;
            spawnData.Id = fishId;
            spawnData.Condition = condition;
            return spawnData;
        }

        public SpawnFishData CreateBlankSpawnData()
        {
            return new SpawnFishData()
            {
                Chance = DEFAULT_CHANCE,
                Precedence = DEFAULT_PRECEDENCE,
            };
        }

        /*
           if (Season != null)
           {
               condition = Season.Length >= 4 ? null : $"LOCATION_SEASON Here {string.Join(" ", Season.Select(x => x.ToLower()))}";
               season = Season.Length != 1 ? null : Season.Select(x => Enum.Parse<Season>(x)).First();
           }
        */
    }
}
