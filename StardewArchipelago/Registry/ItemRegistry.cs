using System;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.Gifting;
using StardewArchipelago.Locations;
using StardewArchipelago.Locations.CodeInjections.Vanilla.MonsterSlayer;
using StardewArchipelago.Locations.InGameLocations;
using StardewArchipelago.Registry.Definition;
using StardewArchipelago.Serialization;
using StardewArchipelago.Stardew;

namespace StardewArchipelago.Registry
{
    public class ItemRegistry : IRegistry
    {
        private ILogger _logger;

        public ItemRegistry(ILogger logger)
        {
            _logger = logger;
        }

        public void Initialize(StardewArchipelagoClient archipelago, StardewItemManager stardewItemManager, StardewLocationChecker locationChecker, IGiftHandler _giftHandler, WeaponsManager weaponsManager, ArchipelagoStateDto state)
        {
        }

        public void RegisterOnModEntry()
        {
            try
            {
                StardewValley.ItemRegistry.AddTypeDefinition(new ArchipelagoLocationDataDefinition());
                StardewValley.ItemRegistry.AddTypeDefinition(new MemeBundleItemDataDefinition());
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(ItemRegistry)} failed during {nameof(RegisterOnModEntry)}: {ex}");
            }
        }
    }
}
