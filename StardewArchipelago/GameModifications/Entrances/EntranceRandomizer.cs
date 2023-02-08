using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace StardewArchipelago.GameModifications.Entrances
{
    public class EntranceRandomizer
    {
        private const int _tolerance = 10;

        private IModHelper _helper;

        public List<LocationTransport> AllVanillaTransportations { get; set; }

        public EntranceRandomizer(IModHelper helper)
        {
            _helper = helper;
            LoadTransports();
        }

        public List<LocationTransport> RandomizeEntrances()
        {
            var random = new Random();
            var vanillaTransports = AllVanillaTransportations.Select(x => x.DeepCopy()).ToList();
            var transportCouples = new Dictionary<LocationTransport, LocationTransport>();

            while (vanillaTransports.Any())
            {
                var side1 = vanillaTransports[0];
                vanillaTransports.RemoveAt(0);
                var side2 = GetInverseEntrance(side1, vanillaTransports);
                vanillaTransports.Remove(side2);

                transportCouples.Add(side1, side2);
            }

            for (var i = 0; i < transportCouples.Count * 2; i++)
            {
                var keys = transportCouples.Keys.ToArray();
                var randomIndex1 = random.Next(0, transportCouples.Count);
                var firstEntrance = keys[randomIndex1];
                var firstEntranceInverted = transportCouples[firstEntrance];

                var randomIndex2 = random.Next(0, transportCouples.Count);

                if (randomIndex2 == randomIndex1)
                {
                    continue;
                }

                var pickedEntrance = keys[randomIndex2];
                var pickedEntranceInverted = transportCouples[pickedEntrance];

                var newEntrance1 = new LocationTransport(firstEntrance.Origin, pickedEntrance.Destination);
                var newEntrance1Inverted = new LocationTransport(pickedEntranceInverted.Origin, firstEntranceInverted.Destination);

                var newEntrance2 = new LocationTransport(pickedEntrance.Origin, firstEntrance.Destination);
                var newEntrance2Inverted = new LocationTransport(firstEntranceInverted.Origin, pickedEntranceInverted.Destination);

                transportCouples.Remove(firstEntrance);
                transportCouples.Remove(pickedEntrance);

                transportCouples.Add(newEntrance1, newEntrance1Inverted);
                transportCouples.Add(newEntrance2, newEntrance2Inverted);
            }

            return transportCouples.SelectMany(x => new[] { x.Key, x.Value }).ToList();
        }

        private LocationTransport GetInverseEntrance(LocationTransport entrance, List<LocationTransport> possibilities)
        {
            var entrancesFromOtherDirection = possibilities.Where(x =>
                x.Origin.LocationName == entrance.Destination.LocationName && x.Destination.LocationName == entrance.Origin.LocationName);
            var entranceFromOtherDirection = entrancesFromOtherDirection.OrderBy(x =>
                GetTotalTileDistance(x.Origin.Tile, entrance.Destination.Tile) +
                GetTotalTileDistance(x.Destination.Tile, entrance.Origin.Tile)).First();
            return entranceFromOtherDirection;
        }

        public void AddTransport(LocationTransport newTransport)
        {
            if (AllVanillaTransportations.Contains(newTransport))
            {
                return;
            }

            AllVanillaTransportations.Add(newTransport);
            DeleteDuplicates();
        }

        public void SaveTransports()
        {
            _helper.Data.WriteJsonFile("AllTransports.json", AllVanillaTransportations);
        }

        public void LoadTransports()
        {
            AllVanillaTransportations = _helper.Data.ReadJsonFile<List<LocationTransport>>("AllTransports.json");
            if (AllVanillaTransportations == null)
            {
                AllVanillaTransportations = new List<LocationTransport>();
            }

            DeleteDuplicates();
        }

        private void DeleteDuplicates()
        {
            for (var i = 0; i < AllVanillaTransportations.Count; i++)
            {
                for (var j = AllVanillaTransportations.Count - 1; j > i; j--)
                {
                    var first = AllVanillaTransportations[i];
                    var second = AllVanillaTransportations[j];

                    if (first.Equals(second))
                    {
                        AllVanillaTransportations.RemoveAt(j);
                        continue;
                    }

                    if (first.Origin.LocationName != second.Origin.LocationName ||
                        first.Destination.LocationName != second.Destination.LocationName)
                    {
                        continue;
                    }

                    var totalDifferenceEntrance1 = GetTotalTileDistance(first.Origin.Tile, second.Origin.Tile);
                    if (totalDifferenceEntrance1 > _tolerance)
                    {
                        continue;
                    }

                    var totalDifferenceEntrance2 = GetTotalTileDistance(first.Destination.Tile, second.Destination.Tile);
                    if (totalDifferenceEntrance2 > _tolerance)
                    {
                        continue;
                    }

                    AllVanillaTransportations.RemoveAt(j);
                }
            }
        }

        public int GetTotalTileDistance(Point tile1, Point tile2)
        {
            return Math.Abs(tile1.X - tile2.X) + Math.Abs(tile1.Y - tile2.Y);
        }
    }
}
