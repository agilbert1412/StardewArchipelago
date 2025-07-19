﻿using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.Gifting;
using StardewArchipelago.Bundles;
using StardewArchipelago.GameModifications;
using StardewArchipelago.Items.Traps;
using StardewArchipelago.Locations.CodeInjections.Vanilla.MonsterSlayer;
using StardewArchipelago.Locations.CodeInjections.Vanilla.Relationship;
using StardewArchipelago.Logging;
using StardewArchipelago.Serialization;
using StardewArchipelago.Stardew;
using StardewArchipelago.Stardew.NameMapping;
using StardewModdingAPI;

namespace StardewArchipelago.Locations.CodeInjections.Initializers
{
    public static class CodeInjectionInitializer
    {
        public static void Initialize(LogHandler logger, IModHelper modHelper, ModConfig config, StardewArchipelagoClient archipelago, ArchipelagoStateDto state,
            StardewLocationChecker locationChecker, JojaLocationChecker jojaLocationChecker, StardewItemManager itemManager, WeaponsManager weaponsManager, BundlesManager bundlesManager,
            SeedShopStockModifier seedShopStockModifier, Friends friends, TrapManager trapManager, BankHandler bank, NameSimplifier nameSimplifier, GiftSender giftSender)
        {
            VanillaCodeInjectionInitializer.Initialize(logger, modHelper, config, archipelago, state, locationChecker, jojaLocationChecker, itemManager, weaponsManager, bundlesManager, friends, trapManager, bank, nameSimplifier, giftSender);
            if (archipelago.SlotData.Mods.IsModded)
            {
                ModCodeInjectionInitializer.Initialize(logger, modHelper, archipelago, locationChecker, seedShopStockModifier);
            }
        }
    }
}
