using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Items;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace StardewArchipelago.GameModifications.Buildings
{
    public class ShopMenuArchipelago : ShopMenu
    {
        public ShopMenuArchipelago(Dictionary<ISalable, int[]> itemPriceAndStock, string who) : base(itemPriceAndStock, who: who)
        {

        }
    }
}
