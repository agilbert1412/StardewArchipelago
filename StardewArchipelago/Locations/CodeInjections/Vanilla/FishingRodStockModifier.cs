
using System;
using System.Collections.Generic;
using System.Linq;
using Force.DeepCloner;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Shops;
using StardewValley.GameData.Tools;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public class FishingRodStockModifier
    {
        private static IMonitor _monitor;
        private static IModHelper _helper;
        private static ArchipelagoClient _archipelago;

        public FishingRodStockModifier(IMonitor monitor, IModHelper helper, ArchipelagoClient archipelago)
        {
            _monitor = monitor;
            _helper = helper;
            _archipelago = archipelago;
        }

        public void OnFishingRodShopStockRequested(object sender, AssetRequestedEventArgs e)
        {
            if (!_archipelago.SlotData.ToolProgression.HasFlag(ToolProgression.Progressive))
            {
                return;
            }

            if (!e.NameWithoutLocale.IsEquivalentTo("Data/Shops"))
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
                if (existingConditions.Length > 1)
                {
                    continue;
                }

                AddArchipelagoCondition(item, toolData, existingConditions);
                var apShopitem = CreateEquivalentArchipelagoLocation(item, toolData);
                fishShopData.Items.Add(apShopitem);
            }
        }

        private static ShopItemData CreateEquivalentArchipelagoLocation(ShopItemData item, ToolData toolData)
        {
            var location = $"Purchase {toolData.Name}";
            var id = $"{IDProvider.PURCHASEABLE_AP_LOCATION} {location}";
            var apShopItem = item.DeepClone();
            apShopItem.Id = id;
            apShopItem.ItemId = id;
            apShopItem.AvailableStock = 1;
            return apShopItem;
        }

        private static void AddArchipelagoCondition(ShopItemData shopItem, ToolData toolData, string[] existingConditions)
        {
            var amount = toolData.UpgradeLevel;
            // For some reason, the training rod is 1 and the bamboo pole is 0
            if (toolData.UpgradeLevel <= 1)
            {
                amount = 1 - toolData.UpgradeLevel;
            }

            var apCondition = GameStateConditionProvider.CreateHasReceivedItemCondition("Progressive Fishing Rod", amount);
            var newConditions = new List<string>();
            newConditions.AddRange(existingConditions);
            newConditions.Add(apCondition);

            shopItem.Condition = string.Join(',', newConditions);
        }
    }
}
