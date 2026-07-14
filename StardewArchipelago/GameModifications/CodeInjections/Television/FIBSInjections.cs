using Archipelago.MultiClient.Net.Models;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.Utilities.Interfaces;
using Microsoft.Xna.Framework.Input;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;
using StardewArchipelago.Constants.Modded;
using StardewArchipelago.Extensions;
using StardewValley;
using StardewValley.GameData.Locations;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StardewArchipelago.GameModifications.CodeInjections.Television
{
    internal class FIBSInjections
    {
        private static ILogger _logger;
        private static StardewArchipelagoClient _archipelago;

        public static void Initialize(ILogger logger, StardewArchipelagoClient archipelago)
        {
            _logger = logger;
            _archipelago = archipelago;
        }

        // private string[] getFishingInfo()
        public static bool GetFishingInfo_AdaptToDataRandomization_Prefix(TV __instance, ref string[] __result)
        {
            try
            {
                if (_archipelago.SlotData.DataRandomization.FishData == null || !_archipelago.SlotData.DataRandomization.FishData.Any())
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                __result = GetFishingInfo();
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(GetFishingInfo_AdaptToDataRandomization_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static string[] GetFishingInfo()
        {
            var fishInfoEntries = new List<string>();
            var fishInfoEntry = new StringBuilder();
            var stringBuilder2 = new StringBuilder();
            var seasonIndex = Game1.seasonIndex;
            fishInfoEntry.AppendLine("---" + Utility.getSeasonNameFromNumber(seasonIndex) + "---^^");
            var allFishData = DataLoader.Fish(Game1.content);
            var allLocationData = Game1.locationData;
            var num = 0;
            foreach (var (fishId, fishData) in allFishData.OrderBy(x => x.Value))
            {
                var fishDataFields = fishData.Split('/');
                var season = fishDataFields[6];

                if (fishData.Contains("spring summer fall winter") &&
                    (!_archipelago.SlotData.DataRandomization.FishData.ContainsKey(fishDataFields[0]) || _archipelago.SlotData.DataRandomization.FishData[fishDataFields[0]].Location == null))
                {
                    continue;
                }

                var fishLocations = ReadFishLocations(allLocationData, fishId);
                if (!fishLocations.Any())
                {
                    continue;
                }

                var fishTime = ArgUtility.SplitBySpace(fishDataFields[5]);
                var fishName = ItemRegistry.GetData("(O)" + fishId)?.DisplayName ?? fishDataFields[0];
                var weather = fishDataFields[7];
                var startTime = fishTime[0];
                var endTime = fishTime[1];
                stringBuilder2.Append(fishName);
                stringBuilder2.Append("... ");

                if (startTime != "600" || endTime != "2600")
                {
                    stringBuilder2.Append(Game1.getTimeOfDayString(Convert.ToInt32(startTime)).Replace(" ", ""));
                    stringBuilder2.Append("-");
                    stringBuilder2.Append(Game1.getTimeOfDayString(Convert.ToInt32(endTime)).Replace(" ", ""));
                    stringBuilder2.Append(", ");
                }

                if (weather != "both")
                {
                    weather = Game1.content.LoadString("Strings\\StringsFromCSFiles:TV_Fishing_Channel_" + weather);
                    stringBuilder2.Append(weather + ", ");
                }

                var flag = false;
                foreach (var fishLocation in fishLocations)
                {
                    if (fishLocation != "")
                    {
                        flag = true;
                        stringBuilder2.Append(fishLocation + ", ");
                    }
                }
                if (flag)
                {
                    stringBuilder2.Append("^^");
                    fishInfoEntry.Append(stringBuilder2.ToString());
                    ++num;
                }
                stringBuilder2.Clear();
                if (num > 5 || (num > 3 && fishInfoEntry.Length > 250))
                {
                    fishInfoEntries.Add(fishInfoEntry.ToString());
                    fishInfoEntry.Clear();
                    num = 0;
                }
            }
            return fishInfoEntries.ToArray();
        }

        private static List<string> ReadFishLocations(IDictionary<string, LocationData> allLocationData, string fishId)
        {
            var fishLocations = new List<string>();
            foreach (var (locationId, locationData) in allLocationData)
            {
                var flag = false;
                if (locationData.Fish == null)
                {
                    continue;
                }
                foreach (var spawnFishData in locationData.Fish)
                {
                    if (spawnFishData.RequireMagicBait)
                    {
                        continue;
                    }
                    var season1 = spawnFishData.Season;
                    if (season1.HasValue)
                    {
                        season1 = spawnFishData.Season;
                        var season2 = Game1.season;
                        if (season1.GetValueOrDefault() != season2)
                        {
                            continue;
                        }
                    }
                    if (spawnFishData.ItemId == fishId || spawnFishData.ItemId == "(O)" + fishId)
                    {
                        if (spawnFishData.Condition != null)
                        {
                            var location = Game1.getLocationFromName(locationId);
                            if (!GameStateQuery.CheckConditions(spawnFishData.Condition, location))
                            {
                                continue;
                            }
                        }
                        AddFishLocation(fishLocations, locationId, spawnFishData);
                    }
                }
            }
            return fishLocations;
        }

        private static void AddFishLocation(List<string> fishLocations, string locationId, SpawnFishData spawnFishData)
        {
            var sanitizedFishingLocation = GetSanitizedFishingLocation(locationId, spawnFishData);
            if (!string.IsNullOrWhiteSpace(sanitizedFishingLocation) && !fishLocations.Contains(sanitizedFishingLocation))
            {
                fishLocations.Add(sanitizedFishingLocation);
            }
        }

        private static string GetSanitizedFishingLocation(string rawLocationName, SpawnFishData spawnFishData)
        {
            var locationName = rawLocationName;
            if (!string.IsNullOrWhiteSpace(spawnFishData.FishAreaId))
            {
                locationName = spawnFishData.FishAreaId.ToLower();
            }

            if (rawLocationName == "Forest" && spawnFishData.FishAreaId == "Lake")
            {
                return "forest lake";
            }

            switch (rawLocationName)
            {
                case "Town":
                case "Forest":
                    return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV_Fishing_Channel_River");
                case "Beach":
                    if (spawnFishData.PlayerPosition.HasValue)
                    {
                        if (spawnFishData.PlayerPosition.Value.X >= 80)
                        {
                            return "tide pools";
                        }
                    }
                    if (spawnFishData.BobberPosition.HasValue)
                    {
                        if (spawnFishData.BobberPosition.Value.Y >= 30 && spawnFishData.BobberPosition.Value.Width <= 15)
                        {
                            return "very deep ocean";
                        }
                    }
                    return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV_Fishing_Channel_Ocean");
                case "Mountain":
                    return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV_Fishing_Channel_Lake");
                case "UndergroundMine":
                    return "underground pond";
                case "BugLand":
                    return "bug lair";
                case "WitchSwamp":
                    return "swamp";
                case "IslandSouthEastCave":
                    return "pirate cove";
                case "fishingGame":
                case "Temp":
                    return "";
                default:
                    return rawLocationName.ToLower();
            }
        }
    }
}
