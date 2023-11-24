using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
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
            foreach (var (itemName, itemDetails) in bundleContent)
            {
                if (itemName == NUMBER_REQUIRED_KEY)
                {
                    continue;
                }

                var splitDetails = itemDetails.Split(' ');
                var amount = int.Parse(splitDetails[0]);
                var quality = splitDetails[splitDetails.Length - 2];
                var bundleItem = new BundleItem(itemManager, itemName, amount, quality);
                Items.Add(bundleItem);
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

        public override string GetNumberRequiredItemsWithSeparator()
        {
            if (NumberRequired == Items.Count)
            {
                return "";
            }

            return $"/{NumberRequired}";
        }
    }
}
