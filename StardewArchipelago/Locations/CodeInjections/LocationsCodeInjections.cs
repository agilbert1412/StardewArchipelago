using StardewArchipelago.Archipelago;
using StardewModdingAPI;
using StardewArchipelago.Stardew;

namespace StardewArchipelago.Locations.CodeInjections
{
    public static class LocationsCodeInjections
    {
        public static void Initialize(IMonitor monitor, IModHelper modHelper, ArchipelagoClient archipelago, BundleReader bundleReader, LocationChecker locationChecker)
        {
            BackpackInjections.Initialize(monitor, locationChecker);
            ToolInjections.Initialize(monitor, modHelper, locationChecker);
            ScytheInjections.Initialize(monitor, locationChecker);
            FishingRodInjections.Initialize(monitor, modHelper, archipelago, locationChecker);
            CommunityCenterInjections.Initialize(monitor, bundleReader, locationChecker);
            MineshaftInjections.Initialize(monitor, archipelago, locationChecker);
            SkillInjections.Initialize(monitor, modHelper, archipelago, locationChecker);
            QuestInjections.Initialize(monitor, archipelago, locationChecker);
            ArcadeMachineInjections.Initialize(monitor, modHelper, locationChecker);
            CarpenterInjections.Initialize(monitor, modHelper, archipelago, locationChecker);
        }
    }
}
