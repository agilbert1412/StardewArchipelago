using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using StardewArchipelago.Bundles;

namespace StardewArchipelago.Archipelago.SlotData.SlotEnums.SlotDataRandomization
{
    public class RandomizedShopItemData
    {
        public string ShopName { get; set; }
        public string ItemName { get; set; }
        public string Currency { get; set; }
        public int? Price { get; set; }
        public Dictionary<string, int> Materials { get; set; }

        public RandomizedShopItemData()
        {

        }

        public void AssignNamesAndDefaults(string shopName, string itemName)
        {
            ShopName = shopName;
            ItemName = itemName;

            if (Materials is { Count: >= 1 } && Currency == null && Price is null or <= 0)
            {
                Currency = "Money";
                Price = 0;
            }
        }
    }
}
