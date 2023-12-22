using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StardewArchipelago.Locations.CodeInjections.Modded
{
    public class PricedItem
    {
        public string ItemName { get; set; }
        public int[] Price { get; set; }

        public PricedItem(string itemName, int priceInGold) : this(itemName, new[] { priceInGold, 1 })
        {
        }

        public PricedItem(string itemName, int[] price)
        {
            ItemName = itemName;
            Price = price;
        }
    }
}
