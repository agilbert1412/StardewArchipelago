using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Bundles;
using StardewArchipelago.GameModifications;
using StardewArchipelago.Locations.CodeInjections.Vanilla.MonsterSlayer;
using StardewArchipelago.Locations.CodeInjections.Vanilla.Relationship;
using StardewArchipelago.Serialization;
using StardewArchipelago.Stardew;
using StardewModdingAPI;

namespace StardewArchipelago.Locations.CodeInjections.Initializers
{
    public static class CodeInjectionInitializer
    {
        public static void Initialize(ILogger logger, IModHelper modHelper, ModConfig config, StardewArchipelagoClient archipelago, ArchipelagoStateDto state,
            LocationChecker locationChecker, StardewItemManager itemManager, WeaponsManager weaponsManager, BundlesManager bundlesManager,
            SeedShopStockModifier seedShopStockModifier, Friends friends)
        {
            VanillaCodeInjectionInitializer.Initialize(logger, modHelper, config, archipelago, state, locationChecker, itemManager, weaponsManager, bundlesManager, friends);
            if (archipelago.SlotData.Mods.IsModded)
            {
                ModCodeInjectionInitializer.Initialize(logger, modHelper, archipelago, locationChecker, seedShopStockModifier);
            }
        }
    }
}
