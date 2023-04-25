using StardewArchipelago.Archipelago;
using StardewArchipelago.Locations.GingerIsland.Boat;
using StardewArchipelago.Locations.GingerIsland.Parrots;
using StardewModdingAPI;

namespace StardewArchipelago.Locations.CodeInjections
{
    public static class GingerIslandInitializer
    {
        public static void Initialize(IMonitor monitor, IModHelper modHelper, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            BoatTunnelInjections.Initialize(monitor, modHelper, archipelago, locationChecker);
            IslandSouthInjections.Initialize(monitor, modHelper, archipelago, locationChecker);
            IslandHutInjections.Initialize(monitor, modHelper, archipelago, locationChecker);
            IslandNorthInjections.Initialize(monitor, modHelper, archipelago, locationChecker);
            IslandWestInjections.Initialize(monitor, modHelper, archipelago, locationChecker);
            VolcanoDungeonInjections.Initialize(monitor, modHelper, archipelago, locationChecker);
        }
    }
}
