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
    public class ShopEntriesDataModifier
    {
        private ILogger _logger;
        private IModHelper _modHelper;
        private readonly StardewArchipelagoClient _archipelago;
        private readonly StardewItemManager _itemManager;
        private readonly DataRandomization _dataRandomization;

        public ShopEntriesDataModifier(ILogger logger, IModHelper modHelper, StardewArchipelagoClient archipelago, StardewItemManager itemManager, DataRandomization dataRandomization)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _itemManager = itemManager;
            _dataRandomization = dataRandomization;
        }

        public void OnShopsDataRequested(object sender, AssetRequestedEventArgs e)
        {
            if (!e.NameWithoutLocale.IsEquivalentTo("Data/Shops"))
            {
                return;
            }

            e.Edit(asset =>
                {
                    var shopsData = asset.AsDictionary<string, ShopData>().Data;

                    foreach (var shopId in shopsData.Keys.ToArray())
                    {
                        ModifyShopData(shopsData, shopId);
                    }
                },
                AssetEditPriority.Late + 1
            );
        }

        private void ModifyShopData(IDictionary<string, ShopData> allShopsData, string shopId)
        {
            var shopData = allShopsData[shopId];

        }
    }
}
