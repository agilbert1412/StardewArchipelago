using System;

namespace StardewArchipelago.PriceMath
{
    public class BackpackPriceCalculator
    {
        private const int SMALL_PRICE = 400;
        private const int LARGE_PRICE = 2000;
        private const int DELUXE_PRICE = 10000;
        private const int PREMIUM_PRICE = 50000;
        private static readonly int[] PRICES_PER_TIER = new int[] { SMALL_PRICE, LARGE_PRICE, DELUXE_PRICE, PREMIUM_PRICE };

        public BackpackPriceCalculator() { }

        public int GetPrice(int backpackTier, int backpackNumber, int numberOfBackpacksInTier)
        {
            /*
             * Algorithm from @Razvogor
             *  c = dst
                x = total price of all items
                n = number of items

                x = 0 + c + 2c + 3c + ... + nc
                x = sum[k=0, n](k*c) = c * sum[k=0, n](k) = c * (n * (n+1)/2)
                => c = 2*x/(n*(n+1))
             */

            double totalPrice = PRICES_PER_TIER[backpackTier];
            var howManyTimesFirstPrice = numberOfBackpacksInTier * (numberOfBackpacksInTier + 1) / 2;
            var priceDeltaPerBackpack = totalPrice / howManyTimesFirstPrice;
            return (int)Math.Round(backpackNumber * priceDeltaPerBackpack);
        }
    }
}
