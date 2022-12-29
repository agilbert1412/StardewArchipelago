using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;

namespace StardewArchipelago.Stardew
{
    public abstract class StardewItem
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public int SellPrice { get; private set; }
        public string DisplayName { get; private set; }
        public string Description { get; private set; }

        public StardewItem(int id, string name, int sellPrice, string displayName, string description)
        {
            Id = id;
            Name = name;
            SellPrice = sellPrice;
            DisplayName = displayName;
            Description = description;
        }

        public abstract Item PrepareForGivingToFarmer(int amount = 1);

        public abstract void GiveToFarmer(Farmer farmer, int amount = 1);
    }
}
