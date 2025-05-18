using FluentAssertions;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles.Gacha;

namespace StardewArchipelagoTests
{
    public class GachaRollerTests
    {
        private const int _numberAttempts = 2000;
        private const double _acceptableLowerBound = 0.75;
        private const double _acceptableUpperBound = 2.25;

        private GachaRoller _gachaRoller;

        [SetUp]
        public void Setup()
        {
        }

        //[Test]
        //public void TestNumberOfRewards()
        //{
        //    var allRewards = new List<string>();
        //    allRewards.AddRange(_tier0Items);
        //    allRewards.AddRange(_tier1Items);
        //    allRewards.AddRange(_tier2Items);
        //    allRewards.AddRange(_tier3Items);
        //    allRewards.AddRange(_tier4Items);
        //    Console.WriteLine($"Number of Rewards: {allRewards.Count}");
        //}

        [TestCaseSource(typeof(GachaRollerTests), nameof(PricesAndInvestments))]
        public void TestRollShouldGiveT5AfterInvestingApproximatelyPrice(int bundlePrice, int investmentAmount)
        {
            if (investmentAmount > bundlePrice)
            {
                return;
            }

            // Arrange
            _gachaRoller = new GachaRoller(bundlePrice);
            var totalTotalInvestments = 0;

            // Act
            for (var i = 0; i < _numberAttempts; i++)
            {
                var totalInvestment = 0;
                var roll = -1;
                while (roll != 5)
                {
                    roll = _gachaRoller.RollTier(investmentAmount);
                    totalInvestment += investmentAmount;
                }
                totalTotalInvestments += totalInvestment;
            }

            // Assert
            var averageTotalInvestment = totalTotalInvestments / _numberAttempts;
            var lowerBound = (int)Math.Round(bundlePrice * _acceptableLowerBound);
            var upperBound = (int)Math.Round(bundlePrice * _acceptableUpperBound);
            averageTotalInvestment.Should().BeGreaterThan(lowerBound);
            averageTotalInvestment.Should().BeLessThan(upperBound);
        }

        [TestCaseSource(typeof(GachaRollerTests), nameof(PricesAndInvestments))]
        public void TestRollsShouldGiveMoreOfExpectedTiers(int bundlePrice, int investmentAmount)
        {
            // Arrange
            _gachaRoller = new GachaRoller(bundlePrice);
            var numberOfEachResult = new Dictionary<int, int>
            {
                { 0, 0 }, { 1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 }, { 5, 0 },
            };

            // Act
            for (var i = 0; i < _numberAttempts; i++)
            {
                var roll = -1;
                while (roll != 5)
                {
                    roll = _gachaRoller.RollTier(investmentAmount);
                    numberOfEachResult[roll]++;
                }
            }

            // Assert
            var cheap = investmentAmount < bundlePrice * 0.05;
            var expensive = investmentAmount > bundlePrice * 0.5;
            if (cheap)
            {
                // numberOfEachResult[0].Should().BeGreaterThanOrEqualTo(numberOfEachResult[1]);
                numberOfEachResult[1].Should().BeGreaterThanOrEqualTo(numberOfEachResult[2]);
                numberOfEachResult[2].Should().BeGreaterThanOrEqualTo(numberOfEachResult[3]);
                numberOfEachResult[3].Should().BeGreaterThanOrEqualTo(numberOfEachResult[4]);
                numberOfEachResult[4].Should().BeGreaterThanOrEqualTo(numberOfEachResult[5]);
            }
            if (expensive)
            {
                numberOfEachResult[5].Should().BeGreaterThanOrEqualTo(numberOfEachResult[4]);
                numberOfEachResult[4].Should().BeGreaterThanOrEqualTo(numberOfEachResult[3]);
                numberOfEachResult[3].Should().BeGreaterThanOrEqualTo(numberOfEachResult[2]);
                numberOfEachResult[2].Should().BeGreaterThanOrEqualTo(numberOfEachResult[1]);
                numberOfEachResult[1].Should().BeGreaterThanOrEqualTo(numberOfEachResult[0]);
            }
        }

        [TestCaseSource(typeof(GachaRollerTests), nameof(PricesAndInvestments))]
        public void TestPrintEffectiveOddsAndExpectedRolls(int bundlePrice, int investmentAmount)
        {
            // Arrange
            _gachaRoller = new GachaRoller(bundlePrice);
            var totalRollsToT5 = 0;
            var totalTierCounts = new Dictionary<int, int>
            {
                { 0, 0 }, { 1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 }, { 5, 0 },
            };

            // Act
            for (var i = 0; i < _numberAttempts; i++)
            {
                var roll = -1;
                var rollsThisRun = 0;
                while (roll != 5)
                {
                    roll = _gachaRoller.RollTier(investmentAmount);
                    totalTierCounts[roll]++;
                    rollsThisRun++;
                }

                totalRollsToT5 += rollsThisRun;
            }

            var avgRollsToT5 = totalRollsToT5 / (double)_numberAttempts;
            Console.WriteLine($"[Price={bundlePrice:N0}, Investment={investmentAmount:N0}]");
            Console.WriteLine($"  Avg Rolls to reach T5: {avgRollsToT5:F2} ({avgRollsToT5*investmentAmount:F2}g)");

            for (var tier = 0; tier <= 5; tier++)
            {
                var odds = 100.0 * totalTierCounts[tier] / totalRollsToT5;
                Console.WriteLine($"  Tier {tier}: {odds:F2}%");
            }

            Console.WriteLine();
        }

        private static IEnumerable<int> Prices()
        {
            return new[] { 1000, 2000, 6000, 10000, 14000, 18000, 40000 };
        }

        private static IEnumerable<int> Investments()
        {
            return new[] { 5, 50, 500, 5000 };
        }

        private static IEnumerable<object[]> PricesAndInvestments()
        {
            var investments = Investments();
            var prices = Prices();
            return GetPricesAndInvestmentsMatrix(prices, investments);
        }

        private static IEnumerable<object[]> GetPricesAndInvestmentsMatrix(IEnumerable<int> prices, IEnumerable<int> investments)
        {
            foreach (var price in prices)
            {
                foreach (var investment in investments)
                {
                    yield return new object[] { price, investment };
                }
            }
        }
    }
}
