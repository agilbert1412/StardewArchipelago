using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewArchipelago.Stardew;

namespace StardewArchipelago.Archipelago.Gifting
{
    public class ItemAmount
    {
        public StardewItem Item { get; set; }
        public int Amount { get; set; }

        public ItemAmount(StardewItem item, int amount)
        {
            Item = item;
            Amount = amount;
        }

        public static implicit operator ItemAmount((StardewItem, int) tuple) => new(tuple.Item1, tuple.Item2);
    }
}
