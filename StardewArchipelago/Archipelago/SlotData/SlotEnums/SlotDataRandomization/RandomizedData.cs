using System.Collections.Generic;

namespace StardewArchipelago.Archipelago.SlotData.SlotEnums.SlotDataRandomization
{
    public class RandomizedData
    {
        public Dictionary<string, RandomizedFishData> Fish { get; set; }

        public RandomizedData()
        {
            Fish = new Dictionary<string, RandomizedFishData>();
        }

        public void AssignNames()
        {
            AssignFishNames();
        }

        private void AssignFishNames()
        {
            foreach (var (name, data) in Fish)
            {
                data.Name = name;
            }
        }
    }
}
