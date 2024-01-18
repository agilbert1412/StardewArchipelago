using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Goals;
using StardewArchipelago.Stardew.NameMapping;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using Object = StardewValley.Object;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public class NightShippingBehaviors
    {
        public const string SHIPSANITY_PREFIX = "Shipsanity: ";

        private IMonitor _monitor;
        private ArchipelagoClient _archipelago;
        private LocationChecker _locationChecker;
        private NameSimplifier _nameSimplifier;
        private List<string> IgnoredShipments = new(){
            //  For items that could be shipped, but are too easily softlockable to be reasonably shipped, to avoid errors
            "Galmoran Gem"
        };

        public NightShippingBehaviors(IMonitor monitor, ArchipelagoClient archipelago, LocationChecker locationChecker, NameSimplifier nameSimplifier)
        {
            _monitor = monitor;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            _nameSimplifier = nameSimplifier;
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

                _monitor.Log($"Currently attempting to check shipsanity locations for the current day", LogLevel.Info);
                var allShippedItems = GetAllItemsShippedToday();
                _monitor.Log($"{allShippedItems.Count} items shipped", LogLevel.Info);
                CheckAllShipsanityLocations(allShippedItems);
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(CheckShipsanityLocationsBeforeSleep)}:\n{ex}", LogLevel.Error);
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

                    allShippedItems.AddRange(chest.items);
                }
            }

            return allShippedItems;
        }

        private static IEnumerable<GameLocation> GetAllGameLocations()
        {
            foreach (var location in Game1.locations)
            {
                yield return location;
                if (location is not BuildableGameLocation buildableLocation)
                {
                    continue;
                }

                foreach (var building in buildableLocation.buildings.Where(building => building.indoors.Value != null))
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
                if (IgnoredShipments.Contains(name))
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
                    _monitor.Log($"Unrecognized Shipsanity Location: {name} [{shippedItem.ParentSheetIndex}]", LogLevel.Error);
                }
            }
        }
    }
}
