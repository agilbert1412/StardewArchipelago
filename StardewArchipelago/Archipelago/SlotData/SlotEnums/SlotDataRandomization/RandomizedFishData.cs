using Microsoft.Xna.Framework;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.GameModifications.EntranceRandomizer;
using StardewArchipelago.Locations.Secrets;
using StardewArchipelago.Stardew;
using StardewValley;
using StardewValley.GameData.Locations;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StardewArchipelago.Archipelago.SlotData.SlotEnums.SlotDataRandomization
{
    public class RandomizedFishData
    {
        public const string CATCH_METHOD_CRAB_POT = "Crab Pot";
        public const string CATCH_METHOD_FISHING_ROD = "Fishing Rod";

        public string Name { get; set; }
        public string Method { get; set; }
        public int? Difficulty { get; set; }
        public string[] Season { get; set; }
        public string[] Location { get; set; }
        public string[] Weather { get; set; }

        public RandomizedFishData()
        {

        }

        public Dictionary<string, List<SpawnFishData>> GetSpawnFishDatas(StardewItemManager itemManager)
        {
            var spawnDatas = new Dictionary<string, List<SpawnFishData>>();
            if (Method == CATCH_METHOD_CRAB_POT)
            {
                return spawnDatas;
            }

            var fish = itemManager.GetObjectByName(Name);
            var fishQualifiedId = fish.GetQualifiedId();

            foreach (var regionName in Location)
            {
                var mapName = GetMapName(regionName);

                string fishAreaId = null;
                Rectangle? playerPosition = null;
                Rectangle? bobberPosition = null;

                if (regionName == "Forest River")
                {
                    mapName = "Forest";
                    fishAreaId = "River";
                }
                else if (regionName == "Forest Pond")
                {
                    mapName = "Forest";
                    fishAreaId = "Lake";
                }
                else if (regionName == "Island West Ocean")
                {
                    mapName = "IslandWest";
                    fishAreaId = "Ocean";
                }
                else if (regionName == "Island West River")
                {
                    mapName = "IslandWest";
                    fishAreaId = "Freshwater";
                }
                else if (regionName == "Tide Pools")
                {
                    mapName = "Beach";
                    playerPosition = new Rectangle(82, 0, 100, 100);
                }
                else if (regionName == "Night Market")
                {
                    mapName = "Submarine";
                    spawnDatas.TryAdd("Beach", new List<SpawnFishData>());
                    var beachSpawn = CreateNightMarketFishSpawnDataOnBeach(fishQualifiedId);
                    spawnDatas["Beach"].Add(beachSpawn);
                }
                else if (regionName.StartsWith("The Mines"))
                {
                    mapName = "UndergroundMine";
                }

                var spawnData = new SpawnFishData()
                {
                    FishAreaId = fishAreaId,
                    PlayerPosition = playerPosition,
                    BobberPosition = bobberPosition,
                    Condition = $"LOCATION_SEASON Here {string.Join(" ", Season.Select(x => x.ToLower()))}",
                    RequireMagicBait = false,
                    Id = fishQualifiedId,
                    ItemId = fishQualifiedId,
                };

                spawnDatas.TryAdd(mapName, new List<SpawnFishData>());
                spawnDatas[mapName].Add(spawnData);
            }

            return spawnDatas;
        }

        private string GetMapName(string regionName)
        {
            return EntranceManager.TurnAliased(regionName);
        }

        private SpawnFishData CreateNightMarketFishSpawnDataOnBeach(string fishId)
        {
            return new SpawnFishData()
            {
                Chance = 0.1f,
                FishAreaId = null,
                BobberPosition = new Rectangle
                {
                    X = 0,
                    Y = 32,
                    Width = 12,
                    Height = 255,
                },
                MinDistanceFromShore = 3,
                ApplyDailyLuck = false,
                CuriosityLureBuff = 0.205f,
                RequireMagicBait = true,
                Precedence = -10,
                IgnoreFishDataRequirements = true,
                CanBeInherited = false,
                Id = fishId,
                ItemId = fishId,
            };
        }
    }
}
