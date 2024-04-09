using FluentAssertions;
using StardewArchipelago.Locations.CodeInjections.Vanilla;

namespace StardewArchipelagoTests
{
    public class QueenOfSauceTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [TestCase(0, 1, 3)]
        [TestCase(1, 0, 0)]
        [TestCase(6, 0, 0)]
        [TestCase(7, 0, 0)]
        [TestCase(8, 0, 1)]
        [TestCase(13, 0, 1)]
        [TestCase(14, 0, 1)]
        [TestCase(15, 0, 2)]
        [TestCase(20, 0, 2)]
        [TestCase(21, 0, 2)]
        [TestCase(22, 0, 3)]
        [TestCase(27, 0, 3)]
        [TestCase(28, 0, 3)]
        [TestCase(29, 0, 0)]
        [TestCase(111, 0, 3)]
        [TestCase(112, 0, 3)]
        [TestCase(113, 1, 0)]
        public void GetCurrentDateComponents(int daysPlayed, int expectedYear, int expectedWeek)
        {
            // Arrange

            // Act
            QueenOfSauceInjections.GetCurrentDateComponents(daysPlayed, out var year, out var week);

            // Assert
            year.Should().Be(expectedYear);
            week.Should().Be(expectedWeek);
        }
    }
}
