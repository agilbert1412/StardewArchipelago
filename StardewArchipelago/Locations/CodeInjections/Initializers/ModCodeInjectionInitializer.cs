using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.Utilities.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants.Modded;
using StardewArchipelago.GameModifications;
using StardewArchipelago.GameModifications.CodeInjections.Modded;
using StardewArchipelago.Locations.CodeInjections.Modded;
using StardewArchipelago.Locations.CodeInjections.Modded.SVE;
using StardewModdingAPI;

namespace StardewArchipelago.Locations.CodeInjections.Initializers
{
    public static class ModCodeInjectionInitializer
    {
        private static StardewArchipelagoClient _archipelago;
        private const string BEAR_KNOWLEDGE = "Bear's Knowledge";
        private const int OATMEAL_PRICE = 12500;
        private const int COOKIE_PRICE = 8750;

        public static void Initialize(ILogger logger, IModHelper modHelper, StardewArchipelagoClient archipelago, LocationChecker locationChecker, SeedShopStockModifier seedShopStockModifier)
        {
            _archipelago = archipelago;
            InitializeModdedContent(logger, modHelper, locationChecker, seedShopStockModifier);
        }

        private static void InitializeModdedContent(ILogger logger, IModHelper modHelper, LocationChecker locationChecker, SeedShopStockModifier seedShopStockModifier)
        {
            if (_archipelago.SlotData.Mods.HasMod(ModNames.DEEP_WOODS))
            {
                DeepWoodsModInjections.Initialize(logger, modHelper, _archipelago, locationChecker);
            }
            if (_archipelago.SlotData.Mods.HasMod(ModNames.MAGIC))
            {
                MagicModInjections.Initialize(logger, modHelper, _archipelago, locationChecker);
            }
            if (_archipelago.SlotData.Mods.HasMod(ModNames.ARCHAEOLOGY))
            {
                ArchaeologyConfigCodeInjections.Initialize(logger, modHelper, _archipelago);
            }
            if (_archipelago.SlotData.Mods.HasMod(ModNames.SKULL_CAVERN_ELEVATOR))
            {
                SkullCavernInjections.Initialize(logger, modHelper, _archipelago, locationChecker);
            }
            if (_archipelago.SlotData.Mods.HasMod(ModNames.SVE))
            {
                SVECutsceneInjections.Initialize(logger, modHelper, _archipelago, locationChecker);
            }
            if (_archipelago.SlotData.Mods.HasMod(ModNames.BOARDING_HOUSE))
            {
                BoardingHouseInjections.Initialize(logger, locationChecker, _archipelago);
            }
        }
    }
}
