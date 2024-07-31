using KaitoKid.ArchipelagoUtilities.Net.Client;
using StardewArchipelago.Locations.GingerIsland;
using StardewArchipelago.Locations.GingerIsland.Boat;
using StardewArchipelago.Locations.GingerIsland.Parrots;
using StardewArchipelago.Locations.GingerIsland.VolcanoForge;
using StardewArchipelago.Locations.GingerIsland.WalnutRoom;
using StardewModdingAPI;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.ArchipelagoUtilities.Net;
using StardewArchipelago.Archipelago;

namespace StardewArchipelago.Locations.CodeInjections
{
    public static class GingerIslandInitializer
    {
        public static void Initialize(ILogger logger, IModHelper modHelper, StardewArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            WalnutRoomDoorInjection.Initialize(logger, modHelper, archipelago, locationChecker);
            BoatTunnelInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            IslandSouthInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            IslandHutInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            IslandNorthInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            IslandWestInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            VolcanoDungeonInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            CalderaInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            FieldOfficeInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            IslandLocationInjections.Initialize(logger, modHelper, archipelago);
        }
    }
}
