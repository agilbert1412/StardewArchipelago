using StardewArchipelago.Archipelago;
using StardewModdingAPI;
using System;
using StardewArchipelago.Stardew;

namespace StardewArchipelago.Locations.CodeInjections
{
    internal class LocationsCodeInjection
    {
        private BackpackInjections _backpackInjections;
        private ToolInjections _toolInjections;
        private ScytheInjections _scytheInjections;
        private FishingRodInjections _fishingRodInjections;
        private CommunityCenterInjections _communityCenterInjections;
        private MineshaftInjections _mineshaftInjections;

        public LocationsCodeInjection(IMonitor monitor, IModHelper modHelper, ArchipelagoClient archipelago, BundleReader bundleReader, Action<string> addCheckedLocation)
        {
            _backpackInjections = new BackpackInjections(monitor, addCheckedLocation);
            _toolInjections = new ToolInjections(monitor, modHelper, addCheckedLocation);
            _scytheInjections = new ScytheInjections(monitor, addCheckedLocation);
            _fishingRodInjections = new FishingRodInjections(monitor, modHelper, archipelago, addCheckedLocation);
            _communityCenterInjections = new CommunityCenterInjections(monitor, bundleReader, addCheckedLocation);
            _mineshaftInjections = new MineshaftInjections(monitor, addCheckedLocation);
        }
    }
}
