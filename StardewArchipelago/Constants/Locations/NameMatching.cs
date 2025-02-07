using StardewArchipelago.Locations.Festival;
using StardewArchipelago.Locations.GingerIsland.Parrots;
using System.Collections.Generic;

namespace StardewArchipelago.Constants.Locations
{
    internal class NameMatching
    {
        public static readonly Dictionary<string, string[]> Exceptions = new()
        {
            { "Anchor", new[] { "Boat Anchor" } },
            { "Carp", new[] { "Midnight Carp", "Scorpion Carp", "Mutant Carp" } },
            { "Chest", new[] { "Stone Chest", "Treasure Chest", "Big Chest", "Common Chest", "Rare Chest" } },
            { "Clay", new[] { "Land Of Clay" } },
            { "Diamond", new[] { "Diamond Hunter", "Starfish Diamond", "Diamond Of", } },
            { "Egg (Brown)", new[] { "Large Egg (Brown)" } },
            { "Egg", new[] { "Duck Egg", "Ostrich Egg", "Dinosaur Egg", "Void Egg", "Thunder Egg", "Large Egg", "Egg (Brown)", "Brown Egg", "Slime Egg", "Calico Egg", "Egg Festival: Strawberry Seeds", "Egg Hunt Victory" } },
            { "Hardwood", new[] { "Hardwood Display" } },
            { "Jasper", new[] { "by M. Jasper" } },
            { "Large Egg", new[] { "Large Egg (Brown)" } },
            { "Legend", new[] { "The Legend of the Winter Star" } },
            { "Opal", new[] { "Fire Opal" } },
            { "Pizza", new[] { "Calico Pizza" } },
            { "Salad", new[] { "Sour Salad" } },
            { "Salmon", new[] { "Void Salmon", "King Salmon" } },
            { "Shrimp", new[] { "Rainforest Shrimp", "Shrimp Donut" } },
            { "Slime", new[] { "Slime Hutch" } },
            { "Snail", new[] { IslandNorthInjections.AP_PROF_SNAIL_CAVE } },
            { "Stone Chest", new[] { "Big Stone Chest" } },
            { "Stone", new[] { FestivalLocationNames.STRENGTH_GAME, "Lemon Stone", "Ocean Stone", "Fairy Stone", "Swirl Stone", } },
            { "Strawberry", new[] { "Egg Festival: Strawberry Seeds" } },
            { "Trash", new[] { "Trash Can Upgrade" } },
            { "Turnip", new[] { "Rarecrow #1 (Turnip Head)" } },
        };
    }
}
