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

        [TestCase(0, 1, 4)]
        [TestCase(1, 0, 1)]
        [TestCase(6, 0, 1)]
        [TestCase(7, 0, 1)]
        [TestCase(8, 0, 2)]
        [TestCase(13, 0, 2)]
        [TestCase(14, 0, 2)]
        [TestCase(15, 0, 3)]
        [TestCase(20, 0, 3)]
        [TestCase(21, 0, 3)]
        [TestCase(22, 0, 4)]
        [TestCase(27, 0, 4)]
        [TestCase(28, 0, 4)]
        [TestCase(29, 0, 1)]
        [TestCase(111, 0, 4)]
        [TestCase(112, 0, 4)]
        [TestCase(113, 1, 1)]
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
