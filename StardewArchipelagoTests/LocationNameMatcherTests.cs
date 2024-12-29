using FluentAssertions;
using StardewArchipelago.Locations;

namespace StardewArchipelagoTests
{
    public class LocationNameMatcherTests
    {
        private LocationNameMatcher _locationNameMatcher;

        [SetUp]
        public void Setup()
        {
            _locationNameMatcher = new LocationNameMatcher();
        }

        [TestCase("Snail", new[] { "Shipsanity: Snail", "Fishsanity: Snail" }, new[] { "Escargot", "Nail" }, TestName = "Snail")]
        [TestCase("Stone", new[] { "Shipsanity: Stone" }, new[] { "Shipsanity: Wood", "Explore Stonehenge" }, TestName = "Stone")]
        [TestCase("Wood", new[] { "Shipsanity: Wood" }, new[] { "Shipsanity: Hardwood", "Craft Wooden Display" }, TestName = "Wood")]
        [TestCase("Juice", new[] { "Shipsanity: Juice" }, new string[0], TestName = "Juice")]
        [TestCase("Win Grange Display", new[] { "Win Grange Display" }, new[] { "Grange Display" }, TestName = "Grange Display")]
        [TestCase("Hardwood", new[] { "Shipsanity: Hardwood" }, new[] { "Robin's Project" }, TestName = "Hardwood")]
        [TestCase("Tomato", new[] { "Harvest Tomato", "Shipsanity: Tomato" }, new string[0], TestName = "Tomato")]
        [TestCase("Wizard", new[] { "Friendsanity: Wizard 4 <3" }, new string[0], TestName = "Wizard")]
        [TestCase("Apple", new[] { "Harvest Apple" }, new[] { "Friendsanity: Apples 1 <3" }, TestName = "Apple")]
        [TestCase("Apples", new[] { "Friendsanity: Apples 10 <3" }, new[] { "Harvest Apple" }, TestName = "Apples")]
        [TestCase("Opal", new[] { "Museumsanity: Opal" }, new[] { "Museumsanity: Fire Opal" }, TestName = "Opal")]
        [TestCase("Fire Opal", new[] { "Museumsanity: Fire Opal" }, new[] { "Museumsanity: Opal" }, TestName = "Opal")]
        [TestCase("Chest", new[] { "Craft Chest" }, new[] { "Craft Stone Chest", "Craft Big Chest", "Craft Big Stone Chest" }, TestName = "Chest")]
        [TestCase("Stone Chest", new[] { "Craft Stone Chest" }, new[] { "Craft Chest", "Craft Big Chest", "Craft Big Stone Chest" }, TestName = "Stone Chest")]
        [TestCase("Big Chest", new[] { "Craft Big Chest" }, new[] { "Craft Stone Chest", "Craft Chest", "Craft Big Stone Chest" }, TestName = "Big Chest")]
        [TestCase("Big Stone Chest", new[] { "Craft Big Stone Chest" }, new[] { "Craft Chest", "Craft Big Chest", "Craft Stone Chest" }, TestName = "Big Stone Chest")]
        [TestCase("Egg", new[] { "Shipsanity: Egg" }, new[] { "Shipsanity: Duck Egg", "Shipsanity: Egg (Brown)", "Shipsanity: Large Egg", "Shipsanity: Green Slime Egg", "Shipsanity: Calico Egg" }, TestName = "Egg")]
        [TestCase("Large Egg", new[] { "Shipsanity: Large Egg" }, new[] { "Shipsanity: Egg (Brown)", "Shipsanity: Large Egg (Brown)" }, TestName = "Large Egg")]
        [TestCase("Egg (Brown)", new[] { "Shipsanity: Egg (Brown)" }, new[] { "Shipsanity: Large Egg (Brown)" }, TestName = "Brown Egg")]
        public void GetAllLocationsContainingWordTruePositivesTest(string itemName, string[] locationsMatching, string[] locationsNotMatching)
        {
            // Arrange
            var allLocations = locationsMatching.Union(locationsNotMatching);

            // Act
            var matches = _locationNameMatcher.GetAllLocationsContainingWord(allLocations, itemName);

            // Assert
            matches.Should().NotBeNull();
            matches.Should().BeEquivalentTo(locationsMatching);
        }

        [TestCase("Snail", new[] { "Open Professor Snail Cave" }, TestName = "Professor Snail Cave")]
        [TestCase("Stone", new[] { "Shipsanity: Swirl Stone", "Smashing Stone" }, TestName = "Swirl Stone")]
        [TestCase("Hardwood", new[] { "Shipsanity: Hardwood Display: Amphibian Fossil" }, TestName = "Hardwood Displays")]
        [TestCase("Anchor", new[] { "Repair Boat Anchor" }, TestName = "Boat Anchor")]
        [TestCase("Diamond", new[] { "Read The Diamond Hunter", "Starfish Diamond", "Diamond Of Indents", "Diamond Of Pebbles" }, TestName = "Diamond")]
        [TestCase("Opal", new[] { "Fire Opal" }, TestName = "Opals")]
        [TestCase("Chest", new[] { "Craft Stone Chest", "Craft Big Chest", "Craft Big Stone Chest", "Volcano Common Chest Walnut", "Volcano Rare Chest Walnut", "Deep Woods Treasure Chest" }, TestName = "Chest")]
        public void GetAllLocationsContainingWordFalsePositivesTest(string itemName, string[] locationsNotMatching)
        {
            // Arrange

            // Act
            var matches = _locationNameMatcher.GetAllLocationsContainingWord(locationsNotMatching, itemName);

            // Assert
            matches.Should().NotBeNull();
            matches.Should().BeEmpty();
        }

        [TestCase("Juice", new[] { "Pam Needs Juice" }, TestName = "Pam Juice")]
        [TestCase("Tomato", new[] { "Shipsanity: Tomato Seeds" }, TestName = "Tomato Seeds")]
        [TestCase("Wizard", new[] { "Meet The Wizard" }, TestName = "Meet the Wizard")]
        public void GetAllLocationsContainingWordThematicallyRelatedItemsTest(string itemName, string[] locationsMatching)
        {
            // Arrange

            // Act
            var matches = _locationNameMatcher.GetAllLocationsContainingWord(locationsMatching, itemName);

            // Assert
            matches.Should().NotBeNull();
            matches.Should().BeEquivalentTo(locationsMatching);
        }
    }
}
