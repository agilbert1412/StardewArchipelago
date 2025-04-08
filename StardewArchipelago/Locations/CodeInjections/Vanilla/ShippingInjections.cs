using System;
using System.Collections.Generic;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.SlotData;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;
using StardewArchipelago.Constants.Modded;
using StardewArchipelago.Stardew.NameMapping;
using StardewValley;
using StardewValley.Objects;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public class NightShippingBehaviors
    {
        public const string SHIPSANITY_PREFIX = "Shipsanity: ";

        private readonly ILogger _logger;
        private readonly StardewArchipelagoClient _archipelago;
        private readonly LocationChecker _locationChecker;
        private readonly NameSimplifier _nameSimplifier;
        private readonly CompoundNameMapper _nameMapper;

        public NightShippingBehaviors(ILogger logger, StardewArchipelagoClient archipelago, LocationChecker locationChecker, NameSimplifier nameSimplifier)
        {
            _logger = logger;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            _nameSimplifier = nameSimplifier;
            _nameMapper = new CompoundNameMapper(archipelago.SlotData);
        }

        // private static IEnumerator<int> _newDayAfterFade()
        public void CheckShipsanityLocationsBeforeSleep()
        {
            try
            {
                if (_archipelago.SlotData.Shipsanity == Shipsanity.None)
                {
                    return;
                }

                _logger.LogInfo($"Currently attempting to check shipsanity locations for the current day");
                var allShippedItems = GetAllItemsShippedToday();
                _logger.LogInfo($"{allShippedItems.Count} items shipped");
                CheckAllShipsanityLocations(allShippedItems);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CheckShipsanityLocationsBeforeSleep)}:\n{ex}");
            }
        }

        private static List<Item> GetAllItemsShippedToday()
        {
            var allShippedItems = new List<Item>();
            allShippedItems.AddRange(Game1.getFarm().getShippingBin(Game1.player));
            foreach (var gameLocation in GetAllGameLocations())
            {
                foreach (var locationObject in gameLocation.Objects.Values)
                {
                    if (locationObject is not Chest { SpecialChestType: Chest.SpecialChestTypes.MiniShippingBin } chest)
                    {
                        continue;
                    }

                    allShippedItems.AddRange(chest.Items);
                }
            }

            return allShippedItems;
        }

        private static IEnumerable<GameLocation> GetAllGameLocations()
        {
            foreach (var location in Game1.locations)
            {
                yield return location;
                if (!location.IsBuildableLocation())
                {
                    continue;
                }

                foreach (var building in location.buildings.Where(building => building.indoors.Value != null))
                {
                    yield return building.indoors.Value;
                }
            }
        }

        private void CheckAllShipsanityLocations(List<Item> allShippedItems)
        {
            foreach (var shippedItem in allShippedItems)
            {
                var name = _nameSimplifier.GetSimplifiedName(shippedItem);
                name = _nameMapper.GetEnglishName(name); // For the Name vs Display Name discrepencies in Mods.
                if (IgnoredModdedStrings.Shipments.Contains(name))
                {
                    continue;
                }
                var apLocation = $"{SHIPSANITY_PREFIX}{name}";
                if (_archipelago.GetLocationId(apLocation) > -1)
                {
                    _locationChecker.AddCheckedLocation(apLocation);
                }
                else
                {
                    var wasSuccessful = DoBugsCleanup(shippedItem);
                    if (wasSuccessful)
                    {
                        continue;
                    }
                    _logger.LogError($"Unrecognized Shipsanity Location: {name} [{shippedItem.ParentSheetIndex}]");
                }
            }
        }

        private bool DoBugsCleanup(Item shippedItem)
        {
            // In the beta async, backend names for SVE shippables are the internal names.  This fixes the mistake ONLY for that beta async.  Remove after it.
            var name = _nameSimplifier.GetSimplifiedName(shippedItem);
            var sveMappedItems = new List<string>() { "Smelly Rafflesia", "Bearberrys", "Big Conch", "Dried Sand Dollar", "Lucky Four Leaf Clover", "Ancient Ferns Seed" };
            if (sveMappedItems.Contains(name))
            {
                var apLocation = $"{SHIPSANITY_PREFIX}{name}";
                if (_archipelago.GetLocationId(apLocation) > -1)
                {
                    _logger.LogWarning($"Bugfix caught this for the beta async.  If this isn't that game, let the developers know there's a bug!");
                    _locationChecker.AddCheckedLocation(apLocation);
                    return true;
                }
            }
            return false;
        }
    }
}
