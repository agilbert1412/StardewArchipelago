using System.Collections.Generic;
using StardewArchipelago.Locations.CodeInjections.Vanilla;
using StardewValley;

namespace StardewArchipelago.Archipelago
{
    public class BundlesManager
    {
        private static Dictionary<string, string> _vanillaBundleData;
        private Dictionary<string, string> _modifiedBundlesData;

        public BundlesManager(Dictionary<string, string> modifiedBundlesData)
        {
            _vanillaBundleData = null;
            _modifiedBundlesData = modifiedBundlesData;
        }

        public void ReplaceAllBundles()
        {
            if (_vanillaBundleData == null)
            {
                _vanillaBundleData = Game1.content.LoadBase<Dictionary<string, string>>("Data\\Bundles");
            }

            Game1.netWorldState.Value.SetBundleData(_vanillaBundleData);
            foreach (var (bundleKey, newBundleData) in _modifiedBundlesData)
            {
                var oldBundle = Game1.netWorldState.Value.BundleData[bundleKey];
                var oldBundleName = oldBundle.Split("/")[0];
                var newBundleName = newBundleData.Split("/")[0];
                CommunityCenterInjections.BundleNames.Add(newBundleName, oldBundleName);
                Game1.netWorldState.Value.BundleData[bundleKey] = newBundleData;
            }
        }
    }
}
