using System;

namespace StardewArchipelago.Archipelago.SlotData.SlotEnums.SlotDataRandomization
{
    public class RandomizedCropData
    {
        public string Crop { get; set; }
        public int? GrowthTime { get; set; }
        public string[] Season { get; set; }
        public string Seed { get; set; }
        public int? SellPrice { get; set; }

        public RandomizedCropData()
        {

        }

        public void AssignName(string name)
        {
            Crop = name;
            if (Seed != null && Seed.Equals("Coffee Bean (Starter)", StringComparison.InvariantCultureIgnoreCase))
            {
                Seed = "Coffee Bean";
            }
        }
    }
}
