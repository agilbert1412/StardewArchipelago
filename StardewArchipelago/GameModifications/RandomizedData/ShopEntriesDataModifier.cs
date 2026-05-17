using KaitoKid.Utilities.Interfaces;
using Newtonsoft.Json;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.ApworldData;
using StardewArchipelago.Archipelago.SlotData.SlotEnums.SlotDataRandomization;
using StardewArchipelago.Constants;
using StardewArchipelago.Constants.Locations;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.GameModifications.Shops;
using StardewArchipelago.Items;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Shops;
using StardewValley.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using StardewArchipelago.Locations.Festival;

namespace StardewArchipelago.GameModifications.RandomizedData
{
    public class ShopEntriesDataModifier
    {
        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static StardewArchipelagoClient _archipelago;
        private static StardewItemManager _itemManager;
        private static DataRandomization _dataRandomization;

        private static HashSet<string> _processedShops;
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
                if (!IsCorrectItem(shopDataItem, randomizedShopItemData, out var priceMultiplier))
                {
                    continue;
                }

                var shouldReplace = ModifyShopItemData(shopDataItem, randomizedShopItemData, priceMultiplier, out var replacementShopItemDatas);
                if (shouldReplace)
                {
                    shopData.Items.RemoveAt(i);
                    shopData.Items.InsertRange(i, replacementShopItemDatas);
                }
                hasModifiedOne = true;
            }

            return hasModifiedOne;
        }

        private bool IsCorrectItem(ShopItemData shopDataItem, RandomizedShopItemData randomizedShopItemData, out double priceMultiplier)
        {
            return IsCorrectItem(shopDataItem.ItemId, shopDataItem.Id, shopDataItem.IsRecipe, randomizedShopItemData, out priceMultiplier);
        }

        private static bool IsCorrectItem(string shopDataItemId, string shopDataEntryId, bool shopDataEntryIsRecipe, RandomizedShopItemData randomizedShopItemData)
        {
            return IsCorrectItem(shopDataItemId, shopDataEntryId, shopDataEntryIsRecipe, randomizedShopItemData, out _);
        }

        private static bool IsCorrectItem(string shopDataItemId, string shopDataEntryId, bool shopDataEntryIsRecipe, RandomizedShopItemData randomizedShopItemData, out double priceMultiplier)
        {
            priceMultiplier = 1;
            var randomizedItemName = randomizedShopItemData.ItemName;
            if (_itemManager.ItemExists(randomizedItemName))
            {
                var randomizedItem = _itemManager.GetItemByName(randomizedItemName);
                var randomizedItemId = randomizedItem.Id;
                var randomizedQualifiedItemId = randomizedItem.GetQualifiedId();
                if (shopDataItemId == randomizedItemId || shopDataItemId == randomizedQualifiedItemId ||
                    shopDataEntryId == randomizedItemId || shopDataEntryId == randomizedQualifiedItemId)
                {
                    if (shopDataEntryIsRecipe)
                    {
                        return randomizedItemName.EndsWith(ItemParser.RECIPE_SUFFIX);
                    }
                    return true;
                }
            }

            if (TryMatchItemId(shopDataItemId, shopDataEntryIsRecipe, randomizedShopItemData, randomizedItemName, out var isCorrectItem, out priceMultiplier))
            {
                return isCorrectItem;
            }

            return false;
        }

        private static bool TryMatchItemId(string shopDataItemId, bool shopDataEntryIsRecipe, RandomizedShopItemData randomizedShopItemData, string randomizedItemName, out bool isCorrectItem, out double priceMultiplier)
        {
            isCorrectItem = false;
            priceMultiplier = 1;
            if (shopDataItemId == null)
            {
                isCorrectItem = false;
                return false;
            }

            if (shopDataItemId.StartsWith(IDProvider.AP_LOCATION))
            {
                var locationName = shopDataItemId.Substring(IDProvider.AP_LOCATION.Length + 1);
                if (locationName.Equals(randomizedShopItemData.ItemName) ||
                    locationName.Equals($"{Prefix.PURCHASE}{randomizedShopItemData.ItemName}") ||
                    locationName.Equals($"{randomizedShopItemData.ItemName}{Suffix.UPGRADE}"))
                {
                    if (shopDataEntryIsRecipe)
                    {
                        isCorrectItem = randomizedItemName.EndsWith(ItemParser.RECIPE_SUFFIX);
                        return true;
                    }
                    isCorrectItem = true;
                    return true;
                }
                if (locationName.Equals(FestivalLocationNames.STRAWBERRY_SEEDS))
                {
                    priceMultiplier = 10;
                    isCorrectItem = true;
                    return true;
                }
            }

            if (shopDataItemId.StartsWith(QualifiedItemIds.TOOLS_QUALIFIER))
            {
                var tool = ItemRegistry.Create(shopDataItemId);
                if (tool.Name.Equals(randomizedShopItemData.ItemName) || tool.Name.Equals($"{Prefix.PURCHASE}{randomizedShopItemData.ItemName}"))
                {
                    if (shopDataEntryIsRecipe)
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

        private bool ModifyShopItemData(ShopItemData shopDataItem, RandomizedShopItemData randomizedShopItemData, double priceMultiplier, out List<ShopItemData> replacementShopItemDatas)
        {
            if (TryReplaceShopItemData(shopDataItem, randomizedShopItemData, out replacementShopItemDatas))
            {
                return true;
            }

            shopDataItem.ModData ??= new Dictionary<string, string>();
            ModifyShopItemCurrency(shopDataItem, randomizedShopItemData);
            ModifyShopItemPrice(shopDataItem, randomizedShopItemData, priceMultiplier);
            ModifyShopItemMaterials(shopDataItem, randomizedShopItemData, priceMultiplier);

            return false;
        }

        private static void ModifyShopItemCurrency(ShopItemData shopDataItem, RandomizedShopItemData randomizedShopItemData)
        {
            ModifyShopItemCurrency(shopDataItem.ModData, randomizedShopItemData);
        }

        private static void ModifyShopItemCurrency(Dictionary<string, string> shopDataItemModData, RandomizedShopItemData randomizedShopItemData)
        {
            if (!string.IsNullOrWhiteSpace(randomizedShopItemData.Currency))
            {
                shopDataItemModData.TryAdd(ShopMenuInjections.CURRENCY_KEY, randomizedShopItemData.Currency);
                shopDataItemModData[ShopMenuInjections.CURRENCY_KEY] = randomizedShopItemData.Currency;
            }
        }

        private static void ModifyShopItemPrice(ShopItemData shopDataItem, RandomizedShopItemData randomizedShopItemData, double priceMultiplier)
        {
            if (randomizedShopItemData.Price.HasValue)
            {
                shopDataItem.Price = (int)Math.Round(randomizedShopItemData.Price.Value * priceMultiplier);
                if (shopDataItem.PriceModifiers != null)
                {
                    shopDataItem.PriceModifiers.Clear();
                }
                shopDataItem.IgnoreShopPriceModifiers = true;
            }
        }

        private static void ModifyShopItemMaterials(ShopItemData shopDataItem, RandomizedShopItemData randomizedShopItemData, double priceMultiplier)
        {
            if (randomizedShopItemData.Materials != null)
            {
                if (randomizedShopItemData.Materials.Count == 1)
                {
                    var (materialName, materialAmount) = randomizedShopItemData.Materials.First();
                    var materialItem = _itemManager.GetObjectByName(materialName);
                    var materialQualifiedId = materialItem.GetQualifiedId();
                    shopDataItem.TradeItemId = materialQualifiedId;
                    shopDataItem.TradeItemAmount = (int)Math.Round(materialAmount * priceMultiplier);
                }
                else
                {
                    shopDataItem.TradeItemId = null;
                    shopDataItem.TradeItemAmount = 0;
                    shopDataItem.ModData.TryAdd(ShopMenuInjections.MATERIALS_KEY, "");
                    var materialsDict = randomizedShopItemData.Materials.ToDictionary(x => _itemManager.GetItemByName(x.Key).GetQualifiedId(), x => Math.Max(1, (int)Math.Round(x.Value * priceMultiplier)));
                    shopDataItem.ModData[ShopMenuInjections.MATERIALS_KEY] = JsonConvert.SerializeObject(materialsDict);
                }
            }
        }

        private bool TryReplaceShopItemData(ShopItemData shopDataItem, RandomizedShopItemData randomizedShopItemData, out List<ShopItemData> replacementShopItemDatas)
        {
            replacementShopItemDatas = null;
            return false;
        }

        // public static IEnumerable<ItemQueryResult> MONSTER_SLAYER_REWARDS(string key, string arguments, ItemQueryContext context, bool avoidRepeat, HashSet<string> avoidItemIds, Action<string, string> logError)
        public static void MonsterSlayerRewards_AddRandomizedData_Postfix(string key, string arguments, ItemQueryContext context, bool avoidRepeat, HashSet<string> avoidItemIds, Action<string, string> logError, ref IEnumerable<ItemQueryResult> __result)
        {
            try
            {
                var shopName = ShopNames.ADVENTURERS_GUILD;
                var randomizedShopData = _dataRandomization.ShopsData[shopName];
                _processedShops.Add(shopName);

                var modifiedShopItems = new List<ItemQueryResult>();

                foreach (var shopItem in __result)
                {
                    foreach (var (randomizedItemName, randomizedShopItemData) in randomizedShopData)
                    {
                        if (IsCorrectItem(shopItem.Item.QualifiedItemId, shopItem.Item.QualifiedItemId, shopItem.Item.IsRecipe, randomizedShopItemData))
                        {
                            ModifyShopItem(shopItem, randomizedShopItemData);
                            break;
                        }
                    }
                    modifiedShopItems.Add(shopItem);
                }

                __result = modifiedShopItems;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(MonsterSlayerRewards_AddRandomizedData_Postfix)}:\n{ex}");
                return;
            }
        }

        private static void ModifyShopItem(ItemQueryResult itemResult, RandomizedShopItemData randomizedShopItemData)
        {

            ModifyShopItemCurrency(itemResult, randomizedShopItemData);
            ModifyShopItemPrice(itemResult, randomizedShopItemData);
            ModifyShopItemMaterials(itemResult, randomizedShopItemData);
        }

        private static void ModifyShopItemCurrency(ItemQueryResult itemResult, RandomizedShopItemData randomizedShopItemData)
        {
            if (itemResult.Item is not Item shopItem)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(randomizedShopItemData.Currency))
            {
                shopItem.modData.TryAdd(ShopMenuInjections.CURRENCY_KEY, randomizedShopItemData.Currency);
                shopItem.modData[ShopMenuInjections.CURRENCY_KEY] = randomizedShopItemData.Currency;
            }
        }

        private static void ModifyShopItemPrice(ItemQueryResult shopDataItem, RandomizedShopItemData randomizedShopItemData)
        {
            if (randomizedShopItemData.Price.HasValue)
            {
                shopDataItem.OverrideBasePrice = randomizedShopItemData.Price.Value;
            }
        }

        private static void ModifyShopItemMaterials(ItemQueryResult itemResult, RandomizedShopItemData randomizedShopItemData)
        {
            if (itemResult.Item is not Item shopItem)
            {
                return;
            }

            if (randomizedShopItemData.Materials != null)
            {
                if (randomizedShopItemData.Materials.Count == 1)
                {
                    var (materialName, materialAmount) = randomizedShopItemData.Materials.First();
                    var materialItem = _itemManager.GetObjectByName(materialName);
                    var materialQualifiedId = materialItem.GetQualifiedId();
                    itemResult.OverrideTradeItemId = materialQualifiedId;
                    itemResult.OverrideTradeItemAmount = materialAmount;
                }
                else
                {
                    itemResult.OverrideTradeItemId = null;
                    itemResult.OverrideTradeItemAmount = 0;
                    shopItem.modData.TryAdd(ShopMenuInjections.MATERIALS_KEY, "");
                    var materialsDict = randomizedShopItemData.Materials.ToDictionary(x => _itemManager.GetItemByName(x.Key).GetQualifiedId(), x => Math.Max(1, x.Value));
                    shopItem.modData[ShopMenuInjections.MATERIALS_KEY] = JsonConvert.SerializeObject(materialsDict);
                }
            }
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
