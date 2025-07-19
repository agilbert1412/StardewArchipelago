using System.Collections.Generic;
using HarmonyLib;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.Gifting;
using StardewArchipelago.Bundles;
using StardewArchipelago.GameModifications;
using StardewArchipelago.Items.Traps;
using StardewArchipelago.Locations.CodeInjections.Initializers;
using StardewArchipelago.Locations.CodeInjections.Vanilla.MonsterSlayer;
using StardewArchipelago.Locations.CodeInjections.Vanilla.Relationship;
using StardewArchipelago.Logging;
using StardewArchipelago.Serialization;
using StardewArchipelago.Stardew;
using StardewArchipelago.Stardew.NameMapping;
using StardewModdingAPI;

namespace StardewArchipelago.Locations.Patcher
{
    public class LocationPatcher
    {
        private readonly List<ILocationPatcher> _patchers;

        public LocationPatcher(LogHandler logger, IModHelper modHelper, ModConfig config, Harmony harmony, StardewArchipelagoClient archipelago, ArchipelagoStateDto state,
            StardewLocationChecker locationChecker, JojaLocationChecker jojaLocationChecker, StardewItemManager itemManager, WeaponsManager weaponsManager, BundlesManager bundlesManager,
            SeedShopStockModifier seedShopStockModifier, Friends friends, TrapManager trapManager, BankHandler bank, NameSimplifier nameSimplifier, GiftSender giftSender)
        {
            CodeInjectionInitializer.Initialize(logger, modHelper, config, archipelago, state, locationChecker, jojaLocationChecker, itemManager, weaponsManager, bundlesManager, seedShopStockModifier, friends, trapManager, bank, nameSimplifier, giftSender);
            _patchers = new List<ILocationPatcher>();
            _patchers.Add(new VanillaLocationPatcher(logger, modHelper, harmony, archipelago, locationChecker, itemManager));
            if (archipelago.SlotData.Mods.IsModded)
            {
                _patchers.Add(new ModLocationPatcher(harmony, logger, modHelper, archipelago, itemManager));
            }
        }

        public void ReplaceAllLocationsRewardsWithChecks()
        {
            foreach (var patcher in _patchers)
            {
                patcher.ReplaceAllLocationsRewardsWithChecks();
            }
        }

        public void CleanEvents()
        {
            foreach (var patcher in _patchers)
            {
                patcher.CleanEvents();
            }
        }
    }
}
