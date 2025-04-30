using KaitoKid.ArchipelagoUtilities.Net;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Logging;
using StardewArchipelago.Serialization;
using StardewArchipelago.Stardew;
using StardewModdingAPI;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles.Remakes
{
    public class ArchipelagoWrapperBundles : ArchipelagoWrapper
    {
        public BundleFactory Factory { get; }
        public BundleReader Reader { get; }

        public ArchipelagoWrapperBundles(LogHandler logger, IModHelper modHelper, StardewArchipelagoClient archipelago, ArchipelagoStateDto state, LocationChecker locationChecker, BundleFactory bundleFactory, BundleReader bundleReader) : base(logger, modHelper, archipelago, state, locationChecker)
        {
            Factory = bundleFactory;
            Reader = bundleReader;
        }
    }
}
