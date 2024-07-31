﻿using KaitoKid.ArchipelagoUtilities.Net.Client;
using StardewArchipelago.Constants.Locations;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Shops;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;

namespace StardewArchipelago.Locations.ShopStockModifiers
{
    public class CookingRecipePurchaseStockModifier : ShopStockModifier
    {
        public CookingRecipePurchaseStockModifier(ILogger logger, IModHelper helper, ArchipelagoClient archipelago, StardewItemManager stardewItemManager) : base(logger, helper, archipelago, stardewItemManager)
        {
        }

        public override void OnShopStockRequested(object sender, AssetRequestedEventArgs e)
        {
            if (!_archipelago.SlotData.Chefsanity.HasFlag(Chefsanity.Purchases))
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
                        ReplaceCookingRecipesWithChefsanityChecks(shopId, shopData);
                    }
                },
                AssetEditPriority.Late
            );
        }

        private void ReplaceCookingRecipesWithChefsanityChecks(string shopId, ShopData shopData)
        {
            for (var i = shopData.Items.Count - 1; i >= 0; i--)
            {
                var item = shopData.Items[i];

                if (!item.IsRecipe)
                {
                    continue;
                }

                var stardewItem = _stardewItemManager.GetItemByQualifiedId(item.ItemId) ?? _stardewItemManager.GetItemByQualifiedId(QualifiedItemIds.QualifiedObjectId(item.ItemId));
                var location = $"{stardewItem.Name}{Suffix.CHEFSANITY}";
                if (!CraftingRecipe.cookingRecipes.ContainsKey(stardewItem.Name) || !_archipelago.LocationExists(location))
                {
                    continue;
                }

                var apShopItem = CreateArchipelagoLocation(item, location);
                shopData.Items.RemoveAt(i);
                shopData.Items.Insert(i, apShopItem);
            }
        }
    }
}
