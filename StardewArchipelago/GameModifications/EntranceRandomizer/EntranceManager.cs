using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Extensions;
using StardewModdingAPI;
using StardewValley;

namespace StardewArchipelago.GameModifications.EntranceRandomizer
{
    public class EntranceManager
    {
        private const string TRANSITIONAL_STRING = " to ";

        private readonly IMonitor _monitor;
        private Dictionary<string, string> _modifiedEntrances;

        private HashSet<string> _checkedEntrancesToday;
        private Dictionary<string, WarpRequest> generatedWarps;

        public EntranceManager(IMonitor monitor)
        {
            _monitor = monitor;
            generatedWarps = new Dictionary<string, WarpRequest>(StringComparer.OrdinalIgnoreCase);
            ResetCheckedEntrancesToday();
        }

        public void ResetCheckedEntrancesToday()
        {
            _checkedEntrancesToday = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        public void SetEntranceRandomizerSettings(SlotData slotData)
        {
            _modifiedEntrances = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (slotData.EntranceRandomization == EntranceRandomization.Disabled)
            {
                return;
            }

            foreach (var (originalEntrance, replacementEntrance) in slotData.ModifiedEntrances)
            {
                RegisterRandomizedEntrance(originalEntrance, replacementEntrance);
            }
        }

        private void RegisterRandomizedEntrance(string originalEntrance, string replacementEntrance)
        {
            var aliasedOriginal = TurnAliased(originalEntrance);
            var aliasedReplacement = TurnAliased(replacementEntrance);
            var originalEquivalentEntrances = _equivalentAreas.FirstOrDefault(x => x.Contains(aliasedOriginal)) ?? new[] { aliasedOriginal };
            var replacementEquivalentEntrances = _equivalentAreas.FirstOrDefault(x => x.Contains(aliasedReplacement)) ??
                                                 new[] { aliasedReplacement };
            foreach (var originalEquivalentEntrance in originalEquivalentEntrances)
            {
                foreach (var replacementEquivalentEntrance in replacementEquivalentEntrances)
                {
                    _modifiedEntrances.Add(originalEquivalentEntrance, replacementEquivalentEntrance);
                }
            }
        }

        public bool TryGetEntranceReplacement(string currentLocationName, string locationRequestName, out WarpRequest warpRequest)
        {
            warpRequest = null;
            var key = GetKey(currentLocationName, locationRequestName);
            if (!_modifiedEntrances.ContainsKey(key))
            {
                return false;
            }

            var desiredWarpName = _modifiedEntrances[key];

            if (_checkedEntrancesToday.Contains(desiredWarpName))
            {
                if (generatedWarps.ContainsKey(desiredWarpName))
                {
                    warpRequest = generatedWarps[desiredWarpName];
                    return true;
                }

                return false;
            }

            return TryFindWarpToDestination(desiredWarpName, out warpRequest);
        }

        private bool TryFindWarpToDestination(string desiredWarpKey, out WarpRequest warpRequest)
        {
            var currentTile = Game1.player.getTileLocationPoint();
            var (locationOriginName, locationDestinationName) = GetLocationNames(desiredWarpKey);
            _checkedEntrancesToday.Add(desiredWarpKey);

            if (!locationOriginName.TryGetClosestWarpPointTo(ref locationDestinationName, currentTile, out var locationOrigin, out var warpPoint))
            {
                warpRequest = null;
                return false;
            }

            var warpPointTarget = locationOrigin.GetWarpPointTarget(warpPoint, locationDestinationName);
            var locationDestination = Game1.getLocationFromName(locationDestinationName);
            var warpAwayPoint = locationDestination.GetClosestWarpPointTo(locationOriginName, warpPointTarget);
            var facingDirection = warpPointTarget.GetFacingAwayFrom(warpAwayPoint);
            var locationRequest = new LocationRequest(locationDestinationName, locationDestination.isStructure.Value,
                locationDestination);
            warpRequest = new WarpRequest(locationRequest, warpPointTarget.X, warpPointTarget.Y, facingDirection);
            generatedWarps[desiredWarpKey] = warpRequest;
            return true;
        }

        private static string GetKey(string currentLocationName, string locationRequestName)
        {
            var key = $"{currentLocationName}{TRANSITIONAL_STRING}{locationRequestName}";
            return key;
        }

        private static (string, string) GetLocationNames(string key)
        {
            var split = key.Split(TRANSITIONAL_STRING);
            return (split[0], split[1]);
        }

        private static string TurnAliased(string key)
        {
            if (key.Contains(TRANSITIONAL_STRING))
            {
                var parts = key.Split(TRANSITIONAL_STRING);
                var aliased1 = TurnAliased(parts[0]);
                var aliased2 = TurnAliased(parts[1]);
                return $"{aliased1}{TRANSITIONAL_STRING}{aliased2}";
            }

            var modifiedString = key;
            foreach (var (oldString, newString) in _aliases)
            {
                var customizedNewString = newString;
                if (customizedNewString.Contains("{0}"))
                {
                    customizedNewString = string.Format(newString, Game1.player.isMale ? "Mens" : "Womens");
                }
                modifiedString = modifiedString.Replace(oldString, customizedNewString);
            }

            return modifiedString;
        }

        private static readonly Dictionary<string, string> _aliases = new()
        {
            { "Mayor's Manor", "ManorHouse" },
            { "Pierre's General Store", "SeedShop" },
            { "Clint's Blacksmith", "Blacksmith" },
            { "Alex", "Josh" },
            { "Tunnel Entrance", "Backwoods" },
            { "Marnie's Ranch", "AnimalShop" },
            { "Cottage", "House" },
            { "Tower", "House" },
            { "Carpenter Shop", "ScienceHouse|Left" },
            { "Maru's Room", "ScienceHouse|Right" },
            { "Adventurer", "Adventure" },
            { "Willy's Fish Shop", "FishShop" },
            { "Museum", "ArchaeologyHouse" },
            { "The Mines", "Mine|Left" },
            { "Quarry Mine Entrance", "Mine|Right" },
            { "Quarry", "Mountain" },
            { "Shipwreck", "CaptainRoom" },
            { "Crystal Cave", "IslandWestCave1" },
            { "Boulder Cave", "IslandNorthCave1" },
            { "Skull Cavern Entrance", "SkullCave" },
            { "Oasis", "SandyHouse" },
            { "Bathhouse Entrance", "BathHouse_Entry" },
            { "Locker Room", "BathHouse_{0}Locker" },
            { "Public Bath", "BathHouse_Pool" },
            { "Pirate Cove", "IslandSouthEastCave" },
            { "Leo Hut", "IslandHut" },
            { "Field Office", "IslandFieldOffice" },
            { "Island Farmhouse", "IslandFarmHouse" },
            { "Qi Walnut Room", "QiNutRoom" },
            { "'s", "" },
            { " ", "" },
        };

        private static string[] _jojaMartLocations = new[] { "JojaMart", "AbandonedJojaMart", "MovieTheater" };
        private static string[] _trailerLocations = new[] { "Trailer", "Trailer_Big" };
        private static string[] _beachLocations = new[] { "Beach", "BeachNightMarket" };

        private List<string[]> _equivalentAreas = new()
        {
            _jojaMartLocations,
            _trailerLocations,
            _beachLocations,
        };
    }

    public enum FacingDirection
    {
        Up = 0,
        Right = 1,
        Down = 2,
        Left = 3,
    }
}