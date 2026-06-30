using System.Linq;

namespace StardewArchipelago.Archipelago.SlotData.SlotEnums.SlotDataRandomization
{
    public class RandomizedVillagerData
    {
        public string Name { get; set; }
        public string Birthday { get; set; }

        public RandomizedVillagerData()
        {

        }

        public void AssignName(string name)
        {
            Name = name;
        }
    }
}
