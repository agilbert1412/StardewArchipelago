using StardewValley;

namespace StardewArchipelago.Locations.CodeInjections.Modded
{
    public class PricedItem
    {
        public string ItemName { get; set; }
        public ItemStockInformation Price { get; set; }

        public PricedItem(string itemName, int priceInGold) : this(itemName, new ItemStockInformation(priceInGold, 1))
        {
        }

        public PricedItem(string itemName, ItemStockInformation price)
        {
            ItemName = itemName;
            Price = price;
        }
    }
}
