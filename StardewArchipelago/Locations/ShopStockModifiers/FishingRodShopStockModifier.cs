
using System;
using StardewArchipelago.Archipelago;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Shops;
using StardewValley.GameData.Tools;

namespace StardewArchipelago.Locations.ShopStockModifiers
{
    public class FishingRodShopStockModifier : ShopStockModifier
    {
        public FishingRodShopStockModifier(IMonitor monitor, IModHelper helper, ArchipelagoClient archipelago) : base(monitor, helper, archipelago)
        {
        }

        public override void OnShopStockRequested(object sender, AssetRequestedEventArgs e)
        {
            if (!_archipelago.SlotData.ToolProgression.HasFlag(ToolProgression.Progressive))
            {
                return;
            }

            if (!AssetIsShops(e))
            {
                return;
            }

            e.Edit(asset =>
                {
                    var shopsData = asset.AsDictionary<string, ShopData>().Data;
                    var willyShopData = shopsData["FishShop"];
                    ReplaceFishingRodsWithToolsChecks(willyShopData);
                },
                AssetEditPriority.Late
            );
        }

        private void ReplaceFishingRodsWithToolsChecks(ShopData fishShopData)
        {
            var toolsData = DataLoader.Tools(Game1.content);
            for (var i = fishShopData.Items.Count - 1; i >= 0; i--)
            {
                var item = fishShopData.Items[i];
                if (!toolsData.ContainsKey(item.ItemId))
                {
                    continue;
                }

                var toolData = toolsData[item.ItemId];
                var existingConditions = item.Condition.Split(",", StringSplitOptions.RemoveEmptyEntries);
                var apShopitem = CreateEquivalentArchipelagoLocation(item, toolData);
                fishShopData.Items.Add(apShopitem);
                AddArchipelagoCondition(item, toolData, existingConditions);
            }
        }

        private ShopItemData CreateEquivalentArchipelagoLocation(ShopItemData item, ToolData toolData)
        {
            var location = $"Purchase {toolData.Name}";
            return CreateArchipelagoLocation(item, location);
        }

        private void AddArchipelagoCondition(ShopItemData shopItem, ToolData toolData, string[] existingConditions)
        {
            var amount = toolData.UpgradeLevel;
            // For some reason, the training rod is 1 and the bamboo pole is 0
            if (toolData.UpgradeLevel <= 1)
            {
                amount = 1 - toolData.UpgradeLevel;
            }

            AddArchipelagoCondition(shopItem, existingConditions, "Progressive Fishing Rod", amount);
        }
    }
}
