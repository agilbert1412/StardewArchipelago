using System;
using System.Collections.Generic;
using System.Linq;
using Force.DeepCloner;
using StardewArchipelago.Constants;
using StardewArchipelago.Constants.Locations;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Shops;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.SlotData;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;
using StardewValley.Extensions;
using StardewValley.Internal;

namespace StardewArchipelago.Locations.ShopStockModifiers
{
    public class ToolShopStockModifier : ShopStockModifier
    {
        private static StardewArchipelagoClient _staticArchipelago;

        public ToolShopStockModifier(ILogger logger, IModHelper modHelper, StardewArchipelagoClient archipelago, StardewItemManager stardewItemManager) : base(logger, modHelper, archipelago, stardewItemManager)
        {
            _staticArchipelago = archipelago;
        }

        public override void OnShopStockRequested(object sender, AssetRequestedEventArgs e)
        {
            if (!AssetIsShops(e))
            {
                return;
            }

            e.Edit(asset =>
                {
                    var shopsData = asset.AsDictionary<string, ShopData>().Data;
                    var toolShopData = shopsData["ClintUpgrade"];
                    ReplaceToolUpgradesWithApChecks(toolShopData);
                    ReplaceVanillaToolUpgradesWithCustomQuery(toolShopData);
                    MakeEverythingCheaper(toolShopData);
                },
                AssetEditPriority.Late
            );
        }

        private void ReplaceToolUpgradesWithApChecks(ShopData toolShopData)
        {
            if (!_archipelago.SlotData.ToolProgression.HasFlag(ToolProgression.Progressive))
            {
                return;
            }

            var toolsData = Game1.toolData;
            toolShopData.Items.Clear();
            foreach (var toolName in Tools.ToolNames)
            {
                for (var i = 1; i < Materials.MaterialNames.Length; i++)
                {
                    var locationName = $"{Materials.MaterialNames[i]} {toolName}{Suffix.UPGRADE}";
                    if (!_archipelago.LocationExists(locationName))
                    {
                        continue;
                    }
                    var requiredItem = $"{Prefix.PROGRESSIVE}{toolName}";
                    var requiredAmount = i - 1;
                    var id = $"{IDProvider.AP_LOCATION} {locationName}";
                    var toolId = $"{Materials.InternalMaterialNames[i]}{toolName.Replace(" ", "")}";

                    var toolData = toolsData[toolId];

                    var apShopItem = new ShopItemData()
                    {
                        Id = id,
                        ItemId = id,
                        AvailableStock = 1,
                        IsRecipe = false,
                        Price = toolData.SalePrice > 0 ? toolData.SalePrice : Materials.ToolUpgradeCosts[i],
                        TradeItemId = GetToolUpgradeConventionalTradeItem(i),
                        TradeItemAmount = 5,
                        Condition = GameStateConditionProvider.CreateHasReceivedItemCondition(requiredItem, requiredAmount),
                    };

                    toolShopData.Items.Add(apShopItem);
                }
            }
        }

        private void ReplaceVanillaToolUpgradesWithCustomQuery(ShopData toolShopData)
        {
            if (_archipelago.SlotData.ToolProgression.HasFlag(ToolProgression.Progressive))
            {
                return;
            }

            var priceMultiplier = _archipelago.SlotData.ToolPriceMultiplier;
            if (Math.Abs(priceMultiplier - 1.0) < 0.01)
            {
                return;
            }

            var upgradeItem = toolShopData.Items.FirstOrDefault(x => x.ItemId == "TOOL_UPGRADES");
            if (upgradeItem == null)
            {
                return;
            }

            toolShopData.Items.Clear();
            var newUpgradeItem = upgradeItem.DeepClone();
            newUpgradeItem.ItemId = IDProvider.TOOL_UPGRADES_CHEAPER;
            toolShopData.Items.Add(newUpgradeItem);
        }

        private void MakeEverythingCheaper(ShopData toolShopData)
        {
            var priceMultiplier = _archipelago.SlotData.ToolPriceMultiplier;
            if (Math.Abs(priceMultiplier - 1.0) < 0.01)
            {
                return;
            }

            foreach (var toolShopItem in toolShopData.Items)
            {
                toolShopItem.Price = (int)Math.Round(toolShopItem.Price * priceMultiplier);
                toolShopItem.TradeItemAmount = (int)Math.Round(toolShopItem.TradeItemAmount * priceMultiplier);
            }
        }

        private static string GetToolUpgradeConventionalTradeItem(int level)
        {
            switch (level)
            {
                case 1:
                    return "334";
                case 2:
                    return "335";
                case 3:
                    return "336";
                case 4:
                    return "337";
                default:
                    return "334";
            }
        }

        public static IEnumerable<ItemQueryResult> ToolUpgradesCheaperQuery(
            string key,
            string arguments,
            ItemQueryContext context,
            bool avoidRepeat,
            HashSet<string> avoidItemIds,
            Action<string, string> logError)
        {
            var priceMultiplier = _staticArchipelago.SlotData.ToolPriceMultiplier;

            string str = null;
            if (!string.IsNullOrWhiteSpace(arguments))
            {
                var dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(arguments);
                if (dataOrErrorItem.HasTypeId("(T)"))
                {
                    return ItemQueryResolver.Helpers.ErrorResult(key, arguments, logError, "can't filter for ID '" + arguments + "' because that isn't a tool item ID");
                }
                str = dataOrErrorItem.ItemId;
            }
            var itemQueryResultList = new List<ItemQueryResult>();
            foreach (var keyValuePair in Game1.toolData)
            {
                var key1 = keyValuePair.Key;
                var tool = keyValuePair.Value;
                if (str != null && key1 != str)
                {
                    continue;
                }
                var toolUpgradeData = ShopBuilder.GetToolUpgradeData(tool, Game1.player);
                if (toolUpgradeData == null)
                {
                    continue;
                }
                var obj = ItemRegistry.Create("(T)" + key1);
                var price = toolUpgradeData.Price > -1 ? toolUpgradeData.Price : Math.Max(0, obj.salePrice());
                price = (int)Math.Round(price * priceMultiplier);
                var tradeItemAmount = toolUpgradeData.TradeItemAmount;
                tradeItemAmount = (int)Math.Round(tradeItemAmount * priceMultiplier);
                var itemQueryResult = new ItemQueryResult(obj)
                {
                    OverrideBasePrice = price,
                    OverrideShopAvailableStock = 1,
                    OverrideTradeItemId = toolUpgradeData.TradeItemId,
                    OverrideTradeItemAmount = tradeItemAmount,
                };
                itemQueryResultList.Add(itemQueryResult);
            }
            return itemQueryResultList;
        }
    }
}
