﻿using StardewArchipelago.Constants.Locations;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Shops;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;
using StardewArchipelago.Archipelago.ApworldData;

namespace StardewArchipelago.Locations.ShopStockModifiers
{
    public class CookingRecipePurchaseStockModifier : ShopStockModifier
    {
        public CookingRecipePurchaseStockModifier(ILogger logger, IModHelper helper, StardewArchipelagoClient archipelago, StardewItemManager stardewItemManager) : base(logger, helper, archipelago, stardewItemManager)
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

                    foreach (var (shopId, shopData) in shopsData)
                    {
                        RemoveFriendshipRecipesFromShops(shopId, shopData);
                        ReplaceCookingRecipesWithChefsanityChecks(shopId, shopData);
                    }
                },
                AssetEditPriority.Late
            );
        }

        private void ReplaceCookingRecipesWithChefsanityChecks(string shopId, ShopData shopData)
        {
            if (!_archipelago.SlotData.Chefsanity.HasFlag(Chefsanity.Purchases))
            {
                return;
            }

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

        private void RemoveFriendshipRecipesFromShops(string shopId, ShopData shopData)
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
                if (!_archipelago.LocationExists(location))
                {
                    continue;
                }

                if (!_archipelago.DataPackageCache.GetLocation(location).LocationTags.Contains(LocationTag.CHEFSANITY_PURCHASE))
                {
                    shopData.Items.RemoveAt(i);
                }
            }
        }
    }
}
