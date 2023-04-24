using StardewArchipelago.Archipelago;
using StardewArchipelago.Locations.Festival;
using StardewArchipelago.Locations.GingerIsland.Boat;
using StardewModdingAPI;
using StardewArchipelago.Stardew;

namespace StardewArchipelago.Locations.CodeInjections
{
    public static class GingerIslandInitializer
    {
        public static void Initialize(IMonitor monitor, IModHelper modHelper, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            BoatTunnelInjections.Initialize(monitor, modHelper, archipelago, locationChecker);
        }
    }
}
