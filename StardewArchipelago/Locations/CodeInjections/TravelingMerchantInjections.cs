using System;
using System.Collections.Generic;
using System.Linq;
using StardewArchipelago.Archipelago;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;

namespace StardewArchipelago.Locations.CodeInjections
{
    public class TravelingMerchantInjections
    {
        private const double BASE_STOCK = 0.1;
        private const double STOCK_AMOUNT_PER_UPGRADE = 0.15;
        private const double BASE_PRICE = 1.4;
        private const double DISCOUNT_PER_UPGRADE = 0.1;
        private const string AP_MERCHANT_DAYS = "Traveling Merchant: {0}"; // 7, One for each day
        private const string AP_MERCHANT_STOCK = "Traveling Merchant Stock Size"; // 10% base size, 6 upgrades of 15% each
        private const string AP_MERCHANT_DISCOUNT = "Traveling Merchant Discount"; // Base Price 140%, 8 x 10% discount
        private const string AP_MERCHANT_LOCATION = "Traveling Merchant {0} Item {1}";

        private static readonly string[] _days = new[]
            { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

        private static IMonitor _monitor;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(IMonitor monitor, IModHelper modHelper, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _monitor = monitor;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }
        
        public static void DayUpdate_IsTravelingMerchantDay_Postfix(Forest __instance, int dayOfMonth)
        {
            try
            {
                if (IsTravelingMerchantDay(dayOfMonth, out _))
                {
                    __instance.travelingMerchantDay = true;
                    __instance.travelingMerchantBounds.Add(new Microsoft.Xna.Framework.Rectangle(1472, 640, 492, 116));
                    __instance.travelingMerchantBounds.Add(new Microsoft.Xna.Framework.Rectangle(1652, 744, 76, 48));
                    __instance.travelingMerchantBounds.Add(new Microsoft.Xna.Framework.Rectangle(1812, 744, 104, 48));
                    foreach (var travelingMerchantBound in __instance.travelingMerchantBounds)
                    {
                        Utility.clearObjectsInArea(travelingMerchantBound, __instance);
                    }

                    if (Game1.IsMasterGame && Game1.netWorldState.Value.VisitsUntilY1Guarantee >= 0 && (dayOfMonth % 7 % 5 != 0))
                    {
                        --Game1.netWorldState.Value.VisitsUntilY1Guarantee;
                    }
                }
                else
                {
                    __instance.travelingMerchantBounds.Clear();
                    __instance.travelingMerchantDay = false;
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(DayUpdate_IsTravelingMerchantDay_Postfix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }
        
        public static void SetUpShopOwner_TravelingMerchantApFlair_Postfix(ShopMenu __instance, string who)
        {
            try
            {
                if (who != "Traveler" || !IsTravelingMerchantDay(Game1.dayOfMonth, out var playerName))
                {
                    return;
                }

                var day = GetDayOfWeekName(Game1.dayOfMonth);
                var text = $"{playerName} recommended that I visit the valley on {day}s. Take a look at my wares!";
                __instance.potraitPersonDialogue = Game1.parseText(text, Game1.dialogueFont, 304);
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(SetUpShopOwner_TravelingMerchantApFlair_Postfix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }
        
        public static void GenerateLocalTravelingMerchantStock_APStock_Postfix(int seed, ref Dictionary<ISalable, int[]> __result)
        {
            try
            {
                var priceUpgrades = _archipelago.GetReceivedItemCount(AP_MERCHANT_DISCOUNT);
                var priceMultiplier = BASE_PRICE - (priceUpgrades * DISCOUNT_PER_UPGRADE);
                var stockUpgrades = _archipelago.GetReceivedItemCount(AP_MERCHANT_STOCK);
                var chanceForItemToRemain = BASE_STOCK + (stockUpgrades * STOCK_AMOUNT_PER_UPGRADE);

                var random = new Random(seed);

                var itemsToRemove = new List<ISalable>();

                foreach (var (item, prices) in __result)
                {
                    if (random.NextDouble() > chanceForItemToRemain)
                    {
                        itemsToRemove.Add(item);
                    }

                    prices[0] = ModifyPrice(prices[0], priceMultiplier);
                }

                AddApStock(ref __result, random, priceMultiplier);

                foreach (var itemToRemove in itemsToRemove)
                {
                    __result.Remove(itemToRemove);
                }

                return;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(GenerateLocalTravelingMerchantStock_APStock_Postfix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }

        private static int ModifyPrice(int price, double priceMultiplier)
        {
            return (int)Math.Round(price * priceMultiplier, MidpointRounding.ToEven);
        }

        private static bool IsTravelingMerchantDay(int dayOfMonth, out string playerName)
        {
            var dayOfWeek = GetDayOfWeekName(dayOfMonth);
            var requiredAPItemToSeeMerchantToday = string.Format(AP_MERCHANT_DAYS, dayOfWeek);
            return _archipelago.HasReceivedItem(requiredAPItemToSeeMerchantToday, out playerName);
        }

        private static void AddApStock(ref Dictionary<ISalable, int[]> currentStock, Random random, double priceMultiplier)
        {
            var dayOfWeek = GetDayOfWeekName(Game1.dayOfMonth);
            var apItems = new List<string>();
            for (var i = 1; i < 4; i++)
            {
                var apLocationName = string.Format(AP_MERCHANT_LOCATION, dayOfWeek, i);

                if (_locationChecker.IsLocationChecked(apLocationName))
                {
                    continue;
                }

                apItems.Add(apLocationName);
            }

            if (!apItems.Any())
            {
                return;
            }

            var chosenApItem = apItems[random.Next(0, apItems.Count)];

            var scamName = _merchantApItemNames[random.Next(0, _merchantApItemNames.Length)];
            var apLocation =
                new PurchaseableArchipelagoLocation(scamName, chosenApItem, _modHelper, _locationChecker, _archipelago);
            var price = ModifyPrice(_merchantPrices[random.Next(0, _merchantPrices.Length)], priceMultiplier);

            currentStock.Add(apLocation, new []{price, 1});
        }

        private static string GetDayOfWeekName(int day)
        {
            var dayOfWeek = day % 7;
            switch (dayOfWeek)
            {
                case 0:
                    return "Sunday";
                case 1:
                    return "Monday";
                case 2:
                    return "Tuesday";
                case 3:
                    return "Wednesday";
                case 4:
                    return "Thursday";
                case 5:
                    return "Friday";
                case 6:
                    return "Saturday";
            }

            throw new ArgumentException($"Invalid day: {day}");
        }

        private static readonly int[] _merchantPrices = new[]
        {
            250,
            500,
            1000,
            2000,
            5000,
            10000,
        };

        private static readonly string[] _merchantApItemNames = new[]
        {
            "Snake Oil",
            "Glass of time",
            "Orb of Slope Detection",
            "Dagger of Time",
            "Harpy's Quill",
            "Oil of Slipperiness",
            "Gauntlets of Touch",
            "Disk of Enlargement",
            "Torch of Night Vision",
            "Potion of Hydration",
            "Viper Liquid",
            "Fire Distinguisher",
            "Bag of Holding",
            "Stone of Weather Detection",
            "Tigerbane Stone",
            "Eyepatch of 2D Vision",
            "Mirror of Reflection",
            "Potion of Courage",
            "Moveable Rod",
            "Orb of shattering",
            "Spectacles of Darkness",
            "Leash of Holding",
            "Dagger of Desperation",
            "Dihydrogen Monoxide Grenade",
            "Pan of Frying",
            "Pan of Drying",
            "Ringing Ring",
        };
    }
}
