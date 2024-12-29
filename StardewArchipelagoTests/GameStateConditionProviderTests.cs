using FluentAssertions;
using StardewArchipelago.Constants;

namespace StardewArchipelagoTests
{
    public class GameStateConditionProviderTests
    {

        [SetUp]
        public void Setup()
        {
        }

        [TestCase("Deluxe Barn", "!BUILDINGS_CONSTRUCTED ALL \"Deluxe Barn\" 0 0")]
        [TestCase("Big Barn", "ANY \"!BUILDINGS_CONSTRUCTED ALL \\\"Big Barn\\\" 0 0\" \"!BUILDINGS_CONSTRUCTED ALL \\\"Deluxe Barn\\\" 0 0\"")]
        [TestCase("Barn", "ANY \"!BUILDINGS_CONSTRUCTED ALL Barn 0 0\" \"!BUILDINGS_CONSTRUCTED ALL \\\"Big Barn\\\" 0 0\" \"!BUILDINGS_CONSTRUCTED ALL \\\"Deluxe Barn\\\" 0 0\"")]
        [TestCase("Deluxe Coop", "!BUILDINGS_CONSTRUCTED ALL \"Deluxe Coop\" 0 0")]
        [TestCase("Big Coop", "ANY \"!BUILDINGS_CONSTRUCTED ALL \\\"Big Coop\\\" 0 0\" \"!BUILDINGS_CONSTRUCTED ALL \\\"Deluxe Coop\\\" 0 0\"")]
        [TestCase("Coop", "ANY \"!BUILDINGS_CONSTRUCTED ALL Coop 0 0\" \"!BUILDINGS_CONSTRUCTED ALL \\\"Big Coop\\\" 0 0\" \"!BUILDINGS_CONSTRUCTED ALL \\\"Deluxe Coop\\\" 0 0\"")]
        [TestCase("Big Shed", "ANY \"!BUILDINGS_CONSTRUCTED ALL \\\"Big Shed\\\" 0 0\" \"!BUILDINGS_CONSTRUCTED ALL \\\"Deluxe Shed\\\" 0 0\"")]
        [TestCase("Shed", "ANY \"!BUILDINGS_CONSTRUCTED ALL Shed 0 0\" \"!BUILDINGS_CONSTRUCTED ALL \\\"Big Shed\\\" 0 0\" \"!BUILDINGS_CONSTRUCTED ALL \\\"Deluxe Shed\\\" 0 0\"")]
        [TestCase("Well", "!BUILDINGS_CONSTRUCTED ALL Well 0 0")]
        [TestCase("Slime Hutch", "!BUILDINGS_CONSTRUCTED ALL \"Slime Hutch\" 0 0")]
        [TestCase("Silo", "!BUILDINGS_CONSTRUCTED ALL Silo 0 0")]
        public void HasBuildingOrHigher(string buildingName, string expectedCondition)
        {
            // Arrange

            // Act
            var condition = GameStateConditionProvider.CreateHasBuildingOrHigherCondition(buildingName, true);

            // Assert
            condition.Should().NotBeNull();
            condition.Should().BeEquivalentTo(expectedCondition);
        }
    }
}
