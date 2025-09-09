using System.Collections.Generic;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.GameData.Shops;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants;
using StardewArchipelago.Constants.Locations;

namespace StardewArchipelago.Locations.ShopStockModifiers
{
    public class QiGemShopStockModifier : ShopStockModifier
    {
        private const string HorseFluteName = "Horse Flute";
        private const string StocklistName = "Pierre's Missing Stocklist";
        private const string MiniShippingBinName = "Mini-Shipping Bin";
        private const string ExoticDoubleBedName = "Exotic Double Bed";
        private const string GoldenEggName = "Golden Egg";

        public QiGemShopStockModifier(ILogger logger, IModHelper helper, StardewArchipelagoClient archipelago, StardewItemManager stardewItemManager) : base(logger, helper, archipelago, stardewItemManager)
        {
            _logger = logger;
            _helper = helper;
            _archipelago = archipelago;
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
                var qiGemShop = shopsData["QiGemShop"];
                ReplaceEndgameLocationsWithChecks(qiGemShop);
            },
                AssetEditPriority.Late
            );
        }

        private void ReplaceEndgameLocationsWithChecks(ShopData shopData)
        {
            if (!_archipelago.SlotData.IncludeEndgameLocations)
            {
                return;
            }

            var itemsToReplace = new Dictionary<string, string>()
            {
                { QualifiedItemIds.HORSE_FLUTE, HorseFluteName },
                { QualifiedItemIds.PIERRE_MISSING_STOCKLIST, StocklistName },
                { "TownKey", APItem.KEY_TO_THE_TOWN },
                { QualifiedItemIds.MINI_SHIPPING_BIN, MiniShippingBinName }, // ???
                { QualifiedItemIds.EXOTIC_DOUBLE_BED, ExoticDoubleBedName },
                { QualifiedItemIds.GOLDEN_EGG, GoldenEggName },
            };

            for (var i = shopData.Items.Count - 1; i >= 0; i--)
            {
                var item = shopData.Items[i];
                if (!itemsToReplace.ContainsKey(item.Id))
                {
                    continue;
                }

                var itemToReplace = itemsToReplace[item.Id];
                var locationName = $"{Prefix.PURCHASE}{itemToReplace}";

                // TODO: Replace with a slot data check for allowed filler items, maybe?
                // item.Condition = GameStateConditionProvider.CreateHasReceivedItemCondition(itemToReplace);
                // shopData.Items.RemoveAt(i);

                var apShopItem = CreateArchipelagoLocation(item, locationName);
                shopData.Items.Insert(i, apShopItem);
            }
        }
    }
}
