using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewArchipelago.GameModifications.Entrances;
using StardewArchipelago.Locations;
using StardewModdingAPI;
using StardewValley;
using xTile.Dimensions;

namespace StardewArchipelago.GameModifications.CodeInjections
{
    public class EntranceInjections
    {
        private static IMonitor _monitor;
        private static ArchipelagoClient _archipelago;
        private static EntranceRandomizer _entranceRandomizer;
        private static List<LocationTransport> _randomizedEntrances;

        public static void Initialize(IMonitor monitor, ArchipelagoClient archipelago, EntranceRandomizer entranceRandomizer)
        {
            _monitor = monitor;
            _archipelago = archipelago;
            _entranceRandomizer = entranceRandomizer;
            _randomizedEntrances = _entranceRandomizer.RandomizeEntrances();
        }

        public static bool PerformWarpFarmer_EntranceRandomization_Prefix(ref LocationRequest locationRequest, ref int tileX,
            ref int tileY, ref int facingDirectionAfterWarp)
        {
            try
            {
                /*var entrance1 = new Entrance(Game1.currentLocation.Name, new Point(Game1.player.getTileX(), Game1.player.getTileY()));
                var entrance2 = new Entrance(locationRequest.Name, new Point(tileX, tileY));
                var transport = new LocationTransport(entrance1, entrance2);
                _entranceRandomizer.AddTransport(transport);*/

                // _monitor.Log($"Warp to {locationRequest.Name} from {Game1.currentLocation.Name} [{tileX}, {tileY}] [{facingDirectionAfterWarp}]", LogLevel.Alert);

                // _randomizedEntrances = _entranceRandomizer.RandomizeEntrances();
                var desiredEntrance = FindBestMatch(Game1.currentLocation.Name, new Point(Game1.player.getTileX(), Game1.player.getTileY()), _randomizedEntrances);

                if (desiredEntrance == null)
                {
                    return true; // run original logic
                }

                locationRequest.Name = desiredEntrance.Destination.LocationName;
                locationRequest.Location = Game1.getLocationFromName(locationRequest.Name, locationRequest.IsStructure);
                tileX = desiredEntrance.Destination.Tile.X;
                tileY = desiredEntrance.Destination.Tile.Y;
                return true; // run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(PerformWarpFarmer_EntranceRandomization_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        private static LocationTransport FindBestMatch(string originName, Point originTile, List<LocationTransport> entranceCandidates)
        {
            LocationTransport bestMatch = null;
            var bestMatchDistance = 2;
            foreach (var entranceCandidate in entranceCandidates)
            {
                if (entranceCandidate.Origin.LocationName != originName)
                {
                    continue;
                }

                var distance = _entranceRandomizer.GetTotalTileDistance(originTile, entranceCandidate.Origin.Tile);
                if (distance < bestMatchDistance)
                {
                    bestMatchDistance = distance;
                    bestMatch = entranceCandidate;
                }
            }

            return bestMatch;
        }
    }
}
