namespace StardewArchipelago.Stardew
{
    public class Bundle
    {
        public int BundleId { get; }
        public string BundleName { get; }
        public string BundleReward { get; }
        public string BundleItemsRequired { get; }
        public string BundleColor { get; }

        public Bundle(int bundleId, string bundleName, string bundleReward, string bundleItemsRequired, string bundleColor)
        {
            BundleId = bundleId;
            BundleName = bundleName;
            BundleReward = bundleReward;
            BundleItemsRequired = bundleItemsRequired;
            BundleColor = bundleColor;
        }
    }
}
