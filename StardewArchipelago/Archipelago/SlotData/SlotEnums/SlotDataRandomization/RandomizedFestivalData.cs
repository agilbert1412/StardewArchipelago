using StardewValley;
using System;
using System.Linq;
using System.Threading;

namespace StardewArchipelago.Archipelago.SlotData.SlotEnums.SlotDataRandomization
{
    public class RandomizedFestivalData
    {
        public string Name { get; set; }
        public string Season { get; set; }
        public string Day { private get; set; }

        public int? StartDay
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Day))
                {
                    return null;
                }

                var parts = Day.Split(",");
                return int.Parse(parts[0]);
            }
        }

        public int? EndDay
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Day))
                {
                    return null;
                }

                var parts = Day.Split(",");
                return int.Parse(parts.Last());
            }
        }

        public RandomizedFestivalData()
        {

        }

        public void AssignName(string name)
        {
            Name = name;
        }
    }
}
