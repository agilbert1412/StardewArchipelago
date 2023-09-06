using System.Linq;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Locations.CodeInjections.Vanilla.MonsterSlayer;
using StardewModdingAPI;
using StardewArchipelago.Stardew;

namespace StardewArchipelago.Locations.CodeInjections.Initializers
{
    public static class CodeInjectionInitializer
    {
        public static void Initialize(IMonitor monitor, IModHelper modHelper, ArchipelagoClient archipelago, BundleReader bundleReader, LocationChecker locationChecker, StardewItemManager itemManager, ArchipelagoStateDto archipelagoState)
        public static void Initialize(IMonitor monitor, IModHelper modHelper, ArchipelagoClient archipelago, BundleReader bundleReader, LocationChecker locationChecker, StardewItemManager itemManager, WeaponsManager weaponsManager)
        {
            VanillaCodeInjectionInitializer.Initialize(monitor, modHelper, archipelago, bundleReader, locationChecker, itemManager, weaponsManager);
            if (archipelago.SlotData.Mods.IsModded)
            {
                ModCodeInjectionInitializer.Initialize(monitor, modHelper, archipelago, locationChecker);
            }
        }
    }
}
