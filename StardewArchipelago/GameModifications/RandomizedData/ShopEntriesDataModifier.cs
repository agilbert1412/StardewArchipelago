using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using KaitoKid.Utilities.Interfaces;
using Newtonsoft.Json;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.SlotData.SlotEnums.SlotDataRandomization;
using StardewArchipelago.Constants;
using StardewArchipelago.Items;
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

        private readonly Dictionary<string, RandomizedShopItemData> _specialCaseShops;

        public ShopEntriesDataModifier(ILogger logger, IModHelper modHelper, StardewArchipelagoClient archipelago, StardewItemManager itemManager, DataRandomization dataRandomization)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _itemManager = itemManager;
            _dataRandomization = dataRandomization;
            _specialCaseShops = new Dictionary<string, RandomizedShopItemData>();
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
                    _specialCaseShops.Clear();

                    foreach (var shopId in shopsData.Keys.ToArray())
                    {
                        ModifyShopData(shopsData, shopId);
                    }

                    File.WriteAllText("DR - Special Shops.json", JsonConvert.SerializeObject(_specialCaseShops, Formatting.Indented));
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
                    _specialCaseShops.Add(randomizedShopItemName, randomizedShopItemData);
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
                if (!string.IsNullOrWhiteSpace(randomizedShopItemData.Currency))
                {
                    shopData.Currency = GetVanillaCurrency(randomizedShopItemData.Currency);
                }
            }

            return hasModifiedOne;
        }

        private bool IsCorrectItem(ShopItemData shopDataItem, RandomizedShopItemData randomizedShopItemData)
        {
            var randomizedItemName = randomizedShopItemData.ItemName;
            if (!_itemManager.ItemExists(randomizedItemName))
            {
                return false;
            }
            var randomizedItem = _itemManager.GetItemByName(randomizedItemName);
            var randomizedItemId = randomizedItem.Id;
            var randomizedQualifiedItemId = randomizedItem.GetQualifiedId();
            if (shopDataItem.ItemId == randomizedItemId || shopDataItem.ItemId == randomizedQualifiedItemId ||
                shopDataItem.Id == randomizedItemId || shopDataItem.Id == randomizedQualifiedItemId)
            {
                if (shopDataItem.IsRecipe)
                {
                    return randomizedItemName.EndsWith(ItemParser.RECIPE_SUFFIX);
                }
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

            int? price = null;
            if (randomizedShopItemData.Price != null)
            {
                price = randomizedShopItemData.Price.Value;
            }

            Dictionary<string, int> materials = null;
            if (randomizedShopItemData.Materials != null)
            {
                materials = randomizedShopItemData.Materials.ToDictionary(x => x.Key, x => x.Value);
            }

            if (!string.IsNullOrWhiteSpace(randomizedShopItemData.Currency))
            {
                if (randomizedShopItemData.Currency == "Calico Egg")
                {
                    materials ??= new Dictionary<string, int>();
                    materials.Add(randomizedShopItemData.Currency, price ?? shopDataItem.Price);
                    price = null;
                }
                else
                {
                    shopDataItem.ModData ??= new Dictionary<string, string>();
                    shopDataItem.ModData.TryAdd("Currency", randomizedShopItemData.Currency);
                    shopDataItem.ModData["Currency"] = randomizedShopItemData.Currency;
                }
            }

            if (price.HasValue)
            {
                shopDataItem.Price = price.Value;
                if (shopDataItem.PriceModifiers != null)
                {
                    shopDataItem.PriceModifiers.Clear();
                }
                shopDataItem.IgnoreShopPriceModifiers = true;
            }

            if (materials != null)
            {
                if (materials.Count == 1)
                {
                    var (materialName, materialAmount) = materials.First();
                    var materialItem = _itemManager.GetObjectByName(materialName);
                    var materialQualifiedId = materialItem.GetQualifiedId();
                    shopDataItem.TradeItemId = materialQualifiedId;
                    shopDataItem.TradeItemAmount = materialAmount;
                }
                else
                {
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
                "Calico Egg" => 0, // Money, but zero
                _ => throw new Exception("Invalid Currency"),
            };
        }
    }
}
