using System;
using System.Linq;
using StardewArchipelago.Constants;
using StardewArchipelago.Items.Traps;
using StardewValley;

namespace StardewArchipelago.Items
{
    public class CowSpawner
    {
        public void SpawnManyInvisibleCows(int amount)
        {
            for (var i = 0; i < amount; i++)
            {
                SpawnInvisibleCow();
            }
        }

        public void SpawnInvisibleCow()
        {
            SpawnInvisibleCowOnMap(Game1.currentLocation);
            SpawnInvisibleCowOnMap(Game1.getFarm());
            SpawnInvisibleCowOnMap(Game1.getLocationFromName("Town"));
            SpawnInvisibleCowOnMap(Game1.getLocationFromName("Forest"));
            SpawnInvisibleCowOnMap(Game1.getLocationFromName("Mountain"));
            var rand = Game1.random.Next(3);
            switch (rand)
            {
                case 0:
                    SpawnInvisibleCowOnMap(Game1.getLocationFromName("BusStop"));
                    break;
                case 1:
                    SpawnInvisibleCowOnMap(Game1.getLocationFromName("Backwoods"));
                    break;
                case 2:
                    SpawnInvisibleCowOnMap(Game1.getLocationFromName("Beach"));
                    break;
            }
        }

        private void SpawnInvisibleCowOnMap(GameLocation currentMap)
        {
            var cowType = Game1.random.NextDouble() < 0.5 ? "White Cow" : "Brown Cow";
            var cowName = ChooseCowName(Game1.random, Community.AllNames);
            var tile = currentMap.getRandomTile() * 64f;
            var cow = new FarmAnimal(cowType, Game1.random.NextInt64(), Game1.player.UniqueMultiplayerID)
            {
                Position = tile,
                Name = cowName,
                displayName = cowName,
            };
            cow.growFully();

            cow.modData.Add(CowInjections.INVISIBLE_COW_KEY, true.ToString());

            // ((AnimalHouse)this.newAnimalHome.GetIndoors()).adoptAnimal(this.animalBeingPurchased);

            Game1.currentLocation.Animals.Add(cow.myID.Value, cow);
            cow.currentLocation = Game1.currentLocation;
            // Game1.currentLocation.Animals.Add(cow.myID.Value);
            cow.homeInterior = Game1.currentLocation;
            cow.setRandomPosition(Game1.currentLocation);
        }

        private string ChooseCowName(Random random, string[] allValidNames)
        {
            var npcNames = DataLoader.Characters(Game1.content).Keys.ToHashSet();
            foreach (var npc in Utility.getAllCharacters())
            {
                npcNames.Add(npc.Name);
            }
            string cowName;
            var maxAttempts = allValidNames.Length * 10;
            var attempt = 0;
            do
            {
                attempt++;
                cowName = allValidNames[random.Next(0, allValidNames.Length)];
                if (attempt >= maxAttempts)
                {
                    while (npcNames.Contains(cowName))
                    {
                        cowName += " ";
                    }
                }
            } while (npcNames.Contains(cowName));

            return cowName;
        }
    }
}
