using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.Utilities.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.SlotData;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;
using StardewArchipelago.Constants.Modded;
using StardewArchipelago.Extensions;
using StardewArchipelago.Serialization;
using StardewValley;

namespace StardewArchipelago.GameModifications.EntranceRandomizer
{
    public class EntranceManager
    {
        public const string TRANSITIONAL_STRING = " to ";

        private readonly ILogger _logger;
        private readonly EquivalentWarps _equivalentAreas;
        private readonly ModEntranceManager _modEntranceManager;
        private readonly ArchipelagoStateDto _state;

        public Dictionary<string, string> ModifiedEntrances { get; private set; }
        private HashSet<string> _checkedEntrancesToday;
        private readonly Dictionary<string, WarpRequest> generatedWarps;

        public EntranceManager(ILogger logger, ArchipelagoClient archipelago, ArchipelagoStateDto state)
        {
            _logger = logger;
            _equivalentAreas = new EquivalentWarps(archipelago);
            _modEntranceManager = new ModEntranceManager();
            _state = state;
            generatedWarps = new Dictionary<string, WarpRequest>(StringComparer.OrdinalIgnoreCase);
        }

        public void ResetCheckedEntrancesToday(SlotData slotData)
        {
            _checkedEntrancesToday = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if ((slotData.EntranceRandomizationBehaviour & EntranceRandomizationBehaviour.Chaos) != 0)
            {
                ReshuffleEntrances(slotData);
            }
        }

        private void ReshuffleEntrances(SlotData slotData)
        {
            var seed = int.Parse(slotData.Seed) + (int)Game1.stats.DaysPlayed; // 998252633 on day 9
            var random = new Random(seed);
            var numShuffles = ModifiedEntrances.Count * ModifiedEntrances.Count;
            var newModifiedEntrances = ModifiedEntrances.ToDictionary(x => x.Key, x => x.Value);

            for (var i = 0; i < numShuffles; i++)
            {
                var keys = newModifiedEntrances.Keys.ToArray();
                var chosenIndex1 = random.Next(keys.Length);
                var chosenIndex2 = random.Next(keys.Length);
                var chosenEntrance1 = keys[chosenIndex1];
                var chosenEntrance2 = keys[chosenIndex2];
                SwapTwoEntrances(newModifiedEntrances, chosenEntrance1, chosenEntrance2);
            }

            ModifiedEntrances = new Dictionary<string, string>(newModifiedEntrances, StringComparer.OrdinalIgnoreCase);
        }

        public void SetEntranceRandomizerSettings(SlotData slotData)
        {
            ModifiedEntrances = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (slotData.EntranceRandomization == EntranceRandomization.Disabled)
            {
                return;
            }

            foreach (var (locationName, locationAlias) in _modEntranceManager.GetModLocationAliases(slotData))
            {
                _locationAliases[locationName] = locationAlias;
            }
            FixInitialEntranceDataGivenMod(slotData.Mods);
            foreach (var (originalEntrance, replacementEntrance) in slotData.ModifiedEntrances)
            {
                RegisterRandomizedEntrance(originalEntrance, replacementEntrance);
            }
        }

        private IEnumerable<string> GetModOutsideEntrances(SlotData slotData)
        {
            if (slotData.Mods.HasMod(ModNames.SVE))
            {
                yield return "Custom_ForestWest";
                yield return "Custom_BlueMoonVineyard";
            }
            if (slotData.Mods.HasMod(ModNames.BOARDING_HOUSE))
            {
                yield return "Custom_BoardingHouse_BackwoodsPlateau";
            }
        }

        private void FixInitialEntranceDataGivenMod(ModsManager modManager)
        {
            foreach (var (mod, aliases) in ModEntranceManager.AlteredMapNamesFromVanilla)
            {
                if (!modManager.HasMod(mod))
                {
                    return;
                }
                foreach (var (name, updatedName) in aliases)
                {
                    if (!_locationAliases.TryGetValue(name, out var _))
                    {
                        continue;
                    }
                    _locationAliases[name] = updatedName;
                }
            }
        }

        private static void SwapTwoEntrances(Dictionary<string, string> entrances, string entrance1, string entrance2)
        {
            // Trust me
            var destination1 = entrances[entrance1];
            var destination2 = entrances[entrance2];
            var reversed1 = ReverseKey(entrance1);
            var reversed2 = ReverseKey(entrance2);
            var reversedDestination1 = ReverseKey(destination1);
            var reversedDestination2 = ReverseKey(destination2);
            if (destination2 == reversed1 || destination1 == reversed2)
            {
                return;
            }
            entrances[entrance1] = destination2;
            entrances[reversedDestination1] = reversed2;
            entrances[entrance2] = destination1;
            entrances[reversedDestination2] = reversed1;
        }

        private void RegisterRandomizedEntrance(string originalEntrance, string replacementEntrance)
        {
            var aliasedOriginal = TurnAliased(originalEntrance);
            var aliasedReplacement = TurnAliased(replacementEntrance);
            _logger.LogMessage($"Aliased {originalEntrance} => {replacementEntrance} to {aliasedOriginal} => {aliasedReplacement}");
            RegisterRandomizedEntranceWithCoordinates(aliasedOriginal, aliasedReplacement);
        }

        private void RegisterRandomizedEntranceWithCoordinates(string originalEquivalentEntrance,
            string replacementEquivalentEntrance)
        {
            ModifiedEntrances.Add(originalEquivalentEntrance, replacementEquivalentEntrance);
        }

        public bool TryGetEntranceReplacement(string currentLocationName, string locationRequestName, Point targetPosition, out WarpRequest warpRequest)
        {
            warpRequest = null;
            if (ModEntry.Instance.State.EntranceRandomizerOverride)
            {
                return false;
            }

            var defaultCurrentLocationName = _equivalentAreas.GetDefaultEquivalentEntrance(currentLocationName);
            var defaultLocationRequestName = _equivalentAreas.GetDefaultEquivalentEntrance(locationRequestName);
            _logger.LogMessage($"replacing entrance {defaultCurrentLocationName} ({currentLocationName}) => {defaultLocationRequestName} ({locationRequestName}), pos {targetPosition}");
            targetPosition = targetPosition.CheckSpecialVolcanoEdgeCaseWarp(defaultLocationRequestName);
            _logger.LogMessage($"target: {targetPosition}");
            var key = GetKeys(defaultCurrentLocationName, defaultLocationRequestName, targetPosition);
            // return false;
            if (!TryGetModifiedWarpName(key, out var desiredWarpName))
            {
                _logger.LogDebug($"Tried to find warp from {currentLocationName} but found none.  Giving default warp.");
                return false;
            }

            var correctDesiredWarpName = _equivalentAreas.GetCorrectEquivalentEntrance(desiredWarpName);
            _logger.LogMessage($"To go to {correctDesiredWarpName} ({desiredWarpName})");

            if (_checkedEntrancesToday.Contains(correctDesiredWarpName))
            {
                if (generatedWarps.ContainsKey(correctDesiredWarpName))
                {
                    warpRequest = generatedWarps[correctDesiredWarpName];
                    return true;
                }
                _logger.LogDebug($"Desired warp {correctDesiredWarpName} was checked, but not generated.");
                return false;
            }

            return TryFindWarpToDestination(correctDesiredWarpName, out warpRequest);
        }

        private bool TryGetModifiedWarpName(IEnumerable<string> keys, out string desiredWarpName)
        {
            foreach (var key in keys.OrderByDescending(x => x.Length))
            {
                if (ModifiedEntrances.ContainsKey(key))
                {
                    desiredWarpName = ModifiedEntrances[key];
                    if (!_state.EntrancesTraversed.Contains(key))
                    {
                        _state.EntrancesTraversed.Add(key);
                    }
                    return true;
                }
            }

            desiredWarpName = "";
            return false;
        }

        private bool TryFindWarpToDestination(string desiredWarpKey, out WarpRequest warpRequest)
        {
            var (locationOriginName, locationDestinationName) = GetLocationNames(desiredWarpKey);
            _checkedEntrancesToday.Add(desiredWarpKey);

            _logger.LogMessage($"Getting closest warp point: {locationOriginName} => {locationDestinationName}");
            if (!locationOriginName.TryGetClosestWarpPointTo(ref locationDestinationName, _equivalentAreas, out var locationOrigin, out var warpPoint))
            {
                _logger.LogError($"Could not find closest warp for {desiredWarpKey}, returning a null warpRequest.");
                warpRequest = null;
                return false;
            }

            var warpPointTarget = locationOrigin.GetWarpPointTarget(warpPoint, locationDestinationName, _equivalentAreas);
            var locationDestination = Game1.getLocationFromName(locationDestinationName);
            var locationRequest = new LocationRequest(locationDestinationName, locationDestination.isStructure.Value, locationDestination);
            (locationRequest, warpPointTarget) = locationRequest.PerformLastLocationRequestChanges(locationOrigin, warpPoint, warpPointTarget);
            var warpAwayPoint = locationDestination.GetClosestWarpPointTo(locationOriginName, warpPointTarget);
            var facingDirection = warpPointTarget.GetFacingAwayFrom(warpAwayPoint);
            warpRequest = new WarpRequest(locationRequest, warpPointTarget.X, warpPointTarget.Y, facingDirection);
            generatedWarps[desiredWarpKey] = warpRequest;
            return true;
        }

        private static List<string> GetKeys(string currentLocationName, string locationRequestName,
            Point targetPosition)
        {
            var currentPosition = Game1.player.TilePoint;
            var currentPositions = new List<Point>();
            var targetPositions = new List<Point>();
            for (var x = -1; x <= 1; x++)
            {
                for (var y = -1; y <= 1; y++)
                {
                    currentPositions.Add(new Point(currentPosition.X + x, currentPosition.Y + y));
                    targetPositions.Add(new Point(targetPosition.X + x, targetPosition.Y + y));
                }
            }

            var keys = new List<string>();
            keys.Add(GetKey(currentLocationName, locationRequestName));
            keys.AddRange(targetPositions.Select(targetPositionWithOffset => GetKey(currentLocationName, locationRequestName, targetPositionWithOffset)));
            keys.AddRange(currentPositions.Select(currentPositionWithOffset => GetKey(currentLocationName, currentPositionWithOffset, locationRequestName)));
            foreach (var currentPositionWithOffset in currentPositions)
            {
                keys.AddRange(targetPositions.Select(targetPositionWithOffset => GetKey(currentLocationName, currentPositionWithOffset, locationRequestName, targetPositionWithOffset)));
            }

            return keys;
        }

        private static string GetKey(string currentLocationName, string locationRequestName)
        {
            var key = $"{currentLocationName}{TRANSITIONAL_STRING}{locationRequestName}";
            return key;
        }

        private static string GetKey(string currentLocationName, string locationRequestName, Point targetPosition)
        {
            var key = $"{currentLocationName}{TRANSITIONAL_STRING}{locationRequestName}|{targetPosition.X}|{targetPosition.Y}";
            return key;
        }

        private static string GetKey(string currentLocationName, Point currentPosition, string locationRequestName)
        {
            var key = $"{currentLocationName}|{currentPosition.X}|{currentPosition.Y}{TRANSITIONAL_STRING}{locationRequestName}";
            return key;
        }

        private static string GetKey(string currentLocationName, Point currentPosition, string locationRequestName, Point targetPosition)
        {
            var key = $"{currentLocationName}|{currentPosition.X}|{currentPosition.Y}{TRANSITIONAL_STRING}{locationRequestName}|{targetPosition.X}|{targetPosition.Y}";
            return key;
        }

        private static string ReverseKey(string key)
        {
            var parts = key.Split(TRANSITIONAL_STRING);
            return $"{parts[1]}{TRANSITIONAL_STRING}{parts[0]}";
        }

        private static (string, string) GetLocationNames(string key)
        {
            var split = key.Split(TRANSITIONAL_STRING);
            return (split[0], split[1]);
        }

        private string TurnAliased(string key)
        {
            if (key.Contains(TRANSITIONAL_STRING))
            {
                var parts = key.Split(TRANSITIONAL_STRING);
                var aliased1 = TurnAliased(parts[0]);
                var aliased2 = TurnAliased(parts[1]);
                var newEntrance = $"{aliased1}{TRANSITIONAL_STRING}{aliased2}";
                var newEntranceAliased = TurnAliased(newEntrance, _entranceAliases, false);
                return newEntranceAliased;
            }

            var modifiedString = TurnAliased(TurnAliased(key, _locationAliases, false), _locationsSingleWordAliases, true);
            return modifiedString;
        }

        private string TurnAliased(string key, Dictionary<string, string> aliases, bool singleWord)
        {
            var modifiedString = key;
            foreach (var oldString in aliases.Keys.OrderByDescending(x => x.Length))
            {
                var newString = aliases[oldString];
                var customizedNewString = newString;

                if (singleWord)
                {
                    modifiedString = modifiedString.Replace(oldString, customizedNewString);
                }
                else
                {
                    if (modifiedString.Contains(oldString) && !modifiedString.Equals(oldString))
                    {
                        // throw new ArgumentException($"This string is a partial replacement! {oldString}");
                    }

                    if (modifiedString.Equals(oldString))
                    {
                        modifiedString = customizedNewString;
                    }
                }
            }

            return modifiedString;
        }

        private static readonly Dictionary<string, string> _entranceAliases = new()
        {
            { "SebastianRoom to ScienceHouse|6|24", "SebastianRoom to ScienceHouse" }, // LockedDoorWarp 6 24 ScienceHouse 900 2000S–
            { "ScienceHouse|6|24 to SebastianRoom", "ScienceHouse to SebastianRoom" }, // LockedDoorWarp 6 24 ScienceHouse 900 2000S–
        };

        private readonly Dictionary<string, string> _locationAliases = new()
        {
            { "Mayor's Manor", "ManorHouse" },
            { "Pierre's General Store", "SeedShop" },
            { "Clint's Blacksmith", "Blacksmith" },
            { "Alex's House", "JoshHouse" },
            { "Tunnel Entrance", "Backwoods" },
            { "Marnie's Ranch", "AnimalShop" },
            { "Leah's Cottage", "LeahHouse" },
            { "Wizard Tower", "WizardHouse" },
            { "Sewers", "Sewer" },
            { "Bus Tunnel", "Tunnel" },
            { "Carpenter Shop", "ScienceHouse|6|24" }, // LockedDoorWarp 6 24 ScienceHouse 900 2000S–
            { "Maru's Room", "ScienceHouse|3|8" }, // LockedDoorWarp 3 8 ScienceHouse 900 2000 Maru 500N
            { "Adventurer's Guild", "AdventureGuild" },
            { "Willy's Fish Shop", "FishShop" },
            { "Museum", "ArchaeologyHouse" },
            { "Wizard Basement", "WizardHouseBasement" },
            { "The Mines", "Mine|18|13" }, // 54 4 Mine 18 13
            { "Quarry Mine Entrance", "Mine|67|17" }, // 103 15 Mine 67 17
            { "Outside Adventure Guild", "Mountain"},
            { "Quarry", "Mountain" },
            { "Shipwreck", "CaptainRoom" },
            { "Gourmand Cave", "IslandFarmcave" },
            { "Crystal Cave", "IslandWestCave1" },
            { "Boulder Cave", "IslandNorthCave1" },
            { "Skull Cavern Entrance", "SkullCave" },
            { "Oasis", "SandyHouse" },
            { "Casino", "Club" },
            { "Bathhouse Entrance", "BathHouse_Entry" },
            { "Men's Locker Room", "BathHouse_MensLocker" },
            { "Women's Locker Room", "BathHouse_WomensLocker" },
            { "Public Bath", "BathHouse_Pool" },
            { "Pirate Cove", "IslandSouthEastCave" },
            { "Leo Hut", "IslandHut" },
            { "Dig Site", "Island North" },
            { "Field Office", "IslandFieldOffice" },
            { "Island Farmhouse", "IslandFarmHouse" },
            { "Volcano Entrance", "VolcanoDungeon0|31|53" },
            { "Volcano River", "VolcanoDungeon0|6|49" },
            { "Secret Beach", "IslandNorth|12|31" },
            { "Secret Woods", "Woods" },
            { "Professor Snail Cave", "IslandNorthCave1" },
            { "Qi Walnut Room", "QiNutRoom" },
            { "Mutant Bug Lair", "BugLand" },
            { "Purple Shorts Maze", "LewisBasement"},
            { "Mountain Shortcut at Fence", "Mountain|57|40" },
            { "Town Shortcut at Fence", "Town|90|0" },
            { "Mountain Shortcut near Quarry Bridge", "Mountain|85|40" },
            { "Town Shortcut through Cave", "Town|98|0" },
            { "Tide Pools Shortcut", "Beach|67|0" },
            { "Town Shortcut below Museum", "Town|94|109" },
            { "Beach Shortcut", "Beach" },
            { "Forest Shortcut", "Forest" },
            { "Minecart Town", "Town|105|80" },
            { "Minecart Mines", "Mine|13|9" },
            { "Minecart Bus Stop", "BusStop|14|4"},
            { "Minecart Quarry", "Mountain|124|12"},
            { "Island South Ridge", "IslandSouth|27|1" },
            { "Parrot Express Volcano", "IslandNorth|60|17" },
            { "Parrot Express Docks", "IslandSouth|6|32" },
            { "Parrot Express Dig Site", "IslandNorth|46|48" },
            { "Parrot Express Jungle", "IslandEast|28|29" },
            { "Parrot Express Farm", "IslandWest|74|10" },
            { "Use Water Obelisk", "Farm to Beach" },
            { "Use Earth Obelisk", "Farm to Mountain" },
            { "Use Desert Obelisk", "Farm to Desert" },
            { "Use Island Obelisk", "Farm to IslandSouth" },
            { "Use Farm Obelisk", "IslandWest to Farm" },
        };

        private readonly Dictionary<string, string> _locationsSingleWordAliases = new()
        {
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
