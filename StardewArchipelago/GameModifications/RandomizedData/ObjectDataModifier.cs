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
using StardewValley.GameData.Objects;
using StardewValley.GameData.Shops;

namespace StardewArchipelago.GameModifications.RandomizedData
{
    public class ObjectDataModifier
    {
        private ILogger _logger;
        private IModHelper _modHelper;
        private readonly StardewArchipelagoClient _archipelago;
        private readonly StardewItemManager _itemManager;
        private readonly DataRandomization _dataRandomization;

        public ObjectDataModifier(ILogger logger, IModHelper modHelper, StardewArchipelagoClient archipelago, StardewItemManager itemManager, DataRandomization dataRandomization)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _itemManager = itemManager;
            _dataRandomization = dataRandomization;
        }

        public void OnObjectDataRequested(object sender, AssetRequestedEventArgs e)
        {
            if (!e.NameWithoutLocale.IsEquivalentTo("Data/Objects"))
            {
                return;
            }

            e.Edit(asset =>
                {
                    var objectData = asset.AsDictionary<string, ObjectData>().Data;

                    foreach (var objectId in objectData.Keys.ToArray())
                    {
                        ModifyObjectData(objectData, objectId);
                    }
                },
                AssetEditPriority.Late
            );
        }

        private void ModifyObjectData(IDictionary<string, ObjectData> allObjectData, string objectId)
        {
            var objectData = allObjectData[objectId];
            var stardewObject = _itemManager.GetObjectById(objectId);
            var objectName = stardewObject.Name;

            if (!TryGetOverridenSellPrice(objectName, out var sellPriceOverride))
            {
                return;
            }

            allObjectData[objectId].Price = sellPriceOverride;
        }

        private bool TryGetOverridenSellPrice(string objectName, out int sellPriceOverride)
        {
            if (_dataRandomization.CropData.ContainsKey(objectName))
            {
                var modifiedCrop = _dataRandomization.CropData[objectName];
                if (modifiedCrop is { SellPrice: > 0 })
                {
                    sellPriceOverride = modifiedCrop.SellPrice.Value;
                    return true;
                }
            }
            if (_dataRandomization.FishData.ContainsKey(objectName))
            {
                var modifiedFish = _dataRandomization.FishData[objectName];
                if (modifiedFish is { SellPrice: > 0 })
                {
                    sellPriceOverride = modifiedFish.SellPrice.Value;
                    return true;
                }
            }

            sellPriceOverride = -1;
            return false;
        }
    }
}
