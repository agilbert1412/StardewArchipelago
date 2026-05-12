using KaitoKid.Utilities.Interfaces;
using Newtonsoft.Json;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.SlotData.SlotEnums.SlotDataRandomization;
using StardewArchipelago.Constants;
using StardewArchipelago.Items;
using StardewArchipelago.Locations.InGameLocations;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.GameData.Shops;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using StardewArchipelago.Archipelago.ApworldData;
using StardewArchipelago.Constants.Locations;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.GameModifications.Shops;
using StardewValley;

namespace StardewArchipelago.GameModifications.RandomizedData
{
    public class ShopEntriesDataModifier
    {
        private ILogger _logger;
        private IModHelper _modHelper;
        private readonly StardewArchipelagoClient _archipelago;
        private readonly StardewItemManager _itemManager;
        private readonly DataRandomization _dataRandomization;

        private readonly HashSet<string> _processedShops;
        private readonly Dictionary<string, RandomizedShopItemData> _specialCaseShops;

        public ShopEntriesDataModifier(ILogger logger, IModHelper modHelper, StardewArchipelagoClient archipelago, StardewItemManager itemManager, DataRandomization dataRandomization)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _itemManager = itemManager;
            _dataRandomization = dataRandomization;
            _processedShops = new HashSet<string>();
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

                    foreach (var (shopName, items) in _dataRandomization.ShopsData)
                    {
                        if (_processedShops.Contains(shopName))
                        {
                            continue;
                        }

                        foreach (var (itemName, itemData) in items)
                        {
                            _specialCaseShops.Add(itemName, itemData);
                        }
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
                _processedShops.Add(shopName);
            }
        }

        private void ModifyShopData(ShopData shopData, Dictionary<string, RandomizedShopItemData> randomizedShopData)
        {
            foreach (var (randomizedShopItemName, randomizedShopItemData) in randomizedShopData)
            {
                if (!TryModifyShopItemData(shopData, randomizedShopItemData))
                {
                    if (_itemsProcessedElsewhere.Any(x =>
                            randomizedShopItemData.ShopName.Equals(x.shopName, StringComparison.InvariantCultureIgnoreCase) &&
                            randomizedShopItemData.ItemName.Equals(x.itemName, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        continue;
                    }
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
            }

            return hasModifiedOne;
        }

        private bool IsCorrectItem(ShopItemData shopDataItem, RandomizedShopItemData randomizedShopItemData)
        {
            var randomizedItemName = randomizedShopItemData.ItemName;
            if (_itemManager.ItemExists(randomizedItemName))
            {
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
            }

            if (TryMatchItemId(shopDataItem, randomizedShopItemData, randomizedItemName, out var isCorrectItem))
            {
                return isCorrectItem;
            }

            return false;
        }

        private static bool TryMatchItemId(ShopItemData shopDataItem, RandomizedShopItemData randomizedShopItemData, string randomizedItemName, out bool isCorrectItem)
        {
            isCorrectItem = false;
            if (shopDataItem.ItemId == null)
            {
                isCorrectItem = false;
                return false;
            }

            if (shopDataItem.ItemId.StartsWith(IDProvider.AP_LOCATION))
            {
                var locationName = shopDataItem.ItemId.Substring(IDProvider.AP_LOCATION.Length + 1);
                if (locationName.Equals(randomizedShopItemData.ItemName) || locationName.Equals($"{Prefix.PURCHASE}{randomizedShopItemData.ItemName}"))
                {
                    if (shopDataItem.IsRecipe)
                    {
                        isCorrectItem = randomizedItemName.EndsWith(ItemParser.RECIPE_SUFFIX);
                        return true;
                    }
                    isCorrectItem = true;
                    return true;
                }
            }

            if (shopDataItem.ItemId.StartsWith(QualifiedItemIds.TOOLS_QUALIFIER))
            {
                var tool = ItemRegistry.Create(shopDataItem.ItemId);
                if (tool.Name.Equals(randomizedShopItemData.ItemName) || tool.Name.Equals($"{Prefix.PURCHASE}{randomizedShopItemData.ItemName}"))
                {
                    if (shopDataItem.IsRecipe)
                    {
                        isCorrectItem = randomizedItemName.EndsWith(ItemParser.RECIPE_SUFFIX);
                        return true;
                    }
                    isCorrectItem = true;
                    return true;
                }
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
                shopDataItem.ModData ??= new Dictionary<string, string>();
                shopDataItem.ModData.TryAdd(ShopMenuInjections.CURRENCY_KEY, randomizedShopItemData.Currency);
                shopDataItem.ModData[ShopMenuInjections.CURRENCY_KEY] = randomizedShopItemData.Currency;
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
                    shopDataItem.ModData.TryAdd(ShopMenuInjections.MATERIALS_KEY, "");
                    var materialsDict = materials.ToDictionary(x => _itemManager.GetItemByName(x.Key).GetQualifiedId(), x => Math.Max(1, x.Value));
                    shopDataItem.ModData[ShopMenuInjections.MATERIALS_KEY] = JsonConvert.SerializeObject(materialsDict);
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

        private static readonly List<(string shopName, string itemName)> _itemsProcessedElsewhere = new()
        {
            (ShopNames.CARPENTER_SHOP, "Coop"),
            (ShopNames.CARPENTER_SHOP, "Barn"),
            (ShopNames.CARPENTER_SHOP, "Big Coop"),
            (ShopNames.CARPENTER_SHOP, "Big Barn"),
            (ShopNames.CARPENTER_SHOP, "Deluxe Coop"),
            (ShopNames.CARPENTER_SHOP, "Deluxe Barn"),
            (ShopNames.CARPENTER_SHOP, "Silo"),
            (ShopNames.CARPENTER_SHOP, "Fish Pond"),
            (ShopNames.CARPENTER_SHOP, "Shipping Bin"),
            (ShopNames.CARPENTER_SHOP, "Pet Bowl"),
            (ShopNames.CARPENTER_SHOP, "Stable"),
            (ShopNames.CARPENTER_SHOP, "Slime Hutch"),
            (ShopNames.CARPENTER_SHOP, "Shed"),
            (ShopNames.CARPENTER_SHOP, "Big Shed"),
        };
    }
}
