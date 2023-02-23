using StardewArchipelago.Archipelago;
using StardewModdingAPI;
using StardewArchipelago.Stardew;

namespace StardewArchipelago.Locations.CodeInjections
{
    public static class LocationsCodeInjections
    {
        public static void Initialize(IMonitor monitor, IModHelper modHelper, ArchipelagoClient archipelago, BundleReader bundleReader, LocationChecker locationChecker, StardewItemManager itemManager)
        {
            BackpackInjections.Initialize(monitor, locationChecker);
            ToolInjections.Initialize(monitor, modHelper, archipelago, locationChecker);
            ScytheInjections.Initialize(monitor, locationChecker);
            FishingRodInjections.Initialize(monitor, modHelper, archipelago, locationChecker);
            CommunityCenterInjections.Initialize(monitor, bundleReader, locationChecker);
            MineshaftInjections.Initialize(monitor, archipelago, locationChecker);
            SkillInjections.Initialize(monitor, modHelper, archipelago, locationChecker);
            QuestInjections.Initialize(monitor, modHelper, archipelago, locationChecker);
            CarpenterInjections.Initialize(monitor, modHelper, archipelago, locationChecker);
            WizardInjections.Initialize(monitor, modHelper, archipelago, locationChecker);
            IsolatedEventInjections.Initialize(monitor, modHelper, archipelago, locationChecker);
            AdventurerGuildInjections.Initialize(monitor, modHelper, archipelago, locationChecker);
            ArcadeMachineInjections.Initialize(monitor, modHelper, archipelago, locationChecker);
            TravelingMerchantInjections.Initialize(monitor, modHelper, archipelago, locationChecker);
            FishingInjections.Initialize(monitor, modHelper, archipelago, locationChecker, itemManager);
            MuseumInjections.Initialize(monitor, modHelper, archipelago, locationChecker, itemManager);
        }
    }
}
