using System.Collections.Generic;
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
