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

        // TODO expand that list, it only has quests currently
        public static readonly Dictionary<string, string[]> ItemNeededForLocations = new()
        {
            { "Albacore", new[] { "Fish Stew" } },
            { "Amaranth", new[] { "Cow's Delight" } },
            { "Amethyst", new[] { "Clint's Attempt" } },
            { "Apricot", new[] { "Fresh Fruit" } },
            { "Battery Pack", new[] { "Pam Needs Juice" } },
            { "Beet", new[] { "The Mysterious Qi" } },
            { "Cauliflower", new[] { "Jodi's Request" } },
            { "Cave Carrot", new[] { "Marnie's Request" } },
            { "Coconut", new[] { "Exotic Spirits" } },
            { "Hardwood", new[] { "Robin's Request", "The Giant Stump" } },
            { "Hot Pepper", new[] { "Knee Therapy" } },
            { "Iridium Bar", new[] { "Staff of Power" } },
            { "Iron Bar", new[] { "A Favor For Clint" } },
            { "Largemouth Bass", new[] { "Fish Casserole" } },
            { "Leek", new[] { "Granny's Gift" } },
            { "Maple Syrup", new[] { "Strange Note" } },
            { "Melon", new[] { "Crop Research" } },
            { "Pale Ale", new[] { "Pam Is Thirsty" } },
            { "Pufferfish", new[] { "Aquatic Research" } },
            { "Pumpkin", new[] { "Carving Pumpkins" } },
            { "Rainbow Shell", new[] { "The Mysterious Qi" } },
            { "Sashimi", new[] { "Pierre's Notice" } },
            { "Solar Essence", new[] { "The Mysterious Qi" } },
            { "Starfruit", new[] { "A Soldier's Star" } },
            { "Truffle Oil", new[] { "Mayor's Need" } },
            { "Void Essence", new[] { "A Dark Reagent" } },
            { "Void Mayonnaise", new[] { "Goblin Problem" } },
        };
    }
}
