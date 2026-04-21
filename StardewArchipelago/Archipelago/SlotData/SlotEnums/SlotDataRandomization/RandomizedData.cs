using System.Collections.Generic;
using StardewValley;

namespace StardewArchipelago.Archipelago.SlotData.SlotEnums.SlotDataRandomization
{
    public class RandomizedData
    {
        public Dictionary<string, RandomizedCropData> Crops { get; set; }
        public Dictionary<string, RandomizedFishData> Fish { get; set; }
        public Dictionary<string, RandomizedFestivalData> Festivals { get; set; }

        public RandomizedData()
        {
            Crops = new Dictionary<string, RandomizedCropData>();
            Fish = new Dictionary<string, RandomizedFishData>();
            Festivals = new Dictionary<string, RandomizedFestivalData>();
        }

        public void AssignNames()
        {
            AssignCropNames();
            AssignFishNames();
            AssignFestivalNames();
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

        private void AssignFestivalNames()
        {
            foreach (var (name, data) in Festivals)
            {
                data.AssignName(name);
            }
        }
    }
}
