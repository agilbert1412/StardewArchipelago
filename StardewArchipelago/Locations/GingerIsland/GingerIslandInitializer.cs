﻿using StardewArchipelago.Archipelago;
using StardewArchipelago.Locations.GingerIsland;
using StardewArchipelago.Locations.GingerIsland.Boat;
using StardewArchipelago.Locations.GingerIsland.Parrots;
using StardewArchipelago.Locations.GingerIsland.VolcanoForge;
using StardewArchipelago.Locations.GingerIsland.WalnutRoom;
using StardewModdingAPI;

namespace StardewArchipelago.Locations.CodeInjections
{
    public static class GingerIslandInitializer
    {
        public static void Initialize(IMonitor monitor, IModHelper modHelper, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            WalnutRoomDoorInjection.Initialize(monitor, modHelper, archipelago, locationChecker);
            BoatTunnelInjections.Initialize(monitor, modHelper, archipelago, locationChecker);
            IslandSouthInjections.Initialize(monitor, modHelper, archipelago, locationChecker);
            IslandHutInjections.Initialize(monitor, modHelper, archipelago, locationChecker);
            IslandNorthInjections.Initialize(monitor, modHelper, archipelago, locationChecker);
            IslandWestInjections.Initialize(monitor, modHelper, archipelago, locationChecker);
            VolcanoDungeonInjections.Initialize(monitor, modHelper, archipelago, locationChecker);
            CalderaInjections.Initialize(monitor, modHelper, archipelago, locationChecker);
            FieldOfficeInjections.Initialize(monitor, modHelper, archipelago, locationChecker);
            IslandLocationInjections.Initialize(monitor, modHelper, archipelago);
        }
    }
}
