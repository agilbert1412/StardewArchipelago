using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Shops;
using StardewValley.GameData.Tools;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago;
using System;
using StardewArchipelago.Archipelago.SlotData;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;

namespace StardewArchipelago.Locations.ShopStockModifiers
{
    public class FishingRodShopStockModifier : ShopStockModifier
    {

        public FishingRodShopStockModifier(ILogger logger, IModHelper helper, StardewArchipelagoClient archipelago, StardewItemManager stardewItemManager) : base(logger, helper, archipelago, stardewItemManager)
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
                    var willyShopData = shopsData["FishShop"];
                    MakeFishingRodsCheaper(willyShopData);
                    ReplaceFishingRodsWithToolsChecks(willyShopData);
                },
                AssetEditPriority.Late
            );
        }

        private void ReplaceFishingRodsWithToolsChecks(ShopData fishShopData)
        {
            if (!_archipelago.SlotData.ToolProgression.HasFlag(ToolProgression.Progressive))
            {
                return;
            }

            var toolsData = DataLoader.Tools(Game1.content);
            for (var i = fishShopData.Items.Count - 1; i >= 0; i--)
            {
                var item = fishShopData.Items[i];
                var unqualifiedId = QualifiedItemIds.UnqualifyId(item.ItemId);
                if (unqualifiedId == null || !toolsData.ContainsKey(unqualifiedId))
                {
                    continue;
                }

                var toolData = toolsData[unqualifiedId];
                CreateEquivalentArchipelagoLocation(fishShopData, item, toolData);
                ReplaceWithArchipelagoCondition(item, toolData);
            }
        }

        private void CreateEquivalentArchipelagoLocation(ShopData fishShopData, ShopItemData item, ToolData toolData)
        {
            var location = $"Purchase {toolData.Name}";
            if (_archipelago.LocationExists(location))
            {
                fishShopData.Items.Insert(fishShopData.Items.IndexOf(item), CreateArchipelagoLocation(item, location));
            }
        }

        private void ReplaceWithArchipelagoCondition(ShopItemData shopItem, ToolData toolData)
        {
            var amount = toolData.UpgradeLevel + 1;
            // For some reason, the training rod is 1 and the bamboo pole is 0
            if (toolData.UpgradeLevel <= 1)
            {
                amount = 2 - toolData.UpgradeLevel;
            }

            var toolName = toolData.Name;
            if (toolData.ClassName == "FishingRod")
            {
                toolName = "Fishing Rod";
            }

            ReplaceWithArchipelagoCondition(shopItem, $"Progressive {toolName}", amount);
        }

        private void MakeFishingRodsCheaper(ShopData willyShopData)
        {
            var priceMultiplier = _archipelago.SlotData.ToolPriceMultiplier;
            if (Math.Abs(priceMultiplier - 1.0) < 0.01)
            {
                return;
            }

            var toolsData = DataLoader.Tools(Game1.content);
            foreach (var toolShopItem in willyShopData.Items)
            {
                var unqualifiedId = QualifiedItemIds.UnqualifyId(toolShopItem.ItemId);
                if (unqualifiedId == null || !toolsData.ContainsKey(unqualifiedId))
                {
                    continue;
                }

                toolShopItem.Price = (int)Math.Round(toolShopItem.Price * priceMultiplier);
                toolShopItem.TradeItemAmount = (int)Math.Round(toolShopItem.TradeItemAmount * priceMultiplier);
            }
        }
    }
}
