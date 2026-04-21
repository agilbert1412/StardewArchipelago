using System;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles.Gacha
{
    public class GachaRoller
    {
        public const int COMMON_PRICE = 50;
        public const int RARE_PRICE = 500;
        public const int LEGENDARY_PRICE = 5000;

        private readonly int _bundlePrice;
        private readonly Random _random;

        public GachaRoller(int bundlePrice, Random random = null)
        {
            _bundlePrice = bundlePrice;
            if (random == null)
            {
                random = new Random();
            }
            _random = random;
        }

        public int RollTier(int investment)
        {
            for (var tier = 5; tier >= 1; tier--)
            {
                var bundlePrice = GetBundleOddsPriceForTier(tier);
                var chancePerDollar = 1.0 / bundlePrice;
                var chanceFailPerDollar = 1.0 - chancePerDollar;
                var chanceFailEveryDollar = Math.Pow(chanceFailPerDollar, investment);
                var chanceSuccess = 1.0 - chanceFailEveryDollar;
                var roll = _random.NextDouble();
                if (roll < chanceSuccess)
                {
                    return tier;
                }
            }

            return 0;
        }

        private int GetBundleOddsPriceForTier(int tier)
        {
            switch (tier)
            {
                case 5:
                    return (int)Math.Round(_bundlePrice * 1.5);
                case 4:
                    return (int)Math.Round(_bundlePrice * 0.75);
                case 3:
                    return (int)Math.Round(_bundlePrice * 0.5);
                case 2:
                    return (int)Math.Round(_bundlePrice * 0.25);
                case 1:
                    return (int)Math.Round(_bundlePrice * 0.05);
                default:
                    throw new ArgumentException();
            }
        }
    }
}
