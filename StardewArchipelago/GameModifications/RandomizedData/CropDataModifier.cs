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
using StardewValley.GameData.Objects;
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

        public void OnObjectDataRequested(object sender, AssetRequestedEventArgs e)
        {
            if (!e.NameWithoutLocale.IsEquivalentTo("Data/Objects"))
            {
                return;
            }

            e.Edit(asset =>
                {
                    var objectsData = asset.AsDictionary<string, ObjectData>().Data;
                    var cropsData = DataLoader.Crops(Game1.content);

                    foreach (var objectId in objectsData.Keys.ToArray())
                    {
                        ModifySeedDescription(objectsData, objectId, cropsData);
                    }
                },
                AssetEditPriority.Late + 1
            );
        }

        private void ModifyCropData(IDictionary<string, CropData> allCropData, string seedId)
        {
            var cropData = allCropData[seedId];
            var cropId = cropData.HarvestItemId;
            var seedItem = _itemManager.GetObjectById(seedId);
            var cropItem = _itemManager.GetObjectById(cropId);

            if (!TryGetRandomizedCropData(seedItem, cropItem, out var randomizedCropData))
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

        private bool TryGetRandomizedCropData(StardewObject seedItem, StardewObject cropItem, out RandomizedCropData randomizedCropData)
        {
            if (_dataRandomization.CropDataBySeedName != null && _dataRandomization.CropDataBySeedName.TryGetValue(seedItem.Name, out randomizedCropData))
            {
                return true;
            }
            if (_dataRandomization.CropData != null && _dataRandomization.CropData.TryGetValue(cropItem.Name, out randomizedCropData))
            {
                return true;
            }

            randomizedCropData = null;
            return false;
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

        private void ModifySeedDescription(IDictionary<string, ObjectData> objectsData, string objectId, Dictionary<string, CropData> cropsData)
        {
            if (!cropsData.TryGetValue(objectId, out var cropData))
            {
                return;
            }

            var cropId = cropData.HarvestItemId;
            var seedItem = _itemManager.GetObjectById(objectId);
            var cropItem = _itemManager.GetObjectById(cropId);

            if (!TryGetRandomizedCropData(seedItem, cropItem, out var randomizedCropData))
            {
                return;
            }

            var season = GetSeasonsText(cropData.Seasons);
            var days = cropData.DaysInPhase.Sum().ToString();
            var cropName = _itemManager.GetObjectById(cropData.HarvestItemId).Name;

            var newDescription = $"Plant in {season}, takes {days} days to grow into a {cropName}.";
            if (cropData.RegrowDays > 0)
            {
                newDescription += $" Continues to produce every {cropData.RegrowDays} days after the first harvest.";
            }

            objectsData[objectId].Description = newDescription;
        }

        private string GetSeasonsText(List<Season> cropSeasons)
        {
            if (cropSeasons == null || cropSeasons.Count <= 0)
            {
                return "any season";
            }

            cropSeasons = cropSeasons.Distinct().ToList();
            if (cropSeasons.Count <= 0 || cropSeasons.Count >= 4)
            {
                return "any season";
            }

            if (cropSeasons.Count == 1)
            {
                return cropSeasons[0].ToString().ToLower();
            }
            if (cropSeasons.Count == 2)
            {
                return $"{cropSeasons[0].ToString().ToLower()} or {cropSeasons[1].ToString().ToLower()}";
            }

            return $"{cropSeasons[0].ToString().ToLower()}, {cropSeasons[1].ToString().ToLower()} or {cropSeasons[2].ToString().ToLower()}";
        }
    }
}
