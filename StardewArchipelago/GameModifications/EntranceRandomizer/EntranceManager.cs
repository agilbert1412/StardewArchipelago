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
            _modifiedEntrances = new Dictionary<string, string>();
            if (slotData.EntranceRandomization == EntranceRandomization.Disabled)
            {
                return;
            }

            foreach (var (originalEntrance, replacementEntrance) in slotData.ModifiedEntrances)
            {
                _modifiedEntrances.Add(TurnAliased(originalEntrance), TurnAliased(replacementEntrance));
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
            var locationOrigin = Game1.getLocationFromName(locationOriginName);
            _checkedEntrancesToday.Add(desiredWarpKey);

            if (!locationOrigin.TryGetClosestWarpPointTo(locationDestinationName, currentTile, out var warpPoint))
            {
                warpRequest = null;
                return false;
            }

            var locationDestination = Game1.getLocationFromName(locationDestinationName);
            var warpPointTarget = locationOrigin.getWarpPointTarget(warpPoint);
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
                modifiedString = modifiedString.Replace(oldString, newString);
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
            { "Carpenter Shop", "ScienceHouse" },
            { "Adventurer", "Adventure" },
            { "Willy's Fish Shop", "FishShop" },
            { "Museum", "ArchaeologyHouse" },
            { "The Mines", "Mine" },
            { "'s", "" },
            { " ", "" },
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