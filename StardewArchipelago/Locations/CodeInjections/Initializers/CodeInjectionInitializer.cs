using StardewArchipelago.Archipelago;
using StardewArchipelago.GameModifications;
using StardewArchipelago.GameModifications.Modded;
using StardewArchipelago.Locations.CodeInjections.Vanilla.MonsterSlayer;
using StardewArchipelago.Locations.CodeInjections.Vanilla.Relationship;
using StardewArchipelago.Serialization;
using StardewArchipelago.Stardew;
using StardewModdingAPI;

namespace StardewArchipelago.Locations.CodeInjections.Initializers
{
    public static class CodeInjectionInitializer
    {
        public static void Initialize(IMonitor monitor, IModHelper modHelper, ModConfig config, ArchipelagoClient archipelago, ArchipelagoStateDto state, LocationChecker locationChecker, StardewItemManager itemManager, WeaponsManager weaponsManager, SeedShopStockModifier seedShopStockModifier, JunimoShopGenerator junimoShopGenerator, Friends friends)
        {
            VanillaCodeInjectionInitializer.Initialize(monitor, modHelper, config, archipelago, state, locationChecker, itemManager, weaponsManager, friends);
            if (archipelago.SlotData.Mods.IsModded)
            {
                ModCodeInjectionInitializer.Initialize(monitor, modHelper, archipelago, locationChecker, seedShopStockModifier, junimoShopGenerator);
            }
        }
    }
}
