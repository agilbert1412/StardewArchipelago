using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;
using StardewValley.Objects;

namespace StardewArchipelago.Stardew
{
    public class StardewObject : StardewItem
    {
        public int Edibility { get; private set; }
        public string Type { get; private set; }
        public string Category { get; private set; }
        public string Misc1 { get; private set; }
        public string Misc2 { get; private set; }
        public string BuffDuration { get; private set; }

        public StardewObject(int id, string name, int sellPrice, int edibility, string type, string category, string displayName, string description, string misc1 = "", string misc2 = "", string buffDuration = "")
        : base(id, name, sellPrice, displayName, description)
        {
            Edibility = edibility;
            Type = type;
            Category = category;
            Misc1 = misc1;
            Misc2 = misc2;
            BuffDuration = buffDuration;
        }

        public override Item PrepareForGivingToFarmer(int amount = 1)
        {
            if (Type == "Ring")
            {
                return new Ring(Id);
            }
            return new StardewValley.Object(Id, amount);
        }

        public override void GiveToFarmer(Farmer farmer, int amount = 1)
        {
            var item = PrepareForGivingToFarmer(amount);
            farmer.addItemByMenuIfNecessary(item);
        }
    }
}
