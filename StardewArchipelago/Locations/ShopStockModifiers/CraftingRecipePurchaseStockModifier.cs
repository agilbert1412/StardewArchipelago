﻿using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants.Locations;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Shops;

namespace StardewArchipelago.Locations.ShopStockModifiers
{
    public class CraftingRecipePurchaseStockModifier : ShopStockModifier
    {
        public CraftingRecipePurchaseStockModifier(IMonitor monitor, IModHelper helper, ArchipelagoClient archipelago, StardewItemManager stardewItemManager) : base(monitor, helper, archipelago, stardewItemManager)
        {
        }

        public override void OnShopStockRequested(object sender, AssetRequestedEventArgs e)
        {
            if (_archipelago.SlotData.Craftsanity == Craftsanity.None)
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

                    foreach (var (shopId, shopData) in shopsData)
                    {
                        ReplaceCraftingRecipesWithCraftsanityChecks(shopId, shopData);
                    }
                },
                AssetEditPriority.Late
            );
        }

        private void ReplaceCraftingRecipesWithCraftsanityChecks(string shopId, ShopData shopData)
        {
            for (var i = shopData.Items.Count - 1; i >= 0; i--)
            {
                var item = shopData.Items[i];

                if (!item.IsRecipe)
                {
                    continue;
                }

                var stardewItem = _stardewItemManager.GetItemByQualifiedId(item.ItemId);
                if (!CraftingRecipe.craftingRecipes.ContainsKey(stardewItem.Name))
                {
                    continue;
                }

                var location = $"{stardewItem.Name}{Suffix.CRAFTSANITY}";
                var apShopItem = CreateArchipelagoLocation(item, location);
                shopData.Items.RemoveAt(i);
                shopData.Items.Insert(i, apShopItem);
            }
        }
    }
}
