using StardewArchipelago.Archipelago;
using StardewModdingAPI;
using StardewArchipelago.Constants.Modded;
using StardewArchipelago.GameModifications;
using StardewArchipelago.GameModifications.CodeInjections.Modded;
using StardewArchipelago.Locations.CodeInjections.Modded;
using StardewArchipelago.Locations.CodeInjections.Modded.SVE;
using StardewArchipelago.GameModifications.Modded;

namespace StardewArchipelago.Locations.CodeInjections.Initializers
{
    public static class ModCodeInjectionInitializer
    {
        static ArchipelagoClient _archipelago;

        public static void Initialize(IMonitor monitor, IModHelper modHelper, ArchipelagoClient archipelago, LocationChecker locationChecker, ShopReplacer shopReplacer, SeedShopStockModifier seedShopStockModifier, JunimoShopGenerator junimoShopGenerator)
        {
            _archipelago = archipelago;
            InitializeModdedContent(monitor, modHelper, archipelago, locationChecker, shopReplacer, seedShopStockModifier, junimoShopGenerator);
        }

        private static void InitializeModdedContent(IMonitor monitor, IModHelper modHelper, ArchipelagoClient archipelago, LocationChecker locationChecker, ShopReplacer shopReplacer, SeedShopStockModifier seedShopStockModifier, JunimoShopGenerator junimoShopGenerator)
        {
            if (_archipelago.SlotData.Mods.HasMod(ModNames.DEEP_WOODS))
            {
                DeepWoodsModInjections.Initialize(monitor, modHelper, archipelago, locationChecker);
            }
            if (_archipelago.SlotData.Mods.HasMod(ModNames.MAGIC))
            {
                MagicModInjections.Initialize(monitor, modHelper, archipelago, locationChecker, shopReplacer);
            }
            if (_archipelago.SlotData.Mods.HasMod(ModNames.SOCIALIZING))
            {
                SocializingConfigCodeInjections.Initialize(monitor, modHelper, archipelago);
            }
            if (_archipelago.SlotData.Mods.HasMod(ModNames.ARCHAEOLOGY))
            {
                ArchaeologyConfigCodeInjections.Initialize(monitor, modHelper, archipelago);
            }
            if (_archipelago.SlotData.Mods.HasMod(ModNames.SKULL_CAVERN_ELEVATOR))
            {
                SkullCavernInjections.Initialize(monitor, modHelper, archipelago, locationChecker);
            }
            if (archipelago.SlotData.Mods.ModIsInstalledAndLoaded(modHelper, "SpaceCore"))
            {
                NewSkillsPageInjections.Initialize(monitor, modHelper, archipelago, locationChecker);
            }
            if (archipelago.SlotData.Mods.HasMod(ModNames.SVE))
            {
                SVECutsceneInjections.Initialize(monitor, modHelper, archipelago, locationChecker);
                SVEShopInjections.Initialize(monitor, modHelper, archipelago, locationChecker, shopReplacer, seedShopStockModifier, junimoShopGenerator);
            }

            if (archipelago.SlotData.Mods.HasMod(ModNames.DISTANT_LANDS)) // Only mod for now that needs it.
            {
                ModdedEventInjections.Initialize(monitor, modHelper, archipelago, locationChecker);
            }
            if (archipelago.SlotData.Mods.HasMod(ModNames.BOARDING_HOUSE))
            {
                BoardingHouseInjections.Initialize(monitor, locationChecker, archipelago, shopReplacer);
            }
        }
    }
}
