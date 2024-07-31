using System.Collections.Generic;
using HarmonyLib;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Bundles;
using StardewArchipelago.GameModifications;
using StardewArchipelago.Locations.CodeInjections.Initializers;
using StardewArchipelago.Locations.CodeInjections.Vanilla.MonsterSlayer;
using StardewArchipelago.Locations.CodeInjections.Vanilla.Relationship;
using StardewArchipelago.Logging;
using StardewArchipelago.Serialization;
using StardewArchipelago.Stardew;
using StardewModdingAPI;

namespace StardewArchipelago.Locations.Patcher
{
    public class LocationPatcher
    {
        private List<ILocationPatcher> _patchers;

        public LocationPatcher(LogHandler logger, IModHelper modHelper, ModConfig config, Harmony harmony, StardewArchipelagoClient archipelago, ArchipelagoStateDto state,
            StardewLocationChecker locationChecker, StardewItemManager itemManager, WeaponsManager weaponsManager, BundlesManager bundlesManager,
            SeedShopStockModifier seedShopStockModifier, Friends friends)
        {
            CodeInjectionInitializer.Initialize(logger, modHelper, config, archipelago, state, locationChecker, itemManager, weaponsManager, bundlesManager, seedShopStockModifier, friends);
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
