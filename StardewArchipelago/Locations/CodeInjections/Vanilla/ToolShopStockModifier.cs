﻿using System;
using System.Collections.Generic;
using Archipelago.MultiClient.Net.Models;
using System.Linq;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants;
using StardewArchipelago.Constants.Locations;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.Items.Unlocks;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Shops;
using StardewValley.Internal;
using StardewValley.Menus;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public class ToolShopStockModifier : ShopStockModifier
    {
        public ToolShopStockModifier(IMonitor monitor, IModHelper modHelper, ArchipelagoClient archipelago) : base(monitor, modHelper, archipelago)
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
                    var requiredItem = $"{Prefix.PROGRESSIVE}{toolName}";
                    var requiredAmount = i - 1;
                    var id = $"{IDProvider.PURCHASEABLE_AP_LOCATION} {locationName}";
                    var toolId = $"{Materials.MaterialNames[i]}{toolName.Replace(" ", "")}";

                    var toolData = toolsData[toolId];
                    
                    var apShopItem = new ShopItemData()
                    {
                        Id = id,
                        ItemId = id,
                        AvailableStock = 1,
                        IsRecipe = false,
                        Price = toolData.SalePrice,
                        TradeItemId = GetToolUpgradeConventionalTradeItem(i),
                        TradeItemAmount = 5,
                        Condition = GameStateConditionProvider.CreateHasReceivedItemCondition(requiredItem, requiredAmount)
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

            var toolsData = Game1.toolData;
            toolShopData.Items.Clear();
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