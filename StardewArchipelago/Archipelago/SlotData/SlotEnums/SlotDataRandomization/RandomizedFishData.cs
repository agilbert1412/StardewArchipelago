using Microsoft.Xna.Framework;
using StardewArchipelago.GameModifications.EntranceRandomizer;
using StardewArchipelago.GameModifications.RandomizedData;
using StardewArchipelago.Stardew;
using StardewValley;
using StardewValley.GameData.Locations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StardewArchipelago.Archipelago.SlotData.SlotEnums.SlotDataRandomization
{
    public class RandomizedFishData
    {
        public string Name { get; set; }
        public string Method { get; set; }
        public int? Difficulty { get; set; }
        public string[] Season { get; set; }
        public string[] Location { get; set; }
        public string[] Weather { get; set; }
        public int? SellPrice { get; set; }

        public RandomizedFishData()
        {

        }

        public void AssignName(string name)
        {
            Name = name;
        }
    }
}
