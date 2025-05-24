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

        public int RollTierChatGpt(int investment)
        {
            if (investment <= 0)
                return 1; // Fail-safe

            var ratio = investment / (double)_bundlePrice;
            ratio = Math.Min(ratio, 1.5); // Cap at 1.5 for stability

            // Weight curve: low ratio favors Tier 1, high ratio boosts Tier 5
            var weights = new double[5];

            weights[0] = 1.5 - ratio;          // Tier 1
            weights[1] = 1.2 - 0.9 * ratio;     // Tier 2
            weights[2] = 1.0 - 0.8 * ratio;     // Tier 3
            weights[3] = 0.7 - 0.5 * ratio;     // Tier 4
            weights[4] = ratio * ratio * 1.2;   // Tier 5 grows fast

            // Clamp to minimum 0.05 for non-Tier 5 to ensure some chance
            for (var i = 0; i < 4; i++)
            {
                weights[i] = Math.Max(weights[i], 0.05);
            }

            // Normalize and roll
            double totalWeight = 0;
            foreach (var w in weights) totalWeight += w;

            var roll = _random.NextDouble() * totalWeight;
            double cumulative = 0;

            for (var i = 0; i < weights.Length; i++)
            {
                cumulative += weights[i];
                if (roll <= cumulative)
                    return i + 1;
            }

            return 5; // Fallback
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
