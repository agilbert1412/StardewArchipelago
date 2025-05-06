using System.Collections.Generic;

namespace StardewArchipelago.Bundles
{
    public class MemeItemBundle : Bundle
    {
        public int NumberRequired { get; set; }
        public List<MemeBundleItem> Items { get; }

        public MemeItemBundle(string roomName, string bundleName, Dictionary<string, string> bundleContent) : base(roomName, bundleName)
        {
            NumberRequired = int.Parse(bundleContent[NUMBER_REQUIRED_KEY]);
            Items = new List<MemeBundleItem>();
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
                var bundleItem = new MemeBundleItem(itemName, amount, quality);
                Items.Add(bundleItem);
            }
        }

        public override string GetItemsString()
        {
            var itemsString = "";
            foreach (var item in Items)
            {
                itemsString += $" {item.MemeItemId} {item.Amount} {item.Quality}";
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
