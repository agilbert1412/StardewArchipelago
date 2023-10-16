using StardewArchipelago.Archipelago;
using StardewModdingAPI;
using StardewArchipelago.Constants;
using StardewArchipelago.GameModifications.CodeInjections.Modded;
using StardewArchipelago.Locations.CodeInjections.Modded;

namespace StardewArchipelago.Locations.CodeInjections.Initializers
{
    public static class ModCodeInjectionInitializer
    {
        static ArchipelagoClient _archipelago;

        public static void Initialize(IMonitor monitor, IModHelper modHelper, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _archipelago = archipelago;
            InitializeModdedContent(monitor, modHelper, archipelago, locationChecker);
        }

        private static void InitializeModdedContent(IMonitor monitor, IModHelper modHelper, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            if (_archipelago.SlotData.Mods.HasMod(ModNames.DEEP_WOODS))
            {
                DeepWoodsModInjections.Initialize(monitor, modHelper, archipelago, locationChecker);
            }
            if (_archipelago.SlotData.Mods.HasMod(ModNames.MAGIC))
            {
                MagicModInjections.Initialize(monitor, modHelper, archipelago, locationChecker);
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
        }
    }
}
