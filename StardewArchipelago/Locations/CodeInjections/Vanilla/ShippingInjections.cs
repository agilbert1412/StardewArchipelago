using System;
using System.Collections.Generic;
using System.Linq;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Goals;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using Object = StardewValley.Object;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public class NightShippingBehaviors
    {
        private IMonitor _monitor;
        private ArchipelagoClient _archipelago;
        private LocationChecker _locationChecker;

        public NightShippingBehaviors(IMonitor monitor, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _monitor = monitor;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
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
                var name = GetShippedItemName(shippedItem);

                var apLocation = $"Shipsanity: {name}";
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

        private static string GetShippedItemName(Item shippedItem)
        {
            var name = shippedItem.Name;
            if (_renamedItems.ContainsKey(shippedItem.ParentSheetIndex))
            {
                name = _renamedItems[shippedItem.ParentSheetIndex];
            }

            if (name.Contains("moonslime.excavation."))
            {
                name = shippedItem.DisplayName.Replace("Woooden", "Wooden"); //Temporary fix; will break for chinese speaking players only atm
            }

            if (shippedItem is not Object shippedObject)
            {
                return name;
            }

            foreach (var simplifiedName in _simplifiedNames)
            {
                if (name.Contains(simplifiedName))
                {
                    return simplifiedName;
                }
            }

            if (shippedObject.preserve.Value.HasValue)
            {
                switch (shippedObject.preserve.Value.GetValueOrDefault())
                {
                    case Object.PreserveType.Wine:
                        return "Wine";
                    case Object.PreserveType.Jelly:
                        return "Jelly";
                    case Object.PreserveType.Pickle:
                        return "Pickles";
                    case Object.PreserveType.Juice:
                        return "Juice";
                    case Object.PreserveType.Roe:
                        return "Roe";
                    case Object.PreserveType.AgedRoe:
                        return "Aged Roe";
                }
            }

            return name;
        }

        private static readonly Dictionary<int, string> _renamedItems = new()
        {
            { 180, "Egg (Brown)" },
            { 182, "Large Egg (Brown)" },
            { 438, "Large Goat Milk" },
            { 223, "Cookies" },
        };

        private static readonly List<string> _simplifiedNames = new()
        {
            "Honey",
            "Secret Note",
        };
    }
}
