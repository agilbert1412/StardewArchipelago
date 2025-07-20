using StardewArchipelago.Locations.Festival;
using StardewArchipelago.Locations.GingerIsland.Parrots;
using System.Collections.Generic;
using StardewArchipelago.Locations.Secrets;

namespace StardewArchipelago.Constants.Locations
{
    internal class NameMatching
    {
        public static readonly Dictionary<string, string[]> Exceptions = new()
        {
            { "Anchor", new[] { "Boat Anchor" } },
            { "Bait", new[] { "Magic Bait", "Wild Bait", "Challenge Bait", "Targeted Bait", "Specific Bait", "Deluxe Bait", "Bait Maker" } },
            { "Blueberry", new[] { "Blueberry Tart" } },
            { "Bomb", new[] { "Cherry Bomb", "Mega Bomb" } },
            { "Carp", new[] { "Midnight Carp", "Scorpion Carp", "Mutant Carp", "Carp Surprise" } },
            { "Chest", new[] { "Stone Chest", "Treasure Chest", "Big Chest", "Common Chest", "Rare Chest" } },
            { "Clay", new[] { "Land Of Clay" } },
            { "Coffee", new[] { "Coffee Bean" } },
            { "Diamond", new[] { "Diamond Hunter", "Starfish Diamond", "Diamond Of", "The Diamond Hunter" } },
            { "Egg (Brown)", new[] { "Large Egg (Brown)" } },
            { "Egg", new[] { "Duck Egg", "Ostrich Egg", "Dinosaur Egg", "Void Egg", "Thunder Egg", "Large Egg", "Egg (Brown)", "Brown Egg", "Slime Egg", "Golden Egg", "Calico Egg", "Egg Festival: Strawberry Seeds", "Egg Hunt Victory" } },
            { "Fiber", new[] { "Fiber Seeds" } },
            { "Geode", new[] { "Geode Crusher" } },
            { "Hardwood", new[] { "Hardwood Display", "Hardwood Fence" } },
            { "Jasper", new[] { "by M. Jasper" } },
            { "Large Egg", new[] { "Large Egg (Brown)" } },
            { "Legend", new[] { "The Legend of the Winter Star" } },
            { "Milk", new[] { "Large Milk", "Goat Milk", "Snake Milk" } },
            { "Opal", new[] { "Fire Opal" } },
            { "Pizza", new[] { "Calico Pizza" } },
            { "Salad", new[] { "Sour Salad" } },
            { "Salmon", new[] { "Void Salmon", "King Salmon" } },
            { "Shrimp", new[] { "Rainforest Shrimp", "Shrimp Donut" } },
            { "Slime", new[] { "Slime Hutch", "Slime Egg", "Slime Incubator" } },
            { "Snail", new[] { IslandNorthInjections.AP_PROF_SNAIL_CAVE } },
            { "Speed-Gro", new[] { "Deluxe Speed-Gro", "Hyper Speed-Gro" } },
            { "Sprinkler", new[] { "Quality Sprinkler", "Iridium Sprinkler" } },
            { "Stone Chest", new[] { "Big Stone Chest" } },
            { "Stone", new[] { FestivalLocationNames.STRENGTH_GAME, "Lemon Stone", "Ocean Stone", "Fairy Stone", "Swirl Stone", "Stone Junimo", "Stone Chest", "Stone Walkway", "Stone Brazier", "Stone Sign" } },
            { "Strawberry", new[] { "Egg Festival: Strawberry Seeds" } },
            { "Trash", new[] { "Trash Can Upgrade", "Trash Bear", "Trash Catalogue" } },
            { "Turnip", new[] { "Rarecrow #1 (Turnip Head)" } },
            { "Wood", new[] { "Wood Fence", "Wood Floor", "Wood Path", "Wood Lamp", "Wood Sign" } }
        };
    }
}
