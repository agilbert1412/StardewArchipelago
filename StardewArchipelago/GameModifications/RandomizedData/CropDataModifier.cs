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
using StardewValley.GameData.Crops;
using StardewValley.GameData.Locations;
using StardewValley.GameData.Shops;

namespace StardewArchipelago.GameModifications.RandomizedData
{
    public class CropDataModifier
    {
        private ILogger _logger;
        private IModHelper _modHelper;
        private readonly StardewArchipelagoClient _archipelago;
        private readonly StardewItemManager _itemManager;
        private readonly DataRandomization _dataRandomization;

        public CropDataModifier(ILogger logger, IModHelper modHelper, StardewArchipelagoClient archipelago, StardewItemManager itemManager, DataRandomization dataRandomization)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _itemManager = itemManager;
            _dataRandomization = dataRandomization;
        }

        public void OnCropDataRequested(object sender, AssetRequestedEventArgs e)
        {
            if (!e.NameWithoutLocale.IsEquivalentTo("Data/Crops"))
            {
                return;
            }

            e.Edit(asset =>
                {
                    var cropsData = asset.AsDictionary<string, CropData>().Data;

                    foreach (var seedId in cropsData.Keys.ToArray())
                    {
                        ModifyCropData(cropsData, seedId);
                    }
                },
                AssetEditPriority.Late
            );
        }

        private void ModifyCropData(IDictionary<string, CropData> allCropData, string seedId)
        {
            var cropData = allCropData[seedId];
            var cropId = cropData.HarvestItemId;
            var seedItem = _itemManager.GetObjectById(seedId);
            var cropItem = _itemManager.GetObjectById(cropId);

            if (_dataRandomization.CropDataBySeedName != null && _dataRandomization.CropDataBySeedName.TryGetValue(seedItem.Name, out var randomizedCropData))
            {
            }
            else if (_dataRandomization.CropData != null && _dataRandomization.CropData.TryGetValue(cropItem.Name, out randomizedCropData))
            {
            }
            else
            {
                return;
            }

            cropItem = _itemManager.GetObjectByName(randomizedCropData.Crop);
            allCropData[seedId].HarvestItemId = cropItem.Id;

            if (randomizedCropData.Season != null)
            {
                allCropData[seedId].Seasons = randomizedCropData.Season.Select(Enum.Parse<Season>).ToList();
            }

            ModifyCropGrowthTime(allCropData, seedId, randomizedCropData);
        }

        private static void ModifyCropGrowthTime(IDictionary<string, CropData> allCropData, string seedId, RandomizedCropData randomizedCropData)
        {
            if (randomizedCropData.GrowthTime is not > 0)
            {
                return;
            }

            var newGrowthTime = randomizedCropData.GrowthTime.Value;
            var currentGrowthPhases = allCropData[seedId].DaysInPhase;
            var currentGrowthTime = currentGrowthPhases.Sum();
            if (currentGrowthTime == newGrowthTime)
            {
                return;
            }

            var multiplier = (double)newGrowthTime / (double)currentGrowthTime;
            var newGrowthPhases = currentGrowthPhases.Select(x => (int)Math.Floor(x * multiplier)).ToList();
            var flooredSum = newGrowthPhases.Sum();
            var remainder = newGrowthTime - flooredSum;
            var originalOrder = Enumerable.Range(0, currentGrowthPhases.Count).OrderByDescending(i => newGrowthPhases[i] <= 0 ? int.MaxValue : currentGrowthPhases[i]).ToArray();
            for (var i = 0; i < originalOrder.Length; i++)
            {
                var index = originalOrder[i];
                newGrowthPhases[index]++;
                remainder--;
                if (remainder <= 0)
                {
                    break;
                }
            }

            allCropData[seedId].DaysInPhase = newGrowthPhases;
        }
    }
}
