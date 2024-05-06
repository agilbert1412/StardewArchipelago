using System;
using System.Collections.Generic;
using StardewArchipelago.Stardew;

namespace StardewArchipelago.Bundles
{
    public class ItemBundle : Bundle
    {
        public int NumberRequired { get; set; }
        public List<BundleItem> Items { get; }

        public ItemBundle(StardewItemManager itemManager, string roomName, string bundleName, Dictionary<string, string> bundleContent) : base(roomName, bundleName)
        {
            NumberRequired = int.Parse(bundleContent[NUMBER_REQUIRED_KEY]);
            Items = new List<BundleItem>();
            foreach (var (key, itemDetails) in bundleContent)
            {
                if (key == NUMBER_REQUIRED_KEY)
                {
                    continue;
                }

                var itemFields = itemDetails.Split("|");
                var itemName = itemFields[0];
                var amount = int.Parse(itemFields[1]);
                var quality = itemFields[2].Split(" ")[0];
                if (itemName.Contains('[', StringComparison.InvariantCultureIgnoreCase))
                {
                    var flavorStart = itemName.IndexOf("[", StringComparison.InvariantCultureIgnoreCase);
                    var flavorEnd = itemName.IndexOf("]", StringComparison.InvariantCultureIgnoreCase);
                    var flavorLength = flavorEnd - flavorStart - 1;
                    var flavorItemName = itemName.Substring(flavorStart + 1, flavorLength);
                    var flavoredItemName = itemName.Substring(0, flavorStart - 1);
                    var bundleItem = new BundleItem(itemManager, flavoredItemName, amount, quality, flavorItemName);
                    Items.Add(bundleItem);
                }
                else
                {
                    var bundleItem = new BundleItem(itemManager, itemName, amount, quality);
                    Items.Add(bundleItem);
                }
            }
        }

        public override string GetItemsString()
        {
            var itemsString = "";
            foreach (var item in Items)
            {
                itemsString += $" {item.StardewObject.Id} {item.Amount} {item.Quality}";
            }

            return itemsString.Trim();
        }

        public override string GetNumberRequiredItems()
        {
            if (NumberRequired == Items.Count)
            {
                return "";
            }

            return $"{NumberRequired}";
        }
    }
}
