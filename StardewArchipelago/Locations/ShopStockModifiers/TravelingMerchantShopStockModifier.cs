using System;
using System.Collections.Generic;
using System.Linq;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants;
using StardewArchipelago.Serialization;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData;
using StardewValley.GameData.Shops;

namespace StardewArchipelago.Locations.ShopStockModifiers
{
    public class TravelingMerchantShopStockModifier : ShopStockModifier
    {
        public TravelingMerchantShopStockModifier(IMonitor monitor, IModHelper modHelper, ArchipelagoClient archipelago, StardewItemManager stardewItemManager) : base(monitor, modHelper, archipelago, stardewItemManager)
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
                    var cartShopData = shopsData["Traveler"];
                    AddStockSizeConditions(cartShopData);
                    AddMetalDetectorItems(cartShopData);
                    AddChecks(cartShopData);
                    AdjustPrices(cartShopData);
                },
                AssetEditPriority.Late
            );
        }

        private void AddStockSizeConditions(ShopData cartShopData)
        {
            foreach (var shopItemData in cartShopData.Items)
            {
                var initialPerItemCondition = shopItemData.PerItemCondition;
                var conditions = initialPerItemCondition.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
                if (shopItemData.Id == "RandomObjects")
                {
                    conditions.Add($"{GameStateConditionProvider.CART_RANDOM_ITEM_STOCK_CHANCE} cart_random_items 1.0");
                }
                else
                {
                    conditions.Add($"{GameStateConditionProvider.CART_EXCLUSIVE_ITEM_STOCK_CHANCE} cart_exclusive_items 1.0");
                }
                shopItemData.PerItemCondition = string.Join(", ", conditions);
            }
        }

        private void AddMetalDetectorItems(ShopData cartShopData)
        {
            var metalDetector = new ShopItemData()
            {
                Id = IDProvider.METAL_DETECTOR_ITEMS,
                ItemId = IDProvider.METAL_DETECTOR_ITEMS,
                AvailableStock = 1,
                IsRecipe = false,
                MaxItems = 10,
                AvoidRepeat = true,
            };
            cartShopData.Items.Add(metalDetector);
        }

        private void AddChecks(ShopData cartShopData)
        {
            var metalDetector = new ShopItemData()
            {
                Id = IDProvider.TRAVELING_CART_DAILY_CHECK,
                ItemId = IDProvider.TRAVELING_CART_DAILY_CHECK,
                AvailableStock = 1,
                IsRecipe = false,
                MaxItems = 1,
            };
            cartShopData.Items.Add(metalDetector);
        }

        private void AdjustPrices(ShopData cartShopData)
        {
            cartShopData.PriceModifierMode = QuantityModifier.QuantityModifierMode.Stack;
            //cartShopData.PriceModifiers.Add(new QuantityModifier()
            //{
            //    Id = IDProvider.TRAVELING_CART_DISCOUNTS,
            //    Amount = 
            //});
            //foreach (var itemShopData in cartShopData.Items)
            //{
            //    if (itemShopData.Price > 0)
            //    {

            //    }
            //}
            throw new NotImplementedException();
        }
    }
}
