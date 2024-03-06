using System;
using System.Collections.Generic;
using System.Linq;
using Archipelago.MultiClient.Net.Models;
using Force.DeepCloner;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.GameModifications;
using StardewArchipelago.Serialization;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData;
using StardewValley.GameData.Shops;
using StardewValley.Locations;
using StardewValley.Menus;
using xTile.Dimensions;
using Object = StardewValley.Object;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public class TravelingMerchantShopStockModifier : ShopStockModifier
    {
        private const double BASE_STOCK = 0.1;
        private const double STOCK_AMOUNT_PER_UPGRADE_FOR_EXCLUSIVE_ITEMS = 0.15;
        private const double STOCK_AMOUNT_PER_UPGRADE_FOR_RANDOM_ITEMS = 0.1;
        private const double STOCK_AMOUNT_PER_ONE_PERCENT_CHECKS_FOR_RANDOM_ITEMS = 0.01;
        private const double STOCK_AMOUNT_REDUCTION_PER_PURCHASE = 0.1;

        // 0.1 + (6 * 0.1) + (100 * 0.05)
        // 10% + 60% + 100% = 6

        private const double BASE_PRICE = 1.4;
        private const double DISCOUNT_PER_UPGRADE = 0.1;
        public const string AP_MERCHANT_DAYS = "Traveling Merchant: {0}"; // 7, One for each day
        private const string AP_MERCHANT_STOCK = "Traveling Merchant Stock Size"; // 10% base size, 6 upgrades of 15% each
        private const string AP_MERCHANT_DISCOUNT = "Traveling Merchant Discount"; // Base Price 140%, 8 x 10% discount
        private const string AP_MERCHANT_LOCATION = "Traveling Merchant {0} Item {1}";
        private const string AP_METAL_DETECTOR = "Traveling Merchant Metal Detector"; // Base Price 140%, 8 x 10% discount
        private const string AP_WEDDING_RING_RECIPE = "Wedding Ring Recipe";

        private static readonly string[] _exclusiveStock = new[]
            { "Rare Seed", "Rarecrow", "Coffee Bean", "Wedding Ring Recipe" };
        
        private static LocationChecker _locationChecker;
        private static ArchipelagoStateDto _archipelagoState;

        private static Dictionary<ISalable, string> _flairOverride;

        public TravelingMerchantShopStockModifier(IMonitor monitor, IModHelper modHelper, ArchipelagoClient archipelago, LocationChecker locationChecker, ArchipelagoStateDto archipelagoState) : base(monitor, modHelper, archipelago)
        {
            _locationChecker = locationChecker;
            _archipelagoState = archipelagoState;
            _flairOverride = new Dictionary<ISalable, string>();
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
                    AddStockSizeTriggers(cartShopData);
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

        private void AddStockSizeTriggers(ShopData cartShopData)
        {
            var randomObjects = cartShopData.Items.First(x => x.Id == "RandomObjects");
            var randomObjectsWithTrigger = randomObjects.DeepClone();
            randomObjectsWithTrigger.Id = "RandomObjectsWithArchipelagoTrigger";
            randomObjectsWithTrigger.ItemId = randomObjects.ItemId.Replace("RANDOM");
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
