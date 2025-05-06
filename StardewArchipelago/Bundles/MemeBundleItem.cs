using StardewArchipelago.Constants;
using StardewValley.Menus;

namespace StardewArchipelago.Bundles
{
    public class MemeBundleItem
    {
        public string MemeItem { get; set; }
        public string MemeItemId { get; set; }
        public int Amount { get; set; }
        public int Quality { get; set; }

        public MemeBundleItem(string itemName, int amount, string quality)
        {
            MemeItem = itemName;
            MemeItemId = MemeIDProvider.MemeItemIds[itemName];
            Amount = amount;
            Quality = BundleItem.QualityTable[quality];
        }

        public BundleIngredientDescription CreateBundleIngredientDescription(bool completed)
        {
            var bundleIngredient = new BundleIngredientDescription(MemeItemId, Amount, Quality, completed);
            return bundleIngredient;
        }
    }
}
