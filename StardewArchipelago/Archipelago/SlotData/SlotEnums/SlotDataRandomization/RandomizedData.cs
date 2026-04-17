using System.Collections.Generic;
using StardewValley;

namespace StardewArchipelago.Archipelago.SlotData.SlotEnums.SlotDataRandomization
{
    public class RandomizedData
    {
        public Dictionary<string, RandomizedCropData> Crops { get; set; }
        public Dictionary<string, RandomizedFishData> Fish { get; set; }

        public RandomizedData()
        {
            Crops = new Dictionary<string, RandomizedCropData>();
            Fish = new Dictionary<string, RandomizedFishData>();
        }

        public void AssignNames()
        {
            AssignCropNames();
            AssignFishNames();
        }

        private void AssignCropNames()
        {
            foreach (var (name, data) in Crops)
            {
                data.AssignName(name);
            }
        }

        private void AssignFishNames()
        {
            foreach (var (name, data) in Fish)
            {
                data.AssignName(name);
            }
        }
    }
}
