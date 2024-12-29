using FluentAssertions;
using StardewArchipelago.PriceMath;

namespace StardewArchipelagoTests.PriceMath
{
    public class BackpackPriceCalculatorTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [TestCase(0, 400)]
        [TestCase(1, 2000)]
        [TestCase(2, 10000)]
        [TestCase(3, 50000)]
        public void TestGetPriceOneBackpack(int tier, int expectedPrice)
        {
            // Arrange
            var priceCalculator = new BackpackPriceCalculator();

            // Act
            var price = priceCalculator.GetPrice(tier, 1, 1);

            // Assert
            price.Should().Be(expectedPrice);
        }

        [TestCase(0, 1, 133)]
        [TestCase(0, 2, 267)]
        [TestCase(1, 1, 667)]
        [TestCase(1, 2, 1333)]
        [TestCase(2, 1, 3333)]
        [TestCase(2, 2, 6667)]
        [TestCase(3, 1, 16667)]
        [TestCase(3, 2, 33333)]
        public void TestGetPriceTwoBackpacks(int tier, int backpackNumber, int expectedPrice)
        {
            // Arrange
            var priceCalculator = new BackpackPriceCalculator();

            // Act
            var price = priceCalculator.GetPrice(tier, backpackNumber, 2);

            // Assert
            price.Should().Be(expectedPrice);
        }

        [TestCase(0, 1, 67)]
        [TestCase(0, 2, 133)]
        [TestCase(0, 3, 200)]
        [TestCase(1, 1, 333)]
        [TestCase(1, 2, 667)]
        [TestCase(1, 3, 1000)]
        [TestCase(2, 1, 1667)]
        [TestCase(2, 2, 3333)]
        [TestCase(2, 3, 5000)]
        [TestCase(3, 1, 8333)]
        [TestCase(3, 2, 16667)]
        [TestCase(3, 3, 25000)]
        public void TestGetPriceThreeBackpacks(int tier, int backpackNumber, int expectedPrice)
        {
            // Arrange
            var priceCalculator = new BackpackPriceCalculator();

            // Act
            var price = priceCalculator.GetPrice(tier, backpackNumber, 3);

            // Assert
            price.Should().Be(expectedPrice);
        }
    }
}
