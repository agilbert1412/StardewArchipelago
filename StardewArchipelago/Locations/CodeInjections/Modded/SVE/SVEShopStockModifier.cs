using System;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants.Modded;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Shops;

namespace StardewArchipelago.Locations.ShopStockModifiers
{
    public class SVEShopStockModifier : ShopStockModifier
    {
        public SVEShopStockModifier(IMonitor monitor, IModHelper helper, ArchipelagoClient archipelago, StardewItemManager stardewItemManager) : base(monitor, helper, archipelago, stardewItemManager)
        {
            _monitor = monitor;
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
                    var alesiaShop = shopsData["FlashShifter.StardewValleyExpandedCP_AlesiaVendor"];
                    var isaacShop = shopsData["FlashShifter.StardewValleyExpandedCP_IsaacVendor"];
                    ReplaceAlesiaShopWithChecks(alesiaShop);
                    ReplaceIsaacWeaponsWithChecks(isaacShop);
                },
                AssetEditPriority.Late
            );
        }

        private void ReplaceAlesiaShopWithChecks(ShopData shopData)
        {
            const string daggerLocationName = "Tempered Galaxy Dagger";
            var itemsData = DataLoader.Objects(Game1.content);
            for (var i = shopData.Items.Count - 1; i >= 0; i--)
            {
                var item = shopData.Items[i];
                if (!item.Id.Equals(ModQualifiedItemIds.TEMPERED_DAGGER, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                var apShopItem = CreateArchipelagoLocation(item, daggerLocationName);
                shopData.Items.RemoveAt(i);
                shopData.Items.Insert(i, apShopItem);
            }
        }

        private void ReplaceIsaacWeaponsWithChecks(ShopData shopData)
        {
            const string swordLocationName = "Tempered Galaxy Sword";
            const string hammerLocationName = "Tempered Galaxy Hammer";
            var itemsData = DataLoader.Objects(Game1.content);
            for (var i = shopData.Items.Count - 1; i >= 0; i--)
            {
                var item = shopData.Items[i];
                if (item.Id.Equals(ModQualifiedItemIds.TEMPERED_SWORD, StringComparison.InvariantCultureIgnoreCase))
                {
                    var apShopItem = CreateArchipelagoLocation(item, swordLocationName);
                    shopData.Items.RemoveAt(i);
                    shopData.Items.Insert(i, apShopItem);
                }
                else if (item.Id.Equals(ModQualifiedItemIds.TEMPERED_HAMMER, StringComparison.InvariantCultureIgnoreCase))
                {
                    var apShopItem = CreateArchipelagoLocation(item, hammerLocationName);
                    shopData.Items.RemoveAt(i);
                    shopData.Items.Insert(i, apShopItem);
                }
            }
        }
    }
}

