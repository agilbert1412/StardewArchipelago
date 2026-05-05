using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using KaitoKid.Utilities.Interfaces;
using Newtonsoft.Json;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.SlotData.SlotEnums.SlotDataRandomization;
using StardewArchipelago.Constants;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewModdingAPI.Events;
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
            if (!ShopIds.ShopIdsToApworldNames.ContainsKey(shopId))
            {
                return;
            }

            var shopNames = ShopIds.ShopIdsToApworldNames[shopId];
            ModifyShopData(shopData, shopNames);
        }

        private void ModifyShopData(ShopData shopData, List<string> shopNames)
        {
            foreach (var shopName in shopNames)
            {
                if (!_dataRandomization.ShopsData.ContainsKey(shopName))
                {
                    continue;
                }

                var randomizedShopData = _dataRandomization.ShopsData[shopName];
                ModifyShopData(shopData, randomizedShopData);
            }
        }

        private void ModifyShopData(ShopData shopData, Dictionary<string, RandomizedShopItemData> randomizedShopData)
        {
            foreach (var (randomizedShopItemName, randomizedShopItemData) in randomizedShopData)
            {
                if (!TryModifyShopItemData(shopData, randomizedShopItemData))
                {
                    Debugger.Break();
                }
            }
        }

        private bool TryModifyShopItemData(ShopData shopData, RandomizedShopItemData randomizedShopItemData)
        {
            var hasModifiedOne = false;
            for (var i = shopData.Items.Count - 1; i >= 0; i--)
            {
                var shopDataItem = shopData.Items[i];
                if (!IsCorrectItem(shopDataItem, randomizedShopItemData))
                {
                    continue;
                }

                var shouldReplace = ModifyShopItemData(shopDataItem, randomizedShopItemData, out var replacementShopItemDatas);
                if (shouldReplace)
                {
                    shopData.Items.RemoveAt(i);
                    shopData.Items.InsertRange(i, replacementShopItemDatas);
                }
                hasModifiedOne = true;
                shopData.Currency = GetVanillaCurrency(randomizedShopItemData.Currency);
            }

            return hasModifiedOne;
        }

        private bool IsCorrectItem(ShopItemData shopDataItem, RandomizedShopItemData randomizedShopItemData)
        {
            var randomizedItemName = randomizedShopItemData.ItemName;
            var randomizedItem = _itemManager.GetItemByName(randomizedItemName);
            var randomizedItemId = randomizedItem.Id;
            var randomizedQualifiedItemId = randomizedItem.GetQualifiedId();
            if (shopDataItem.ItemId == randomizedItemId || shopDataItem.ItemId == randomizedQualifiedItemId ||
                shopDataItem.Id == randomizedItemId || shopDataItem.Id == randomizedQualifiedItemId)
            {
                return true;
            }

            return false;
        }

        private bool ModifyShopItemData(ShopItemData shopDataItem, RandomizedShopItemData randomizedShopItemData, out List<ShopItemData> replacementShopItemDatas)
        {
            if (TryReplaceShopItemData(shopDataItem, randomizedShopItemData, out replacementShopItemDatas))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(randomizedShopItemData.Currency))
            {
                shopDataItem.ModData ??= new Dictionary<string, string>();
                shopDataItem.ModData.TryAdd("Currency", randomizedShopItemData.Currency);
                shopDataItem.ModData["Currency"] = randomizedShopItemData.Currency;
            }

            if (randomizedShopItemData.Price.HasValue)
            {
                shopDataItem.Price = randomizedShopItemData.Price.Value;
                shopDataItem.PriceModifiers.Clear();
                shopDataItem.IgnoreShopPriceModifiers = true;
            }

            if (randomizedShopItemData.Materials != null)
            {
                if (randomizedShopItemData.Materials.Count == 1)
                {
                    var (materialName, materialAmount) = randomizedShopItemData.Materials.First();
                    var materialItem = _itemManager.GetObjectByName(materialName);
                    var materialQualifiedId = materialItem.GetQualifiedId();
                    shopDataItem.TradeItemId = materialQualifiedId;
                    shopDataItem.TradeItemAmount = materialAmount;
                }
                else
                {
                    shopDataItem.TradeItemId = null;
                    shopDataItem.TradeItemAmount = 0;

                    shopDataItem.ModData ??= new Dictionary<string, string>();
                    shopDataItem.ModData.TryAdd("Materials", "");
                    shopDataItem.ModData["Materials"] = JsonConvert.SerializeObject(randomizedShopItemData.Materials);
                }
            }

            return false;
        }

        private bool TryReplaceShopItemData(ShopItemData shopDataItem, RandomizedShopItemData randomizedShopItemData, out List<ShopItemData> replacementShopItemDatas)
        {
            replacementShopItemDatas = null;
            return false;
        }

        private int GetVanillaCurrency(string currency)
        {
            // The valid values are 0 (money), 1 (star tokens), 2 (Qi coins), and 4 (Qi gems)
            return currency switch
            {
                "Money" => 0,
                "Star Token" => 1,
                "Qi Coin" => 2,
                "Qi Gem" => 4,
                "Calico Egg" => throw new Exception("Send help"),
                _ => throw new Exception("Invalid Currency"),
            };
        }
    }
}
