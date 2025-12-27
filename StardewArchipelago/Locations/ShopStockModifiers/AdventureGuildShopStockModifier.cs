using System;
using StardewArchipelago.Constants;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.GameData.Shops;
using KaitoKid.Utilities.Interfaces;
using StardewArchipelago.Archipelago;

namespace StardewArchipelago.Locations.ShopStockModifiers
{
    public class AdventureGuildShopStockModifier : ShopStockModifier
    {
        public AdventureGuildShopStockModifier(ILogger logger, IModHelper modHelper, StardewArchipelagoClient archipelago, StardewItemManager stardewItemManager) : base(logger, modHelper, archipelago, stardewItemManager)
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
                    var adventureShop = shopsData["AdventureShop"];
                    var adventureRecovery = shopsData["AdventureGuildRecovery"];
                    AddAllWeaponsWithReceivedConditions(adventureShop);
                    AddRandomWeaponRecoveries(adventureRecovery);
                },
                AssetEditPriority.Late
            );
        }

        private void AddAllWeaponsWithReceivedConditions(ShopData adventureShop)
        {
            for (var i = adventureShop.Items.Count - 1; i >= 0; i--)
            {
                var item = adventureShop.Items[i];
                if (!item.Id.StartsWith("(W)", StringComparison.InvariantCultureIgnoreCase) && !item.Id.StartsWith("(B)", StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                adventureShop.Items.RemoveAt(i);
            }

            var shopEquipments = new ShopItemData()
            {
                Id = IDProvider.ARCHIPELAGO_EQUIPMENTS,
                ItemId = $"{IDProvider.ARCHIPELAGO_EQUIPMENTS} {IDProvider.ARCHIPELAGO_EQUIPMENTS_SALE}",
                AvoidRepeat = true,
            };
            adventureShop.Items.Add(shopEquipments);
        }

        private void AddRandomWeaponRecoveries(ShopData adventureRecovery)
        {
            var shopEquipmentRecoveries = new ShopItemData()
            {
                Id = IDProvider.ARCHIPELAGO_EQUIPMENTS,
                ItemId = $"{IDProvider.ARCHIPELAGO_EQUIPMENTS} {IDProvider.ARCHIPELAGO_EQUIPMENTS_RECOVERY}",
                AvoidRepeat = true,
            };
            adventureRecovery.Items.Add(shopEquipmentRecoveries);
        }
    }
}
