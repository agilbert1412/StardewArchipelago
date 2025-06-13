using System;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Shops;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants;

namespace StardewArchipelago.Locations.ShopStockModifiers
{
    public class KrobusShopStockModifier : ShopStockModifier
    {
        public KrobusShopStockModifier(ILogger logger, IModHelper helper, StardewArchipelagoClient archipelago, StardewItemManager stardewItemManager) : base(logger, helper, archipelago, stardewItemManager)
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
                var krobusShop = shopsData["ShadowShop"];
                ReplaceStardropWithCheck(krobusShop);
                ReplaceReturnScepterWithCheck(krobusShop);
            },
                AssetEditPriority.Late
            );
        }

        private void ReplaceStardropWithCheck(ShopData shopData)
        {
            const string krobusStardropLocationName = "Krobus Stardrop";
            for (var i = shopData.Items.Count - 1; i >= 0; i--)
            {
                var item = shopData.Items[i];
                if (!item.Id.Equals(QualifiedItemIds.STARDROP, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                var apShopItem = CreateArchipelagoLocation(item, krobusStardropLocationName);
                shopData.Items.RemoveAt(i);
                shopData.Items.Insert(i, apShopItem);
            }
        }

        private void ReplaceReturnScepterWithCheck(ShopData shopData)
        {
            if (!_archipelago.SlotData.IncludeEndgameLocations)
            {
                return;
            }

            const string returnScepterLocationName = "Purchase Return Scepter";
            for (var i = shopData.Items.Count - 1; i >= 0; i--)
            {
                var item = shopData.Items[i];
                if (!item.Id.Equals(QualifiedItemIds.RETURN_SCEPTER, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                item.Condition = GameStateConditionProvider.CreateHasReceivedItemCondition("Return Scepter");
                // shopData.Items.RemoveAt(i);

                var apShopItem = CreateArchipelagoLocation(item, returnScepterLocationName);
                shopData.Items.Insert(i, apShopItem);
            }
        }
    }
}
