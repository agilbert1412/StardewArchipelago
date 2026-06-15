using FluentAssertions;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;
using StardewArchipelago.Items.Traps;

namespace StardewArchipelagoTests
{
    public class TrapTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [TestCase(TrapItemsDifficulty.NoTraps, 0, 1)]
        [TestCase(TrapItemsDifficulty.NoTraps, 1, 1)]
        [TestCase(TrapItemsDifficulty.NoTraps, 10, 1)]
        [TestCase(TrapItemsDifficulty.NoTraps, 9999, 1)]
        [TestCase(TrapItemsDifficulty.Easy, 0, 1)]
        [TestCase(TrapItemsDifficulty.Medium, 0, 1)]
        [TestCase(TrapItemsDifficulty.Hard, 0, 1)]
        [TestCase(TrapItemsDifficulty.Hell, 0, 1)]
        [TestCase(TrapItemsDifficulty.Nightmare, 0, 1)]
        [TestCase(TrapItemsDifficulty.Eldritch, 0, 1)]
        [TestCase(TrapItemsDifficulty.Easy, 1, 1.1)]
        [TestCase(TrapItemsDifficulty.Medium, 1, 1.25)]
        [TestCase(TrapItemsDifficulty.Hard, 1, 1.75)]
        [TestCase(TrapItemsDifficulty.Hell, 1, 2.25)]
        [TestCase(TrapItemsDifficulty.Nightmare, 1, 3)]
        [TestCase(TrapItemsDifficulty.Eldritch, 1, 9)]
        [TestCase(TrapItemsDifficulty.Easy, 2, 1.2)]
        [TestCase(TrapItemsDifficulty.Medium, 2, 1.55)]
        [TestCase(TrapItemsDifficulty.Hard, 2, 3)]
        [TestCase(TrapItemsDifficulty.Hell, 2, 5)]
        [TestCase(TrapItemsDifficulty.Nightmare, 2, 7)]
        [TestCase(TrapItemsDifficulty.Eldritch, 2, 34)]
        [TestCase(TrapItemsDifficulty.Easy, 10, 2.6)]
        [TestCase(TrapItemsDifficulty.Medium, 10, 9)]
        [TestCase(TrapItemsDifficulty.Hard, 10, 26)]
        [TestCase(TrapItemsDifficulty.Hell, 10, 67)]
        [TestCase(TrapItemsDifficulty.Hell, 31, 166)]
        [TestCase(TrapItemsDifficulty.Nightmare, 10, 148)]
        [TestCase(TrapItemsDifficulty.Eldritch, 10, 925)]
        [TestCase(TrapItemsDifficulty.Easy, 100, 20)]
        [TestCase(TrapItemsDifficulty.Medium, 100, 105)]
        [TestCase(TrapItemsDifficulty.Hard, 100, 1010)]
        [TestCase(TrapItemsDifficulty.Hell, 100, 1029)]
        [TestCase(TrapItemsDifficulty.Nightmare, 100, 1825)]
        [TestCase(TrapItemsDifficulty.Eldritch, 100, 30320)]
        [TestCase(TrapItemsDifficulty.Easy, 1000, 117)]
        [TestCase(TrapItemsDifficulty.Medium, 1000, 4101)]
        [TestCase(TrapItemsDifficulty.Hard, 1000, 10106)]
        [TestCase(TrapItemsDifficulty.Hell, 1000, 10322)]
        [TestCase(TrapItemsDifficulty.Nightmare, 1000, 30198)]
        // [TestCase(TrapItemsDifficulty.Eldritch, 1000, 54)] // Apparently this kill the BigInteger...
        public void TestInflationAmount(TrapItemsDifficulty trapDifficulty, int numberReceived, double expectedInflation)
        {
            // Arrange
            var balancer = new TrapDifficultyBalancer();
            var inflationBaseMultiplier = balancer.InflationAmount[trapDifficulty];
            var softcapMultiplier = balancer.InflationSoftcapThreshold[trapDifficulty];

            // Act
            var inflationAmount = TrapExecutor.GetInflationMultiplier(inflationBaseMultiplier, softcapMultiplier, numberReceived);

            // Assert
            var precision = expectedInflation == Math.Round(expectedInflation) ? 1 : 0.1;
            inflationAmount.Should().BeApproximately(expectedInflation, precision);
        }
    }
}
