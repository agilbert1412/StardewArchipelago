using KaitoKid.ArchipelagoUtilities.Net;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Logging;
using StardewArchipelago.Serialization;
using StardewModdingAPI;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles.Remakes
{
    public class ArchipelagoWrapper
    {
        public LogHandler Logger { get; }
        public IModHelper ModHelper { get; }
        public StardewArchipelagoClient Archipelago { get; }
        public ArchipelagoStateDto State { get; }
        public LocationChecker LocationChecker { get; }

        protected ArchipelagoWrapper(LogHandler logger, IModHelper modHelper, StardewArchipelagoClient archipelago, ArchipelagoStateDto state, LocationChecker locationChecker)
        {
            Logger = logger;
            ModHelper = modHelper;
            Archipelago = archipelago;
            State = state;
            LocationChecker = locationChecker;
        }
    }
}
