using FluentAssertions;
using StardewArchipelago.Items.Traps;

namespace StardewArchipelagoTests
{
    public class GiftTrapManagerTests
    {
        private GiftTrapManager _giftTrapManager;

        [SetUp]
        public void Setup()
        {
            _giftTrapManager = new GiftTrapManager(null);
        }

        [Test]
        public void TestBombOfVeryLowQualityHasRadiusOf1()
        {
            // Arrange
            var quality1 = 0.25;
            var quality2 = 0.1;

            // Act
            var radius1 = _giftTrapManager.GetRadiusFromGift(quality1);
            var radius2 = _giftTrapManager.GetRadiusFromGift(quality2);

            // Assert
            radius1.Should().Be(1);
            radius2.Should().Be(1);
        }

        [Test]
        public void TestBombOfLowQualityHasRadiusOf3()
        {
            // Arrange
            var quality = 0.5;

            // Act
            var radius = _giftTrapManager.GetRadiusFromGift(quality);

            // Assert
            radius.Should().Be(3);
        }

        [Test]
        public void TestBombOfAverageQualityHasRadiusOf5()
        {
            // Arrange
            var quality = 1.0;

            // Act
            var radius = _giftTrapManager.GetRadiusFromGift(quality);

            // Assert
            radius.Should().Be(5);
        }

        [Test]
        public void TestBombOfHighQualityHasRadiusOf7()
        {
            // Arrange
            var quality = 2.0;

            // Act
            var radius = _giftTrapManager.GetRadiusFromGift(quality);

            // Assert
            radius.Should().Be(7);
        }

        [TestCase(4.0, 9)]
        [TestCase(8.0, 11)]
        [TestCase(16.0, 13)]
        [TestCase(32.0, 15)]
        [TestCase(64.0, 17)]
        [TestCase(128.0, 19)]
        [TestCase(256.0, 21)]
        [TestCase(512.0, 23)]
        [TestCase(1024.0, 25)]
        [TestCase(2048.0, 25)]
        public void TestBombOfVeryHighQualityHasVeryHighRadius(double quality, int expectedRadius)
        {
            // Arrange

            // Act
            var radius = _giftTrapManager.GetRadiusFromGift(quality);

            // Assert
            radius.Should().Be(expectedRadius);
        }

        [Test]
        public void TestBombOfLowDurationHasDelayOf50()
        {
            // Arrange
            var duration = 0.5;

            // Act
            var delay = _giftTrapManager.GetDelayFromGift(duration);

            // Assert
            delay.Should().Be(50);
        }

        [Test]
        public void TestBombOfAverageDurationHasDelayOf100()
        {
            // Arrange
            var duration = 1.0;

            // Act
            var delay = _giftTrapManager.GetDelayFromGift(duration);

            // Assert
            delay.Should().Be(100);
        }

        [Test]
        public void TestBombOfHighDurationHasDelayOf200()
        {
            // Arrange
            var duration = 2.0;

            // Act
            var delay = _giftTrapManager.GetDelayFromGift(duration);

            // Assert
            delay.Should().Be(200);
        }
    }
}
