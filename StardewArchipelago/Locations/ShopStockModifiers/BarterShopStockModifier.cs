using System;
using System.Collections.Generic;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Shops;

namespace StardewArchipelago.Locations.ShopStockModifiers
{
    public abstract class BarterShopStockModifier : ShopStockModifier
    {
        public BarterShopStockModifier(ILogger logger, IModHelper helper, StardewArchipelagoClient archipelago, StardewItemManager stardewItemManager) : base(logger, helper, archipelago, stardewItemManager)
        {
        }

        protected ShopItemData CreateBarterItem(List<StardewObject> stardewObjects, StardewItem stardewItem, string condition = null, int overridePrice = 0, int offeredStock = 1, double priceReduction = 1, bool isSingle = false)
        {
            var barterItem = new ShopItemData();
            var itemPrice = overridePrice == 0 ? (int)(priceReduction * Math.Sqrt(stardewItem.SellPrice)) : (int)(priceReduction * Math.Sqrt(overridePrice));
            barterItem.Id = IDProvider.CreateId(stardewItem.Name.Replace(" ", "_"));
            barterItem.ItemId = stardewItem.Id;
            barterItem.AvailableStock = offeredStock;
            barterItem.IsRecipe = false;
            barterItem.AvoidRepeat = true;
            var random = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2 + stardewItem.GetHashCode());
            var chosenItemGroup = stardewObjects.Where(x => x.SellPrice > 0).ToArray();
            var chosenItem = chosenItemGroup[random.Next(chosenItemGroup.Length)];
            var sellPrice = (int)Math.Sqrt(chosenItem.SellPrice);
            var chosenItemExchangeRate = GetExchangeRate(itemPrice, sellPrice, isSingle);
            barterItem.TradeItemId = chosenItem.Id;
            barterItem.Price = 0;
            barterItem.MinStack = chosenItemExchangeRate[0];
            barterItem.TradeItemAmount = chosenItemExchangeRate[1];

            barterItem.Condition = condition;

            return barterItem;
        }

        protected void ReplaceCurrencyWithBarterGivenObjects(List<StardewObject> stardewObjects, ShopItemData item, int offeredStock = 1, double priceReduction = 1, bool isSingle = false)
        {
            var random = new Random((int)(Game1.stats.DaysPlayed / 28) + (int)(Game1.uniqueIDForThisGame / 2) + item.GetHashCode());
            var chosenItemGroup = stardewObjects.Where(x => x.SellPrice != 0).ToArray();
            var chosenItem = chosenItemGroup[random.Next(chosenItemGroup.Length)];
            var newPrice = (int)(priceReduction * Math.Sqrt(item.Price));
            var newSellPrice = (int)Math.Sqrt(chosenItem.SellPrice);
            var chosenItemExchangeRate = GetExchangeRate(newPrice, newSellPrice, isSingle);
            item.Price = 0;
            item.AvailableStock = offeredStock;
            item.TradeItemId = chosenItem.Id;
            item.TradeItemAmount = chosenItemExchangeRate[1];
            item.MinStack = chosenItemExchangeRate[0];
        }

        protected void ReplaceCurrencyWithBarterGivenObject(StardewObject stardewObject, ShopItemData item, int offeredStock = 1, double priceReduction = 1, bool isSingle = false)
        {
            var newPrice = (int)(priceReduction * Math.Sqrt(item.Price));
            var newSellPrice = (int)Math.Sqrt(stardewObject.SellPrice);
            var chosenItemExchangeRate = GetExchangeRate(newPrice, newSellPrice, isSingle);
            item.Price = 0;
            item.AvailableStock = offeredStock;
            item.TradeItemId = stardewObject.Id;
            item.TradeItemAmount = chosenItemExchangeRate[1];
            item.MinStack = chosenItemExchangeRate[0];
        }

        public int[] GetExchangeRate(int soldItemValue, int requestedItemValue, bool isSingle)
        {
            if (TryGetDirectExchangeRate(soldItemValue, requestedItemValue, out var exchangeRate))
            {
                return exchangeRate;
            }
            var greatestCommonDivisor = GreatestCommonDivisor(soldItemValue, requestedItemValue);
            var leastCommonMultiple = soldItemValue * requestedItemValue / greatestCommonDivisor;
            var soldItemCount = leastCommonMultiple / soldItemValue;
            var requestedItemCount = leastCommonMultiple / requestedItemValue;

            var lowestCount = 5; // This is for us to change if we want to move this value around easily in testing
            var finalCounts = MakeMinimalCountBelowGivenCount(soldItemCount, requestedItemCount, lowestCount);
            if (isSingle)
            {
                finalCounts = new int[2]{1, Math.Max(1, finalCounts[1]/finalCounts[0])};
            }
            return finalCounts;
        }

        private bool TryGetDirectExchangeRate(int soldItemValue, int requestedItemValue, out int[] exchangeRate)
        {
            exchangeRate = null;
            soldItemValue = Math.Max(soldItemValue, 1); // Item's value may be so low the discount makes the system round to zero.
            if (soldItemValue > requestedItemValue && soldItemValue % requestedItemValue == 0)
            {
                exchangeRate = new int[2] { 1, soldItemValue / requestedItemValue };
                return true;
            }
            if (soldItemValue <= requestedItemValue && requestedItemValue % soldItemValue == 0)
            {
                exchangeRate = new int[2] { requestedItemValue / soldItemValue, 1 };
                return true;
            }

            return false;
        }

        private int[] MakeMinimalCountBelowGivenCount(int soldItemCount, int requestedItemCount, int givenCount)
        {
            if (Math.Min(soldItemCount, requestedItemCount) > givenCount)
            {
                var closestCount = (int)Math.Pow(givenCount, (int)(Math.Log10(Math.Min(soldItemCount, requestedItemCount)) / Math.Log10(givenCount)));
                soldItemCount /= closestCount;
                requestedItemCount /= closestCount;
                var greatestCommonDivisor = GreatestCommonDivisor(soldItemCount, requestedItemCount); // Due to the rounding we may find the two aren't relatively prime anymore
                soldItemCount /= greatestCommonDivisor;
                requestedItemCount /= greatestCommonDivisor;
            }
            return new int[2] { soldItemCount, requestedItemCount };
        }

        private static int GreatestCommonDivisor(int firstValue, int secondValue) //Seemingly no basic method outside of BigInteger?
        {
            var largestValue = Math.Max(firstValue, secondValue);
            var lowestValue = Math.Min(firstValue, secondValue);
            var remainder = largestValue % lowestValue;
            if (remainder == 0)
            {
                return lowestValue;
            }
            while (remainder != 0)
            {
                largestValue = lowestValue;
                lowestValue = remainder;
                if (largestValue % lowestValue == 0)
                {
                    break;
                }
                remainder = largestValue % lowestValue;
            }
            return remainder;
        }
    }
}
