using System;
using System.Collections.Generic;
using System.Linq;
using KaitoKid.Utilities.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.SlotData.SlotEnums.SlotDataRandomization;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.GameData.Shops;

namespace StardewArchipelago.GameModifications.RandomizedData
{
    public class FishDataModifier
    {
        private ILogger _logger;
        private IModHelper _modHelper;
        private readonly StardewArchipelagoClient _archipelago;
        private readonly DataRandomization _dataRandomization;

        public FishDataModifier(ILogger logger, IModHelper modHelper, StardewArchipelagoClient archipelago, DataRandomization dataRandomization)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
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

            var randomizedData = _dataRandomization.FishData[fishName];

            var fishDifficultyOrTrap = fishDataFields[1];

            if (fishDifficultyOrTrap.Equals("trap", StringComparison.InvariantCultureIgnoreCase))
            {
                var fishChance = fishDataFields[2];
                var fishUnused = fishDataFields[3];
                var fishLocation = fishDataFields[4];
                var fishMinSize = fishDataFields[5];
                var fishMaxSize = fishDataFields[6];
            }
            else
            {
                var fishDifficulty = fishDataFields[1];
                var fishDifficultyStyle = fishDataFields[2];
                var fishMinSize = fishDataFields[3];
                var fishMaxSize = fishDataFields[4];
                var fishTimeOfDay = fishDataFields[5];
                var fishSeason = fishDataFields[6];
                var fishWeather = fishDataFields[7];
                var fishLocations = fishDataFields[8];
                var fishMaxDepth = fishDataFields[9];
                var fishChance = fishDataFields[10];
                var fishDepthMultiplier = fishDataFields[11];
                var fishFishingLevel = fishDataFields[12];
                var fishFirstCatchTutorial = fishDataFields[13];

                if (randomizedData.Difficulty != null)
                {
                    fishDifficulty = randomizedData.Difficulty.ToString();
                }
                if (randomizedData.Season != null)
                {
                    fishSeason = string.Join(" ", randomizedData.Season.Select(x => x.ToLower()));
                }
                if (randomizedData.Weather != null)
                {
                    fishWeather = ConvertWeatherName(randomizedData.Weather);
                }
            }

            allFishData[fishId] = modifiedFishData;
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
    }
}
