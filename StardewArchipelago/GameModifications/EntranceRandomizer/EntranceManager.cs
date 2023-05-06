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
        public EquivalentWarps Equivalencies { get; }
        private Dictionary<string, OneWayEntrance> _allEntrances;

        public EntranceManager(IMonitor monitor)
        {
            _monitor = monitor;
            Equivalencies = new EquivalentWarps();
            RegisterAllEntrances();
            Equivalencies.SetAllEntrances(_allEntrances);
        }

        public void RegisterAllEntrances()
        {
            _allEntrances = new Dictionary<string, OneWayEntrance>();
            CreateTwoWayEntrance("FarmHouse", "Farm", FacingDirection.Up, FacingDirection.Down);
            CreateTwoWayEntrance("FarmHouse", "Cellar", FacingDirection.Up, FacingDirection.Down);

            CreateTwoWayEntrance("Farm", "Greenhouse", FacingDirection.Down, FacingDirection.Up);
            CreateTwoWayEntrance("Farm", "FarmCave", FacingDirection.Down, FacingDirection.Up);
            CreateTwoWayEntrance("Farm", "Backwoods", FacingDirection.Down, FacingDirection.Up);
            CreateTwoWayEntrance("Farm", "BusStop", FacingDirection.Left, FacingDirection.Right);
            CreateTwoWayEntrance("Farm", "Forest", FacingDirection.Up, FacingDirection.Down);

            CreateTwoWayEntrance("BusStop", "Backwoods", FacingDirection.Right, FacingDirection.Left);
            CreateTwoWayEntrance("BusStop", "Town", FacingDirection.Left, FacingDirection.Right);
            CreateTwoWayEntrance("Backwoods", "Tunnel", FacingDirection.Right, FacingDirection.Left);


            CreateTwoWayEntrance("Backwoods", "Mountain", FacingDirection.Right, FacingDirection.Left);

            CreateTwoWayEntrance("Forest", "Woods", FacingDirection.Right, FacingDirection.Left);
            CreateTwoWayEntrance("Forest", "WizardHouse", FacingDirection.Down, FacingDirection.Up);
            CreateTwoWayEntrance("Forest", "AnimalShop", FacingDirection.Down, FacingDirection.Up);
            CreateTwoWayEntrance("Forest", "LeahHouse", FacingDirection.Down, FacingDirection.Up);
            CreateTwoWayEntrance("Forest", "Town", FacingDirection.Left, FacingDirection.Right);

            CreateTwoWayEntrance("Town", "CommunityCenter", FacingDirection.Down, FacingDirection.Up);
            CreateTwoWayEntrance("Town", "Hospital", FacingDirection.Down, FacingDirection.Up);
            CreateTwoWayEntrance("Town", "SeedShop", FacingDirection.Down, FacingDirection.Up);
            CreateTwoWayEntrance("Town", "Saloon", FacingDirection.Down, FacingDirection.Up);
            CreateTwoWayEntrance("Town", "JoshHouse", FacingDirection.Down, FacingDirection.Up);
            CreateTwoWayEntrance("Town", "Trailer", FacingDirection.Down, FacingDirection.Up);
            CreateTwoWayEntrance("Town", "Trailer_Big", FacingDirection.Down, FacingDirection.Up);
            CreateTwoWayEntrance("Town", "HaleyHouse", FacingDirection.Down, FacingDirection.Up);
            CreateTwoWayEntrance("Town", "SamHouse", FacingDirection.Down, FacingDirection.Up);
            CreateTwoWayEntrance("Town", "Sewer", FacingDirection.Down, FacingDirection.Down);
            CreateTwoWayEntrance("Town", "ManorHouse", FacingDirection.Down, FacingDirection.Up);
            CreateTwoWayEntrance("Town", "Beach", FacingDirection.Up, FacingDirection.Down);
            CreateTwoWayEntrance("Town", "ArchaeologyHouse", FacingDirection.Down, FacingDirection.Up);
            CreateTwoWayEntrance("Town", "Blacksmith", FacingDirection.Down, FacingDirection.Up);
            CreateTwoWayEntrance("Town", "Mountain", FacingDirection.Down, FacingDirection.Up);
            CreateTwoWayEntrance("Town", "JojaMart", FacingDirection.Down, FacingDirection.Up);
            CreateTwoWayEntrance("Town", "AbandonedJojaMart", FacingDirection.Down, FacingDirection.Up);
            CreateTwoWayEntrance("Town", "MovieTheater", FacingDirection.Down, FacingDirection.Up);
            CreateTwoWayEntrance("Mountain", "Railroad", FacingDirection.Down, FacingDirection.Up);
            CreateTwoWayEntrance("Mountain", "Tent", FacingDirection.Down, FacingDirection.Up);
            CreateTwoWayEntrances("Mountain", "ScienceHouse", FacingDirection.Down, FacingDirection.Up).ToArray();
            CreateTwoWayEntrance("Mountain", "AdventureGuild", FacingDirection.Down, FacingDirection.Up);
            CreateTwoWayEntrance("Mountain", "Mine", FacingDirection.Down, FacingDirection.Up);
            CreateTwoWayEntrance("Beach", "FishShop", FacingDirection.Down, FacingDirection.Up);
            CreateTwoWayEntrance("Railroad", "BathHouse_Entry", FacingDirection.Down, FacingDirection.Up);
            CreateTwoWayEntrance("Railroad", "WitchWarpCave", FacingDirection.Down, FacingDirection.Up);
            CreateTwoWayEntrance("WitchWarpCave", "WitchSwamp", FacingDirection.Down, FacingDirection.Up);
            CreateTwoWayEntrance("WitchSwamp", "WitchHut", FacingDirection.Down, FacingDirection.Up);
            CreateTwoWayEntrance("ScienceHouse", "SebastianRoom", FacingDirection.Up, FacingDirection.Down);
            CreateTwoWayEntrance("Hospital", "HarveyRoom", FacingDirection.Down, FacingDirection.Up);
            CreateTwoWayEntrance("WizardHouse", "WizardHouseBasement",
                FacingDirection.Up, FacingDirection.Down);

            // Quarry Mine
            // public static readonly (OneWayEntrance, OneWayEntrance) MountainToMine = AddEntrance("Mountain", "Mine", 103, 17, 67, 17, 2, 0);
        }

        public void ReplaceEntrances(SlotData slotData)
        {
            var replacer = new EntranceReplacer(_monitor, slotData, this);
            replacer.ReplaceEntrances();
        }

        public bool TryGetEntrance(string key, out OneWayEntrance entrance)
        {
            var aliasedKey = TurnAliased(key);

            if (_allEntrances.TryGetValue(aliasedKey, out entrance))
            {
                return true;
            }

            if (_allEntrances.TryGetValue(aliasedKey.ToLower(), out entrance))
            {
                return true;
            }

            return _allEntrances.TryGetValue(aliasedKey.ToUpper(), out entrance);
        }

        public bool TryGetEntrance(string location1, string location2, out OneWayEntrance entrance)
        {
            var aliasedlocation1 = TurnAliased(location1);
            var aliasedlocation2 = TurnAliased(location2);
            var key = $"{aliasedlocation1}{TRANSITIONAL_STRING}{aliasedlocation2}";
            return TryGetEntrance(key, out entrance);
        }

        private (OneWayEntrance, OneWayEntrance) CreateTwoWayEntrance(string location1Name, string location2Name,
            FacingDirection direction1, FacingDirection direction2)
        {
            return CreateTwoWayEntrances(location1Name, location2Name, direction1, direction2).FirstOrDefault();
        }

        private IEnumerable<(OneWayEntrance, OneWayEntrance)> CreateTwoWayEntrances(string location1Name,
            string location2Name, FacingDirection direction1, FacingDirection direction2)
        {
            var location1 = Game1.getLocationFromName(location1Name);
            var location2 = Game1.getLocationFromName(location2Name);

            var (warpPointsFirstDirection, warpPointsSecondDirection) = GetAllWarpPointsBetween(location1, location2);

            if (warpPointsFirstDirection.Count != warpPointsSecondDirection.Count ||
                warpPointsFirstDirection.Count == 0)
            {
                Debugger.Break();
                yield break;
                /*throw new ArgumentException(
                    $"Could not create TwoWayEntrances between {location1Name} and {location2Name}");*/
            }

            for (var i = 0; i < warpPointsFirstDirection.Count; i++)
            {
                var warpPoint1 = warpPointsFirstDirection[i];
                var warpPoint2 = warpPointsSecondDirection[i];
                var entrance = AddEntrance(location1Name, location2Name,
                    warpPoint2.Item2, warpPoint1.Item2,
                    direction1, direction2);
                yield return entrance;
            }
        }

        private static (List<(Point, Point)>, List<(Point, Point)>) GetAllWarpPointsBetween(GameLocation location1, GameLocation location2)
        {
            var warpPointsFirstDirection = GetWarpPointsTo(location1, location2);
            var warpPointsSecondDirection = GetWarpPointsTo(location2, location1);

            var firstDirectionTargets = warpPointsFirstDirection.Select(x => x.Item2).RemoveDuplicates(true).ToArray();
            var secondDirectionTargets = warpPointsSecondDirection.Select(x => x.Item2).RemoveDuplicates(true).ToArray();

            if (firstDirectionTargets.Length > 1 && secondDirectionTargets.Length > 1)
            {
                Debugger.Break();
                return (warpPointsFirstDirection, warpPointsSecondDirection);
            }

            if (!firstDirectionTargets.Any() || !secondDirectionTargets.Any())
            {
                return (new List<(Point, Point)>(),
                    new List<(Point, Point)>());
                throw new ArgumentException($"Could not detect the warps between {location1.Name} and {location2.Name}");
            }

            var closestWarpFirstDirection = GetClosestWarpTo(warpPointsFirstDirection, secondDirectionTargets[0]);
            var closestWarpSecondDirection = GetClosestWarpTo(warpPointsSecondDirection, firstDirectionTargets[0]);

            return (new List<(Point, Point)> { closestWarpFirstDirection },
                new List<(Point, Point)> { closestWarpSecondDirection });
        }

        private static (Point, Point) GetClosestWarpTo(List<(Point, Point)> warpPoints, Point reverseTarget)
        {
            var shortestDistance = int.MaxValue;
            (Point, Point) closestWarp = warpPoints.First();
            foreach (var warpPoint in warpPoints)
            {
                var originPoint = warpPoint.Item1;
                var distance = originPoint.GetTotalDistance(reverseTarget);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    closestWarp = warpPoint;
                }
            }

            return closestWarp;
        }

        private static List<(Point, Point)> GetWarpPointsTo(GameLocation location1, GameLocation location2)
        {
            var warps = new List<(Point, Point)>();
            foreach (var warp in location1.warps)
            {
                if (warp.TargetName.Equals(location2.Name))
                {
                    warps.Add((new Point(warp.X, warp.Y), new Point(warp.TargetX, warp.TargetY)));
                }
            }

            foreach (var pair in location1.doors.Pairs)
            {
                if (pair.Value.Equals(location2.Name))
                {
                    var target = location1.getWarpPointTarget(pair.Key);
                    warps.Add((pair.Key, target));
                }
            }

            return warps;
        }

        private (OneWayEntrance, OneWayEntrance) AddEntrance(string location1Name, string location2Name,
            int location1X, int location1Y, int location2X, int location2Y, FacingDirection facingDirection1,
            FacingDirection facingDirection2)
        {
            return AddEntrance(location1Name, location2Name, new Point(location1X, location1Y),
                new Point(location2X, location2Y), facingDirection1, facingDirection2);
        }

        private (OneWayEntrance, OneWayEntrance) AddEntrance(string location1Name, string location2Name,
            Point location1Position, Point location2Position, FacingDirection facingDirection1,
            FacingDirection facingDirection2)
        {
            var entrance1 = new OneWayEntrance(Equivalencies, location1Name, location2Name, location1Position, location2Position,
                facingDirection2);
            var entrance2 = new OneWayEntrance(Equivalencies, location2Name, location1Name, location2Position, location1Position,
                facingDirection1);
            var key1 = $"{location1Name}{TRANSITIONAL_STRING}{location2Name}";
            var key2 = $"{location2Name}{TRANSITIONAL_STRING}{location1Name}";

            AddDefaultAliases(key1, entrance1);
            AddDefaultAliases(key2, entrance2);

            return (entrance1, entrance2);
        }

        private void AddDefaultAliases(string entranceName, OneWayEntrance entrance)
        {
            _allEntrances.Add(entranceName, entrance);
            _allEntrances.Add(entranceName.ToLower(), entrance);
            _allEntrances.Add(entranceName.ToUpper(), entrance);
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