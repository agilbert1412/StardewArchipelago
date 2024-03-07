using System;
using System.Collections.Generic;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.Locations.CodeInjections.Vanilla;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Shops;
using Object = StardewValley.Object;

namespace StardewArchipelago.GameModifications.CodeInjections
{
    public class AnimalShopStockModifier : ShopStockModifier
    {
        public AnimalShopStockModifier(IMonitor monitor, IModHelper helper, ArchipelagoClient archipelago) : base(monitor, helper, archipelago)
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
                    var marnieShopData = shopsData["AnimalShop"];
                    foreach (var shopItemData in marnieShopData.Items)
                    {
                        if (shopItemData.Id != QualifiedItemIds.GOLDEN_EGG)
                        {
                            continue;
                        }

                        shopItemData.Condition = GameStateConditionProvider.CreateHasReceivedItemCondition("Golden Egg");
                    }
                },
                AssetEditPriority.Late
            );
        }
    }
}
