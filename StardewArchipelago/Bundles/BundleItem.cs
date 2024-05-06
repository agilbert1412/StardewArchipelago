using System.Collections.Generic;
using StardewArchipelago.Stardew;

namespace StardewArchipelago.Bundles
{
    public class BundleItem
    {
        private static readonly Dictionary<string, int> QualityTable = new() { { "Basic", 0 }, { "Silver", 1 }, { "Gold", 2 }, { "Iridium", 3 } };
        public StardewObject StardewObject { get; set; }
        public int Amount { get; set; }
        public int Quality { get; set; }
        public StardewObject Flavor { get; set; }

        public BundleItem(StardewItemManager itemManager, string itemName, int amount, string quality)
        {
            StardewObject = itemManager.GetObjectByName(itemName);
            Amount = amount;
            Quality = QualityTable[quality];
            Flavor = null;
        }

        public BundleItem(StardewItemManager itemManager, string itemName, int amount, string quality, string flavorItemName) : this(itemManager, itemName, amount, quality)
        {
            Flavor = itemManager.GetObjectByName(flavorItemName);
        }
    }
}
