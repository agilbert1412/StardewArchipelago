using System.Collections.Generic;
using Newtonsoft.Json;
using StardewArchipelago.Locations.CodeInjections.Vanilla.CC;
using StardewArchipelago.Stardew;
using StardewValley;

namespace StardewArchipelago.Bundles
{
    public class BundlesManager
    {
        private static Dictionary<string, string> _vanillaBundleData;
        private Dictionary<string, string> _currentBundlesData;
        private BundleRooms BundleRooms { get; }

        public BundlesManager(StardewItemManager itemManager, string bundlesJson)
        {
            var bundlesDictionary = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Dictionary<string, string>>>>(bundlesJson);
            BundleRooms = new BundleRooms(itemManager, bundlesDictionary);
            _vanillaBundleData = Game1.content.LoadBase<Dictionary<string, string>>("Data\\Bundles");
            _currentBundlesData = BundleRooms.ToStardewStrings();
        }

        public void ReplaceAllBundles()
        {
            Game1.netWorldState.Value.SetBundleData(_currentBundlesData);
        }
    }
}
