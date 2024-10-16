﻿using System;
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

namespace StardewArchipelago.Locations.ShopStockModifiers
{
    public class ToolShopStockModifier : ShopStockModifier
    {
        public ToolShopStockModifier(ILogger logger, IModHelper modHelper, StardewArchipelagoClient archipelago, StardewItemManager stardewItemManager) : base(logger, modHelper, archipelago, stardewItemManager)
        {
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
    }
}
