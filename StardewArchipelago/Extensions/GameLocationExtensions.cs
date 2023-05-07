using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewArchipelago.GameModifications.EntranceRandomizer;
using StardewValley;
using StardewValley.Locations;

namespace StardewArchipelago.Extensions
{
    public static class GameLocationExtensions
    {
        private static readonly Dictionary<WarpRequest, WarpRequest> ExtraWarps = new()
        {
            {new WarpRequest(Game1.getLocationRequest("Town"), 35, 97, FacingDirection.Down), new WarpRequest(Game1.getLocationRequest("Sewer"), 16, 11, FacingDirection.Down)},
            {new WarpRequest(Game1.getLocationRequest("IslandWest"), 20, 23, FacingDirection.Down), new WarpRequest(Game1.getLocationRequest("QiNutRoom"), 7, 8, FacingDirection.Up)},
        };

        public static List<Point> GetAllWarpPointsTo(this GameLocation origin, string destinationName)
        {
            var warpPoints = new List<Point>();
            warpPoints.AddRange(GetAllTouchWarpsTo(origin, destinationName).Select(warp => new Point(warp.X, warp.Y)));
            warpPoints.AddRange(GetDoorWarpPoints(origin, destinationName));
            warpPoints.AddRange(GetSpecialTriggerWarps(origin, destinationName).Keys);

            return warpPoints;
        }

        public static Point GetWarpPointTarget(this GameLocation origin, Point warpPointLocation, string destinationName)
        {
            foreach (var warp in GetAllTouchWarpsTo(origin, destinationName))
            {
                if (warp.X == warpPointLocation.X && warp.Y == warpPointLocation.Y)
                {
                    return new Point(warp.TargetX, warp.TargetY);
                }
            }

            if (TryGetDoorWarpPointTarget(origin, warpPointLocation, destinationName, out var warpPointTarget))
            {
                return warpPointTarget;
            }

            foreach (var (warp, warpTarget) in GetSpecialTriggerWarps(origin, destinationName))
            {
                if (warp.X == warpPointLocation.X && warp.Y == warpPointLocation.Y)
                {
                    return new Point(warpTarget.X, warpTarget.Y);
                }
            }

            throw new Exception(
                $"Could not find Warp Point Target for '{origin.Name}' to '{destinationName}' at [{warpPointLocation.X}, {warpPointLocation.Y}]");
        }

        private static List<Warp> GetAllTouchWarpsTo(GameLocation origin, string destinationName)
        {
            var warps = new List<Warp>();
            foreach (var warp in origin.warps)
            {
                if (warp.TargetName.Equals(destinationName, StringComparison.OrdinalIgnoreCase))
                {
                    warps.Add(warp);
                }
            }

            warps.AddRange(GetSpecialTouchWarps(origin));
            return warps;
        }

        private static IEnumerable<Warp> GetSpecialTouchWarps(GameLocation origin)
        {
            if (origin is FarmHouse farmhouse && farmhouse.cellarWarps != null)
            {
                foreach (var cellarWarp in farmhouse.cellarWarps)
                {
                    yield return cellarWarp;
                }
            }
        }

        private static IEnumerable<Point> GetDoorWarpPoints(GameLocation origin, string destinationName)
        {
            foreach (var pair in origin.doors.Pairs)
            {
                if (pair.Value.Equals(destinationName, StringComparison.OrdinalIgnoreCase))
                {
                    yield return pair.Key;
                }
            }
        }

        private static bool TryGetDoorWarpPointTarget(GameLocation origin, Point warpPointLocation, string targetDestinationName, out Point warpPointTarget)
        {
            foreach (var (warpPoint, destinationName) in origin.doors.Pairs)
            {
                if (!warpPoint.Equals(warpPointLocation) || !destinationName.Equals(targetDestinationName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var str = origin.doesTileHaveProperty(warpPointLocation.X, warpPointLocation.Y, "Action", "Buildings");
                if (str == null || !str.Contains("Warp"))
                {
                    continue;
                }

                var strArray = str.Split(' ');
                if (strArray[0].Equals("WarpCommunityCenter"))
                {
                    warpPointTarget = new Point(32, 23);
                    return true;
                }

                if (strArray[0].Equals("Warp_Sunroom_Door"))
                {
                    warpPointTarget = new Point(5, 13);
                    return true;
                }

                if (strArray.Length > 3 && strArray[3].Equals("BoatTunnel"))
                {
                    warpPointTarget = new Point(17, 43);
                    return true;
                }

                if (strArray[3].Equals("Trailer") && Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
                {
                    warpPointTarget = new Point(13, 24);
                    return true;
                }

                warpPointTarget = new Point(Convert.ToInt32(strArray[1]), Convert.ToInt32(strArray[2]));
                return true;
            }

            warpPointTarget = Point.Zero;
            return false;
        }

        private static Dictionary<Point, Point> GetSpecialTriggerWarps(GameLocation origin, string destinationName)
        {
            var specialTriggerWarps = new Dictionary<Point, Point>();
            foreach (var (warp1, warp2) in ExtraWarps.Union(ExtraWarps.ToDictionary(x => x.Value, x => x.Key)))
            {
                if (!warp1.LocationRequest.Name.Equals(origin.Name, StringComparison.OrdinalIgnoreCase) ||
                    !warp2.LocationRequest.Name.Equals(destinationName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                specialTriggerWarps.Add(new Point(warp1.TileX, warp1.TileY), new Point(warp2.TileX, warp2.TileY));
            }

            return specialTriggerWarps;
        }

        public static Point GetClosestWarpPointTo(this GameLocation origin, string destinationName, Point currentLocation)
        {
            var allWarpPoints = origin.GetAllWarpPointsTo(destinationName);
            if (!allWarpPoints.Any())
            {
                return new Point(currentLocation.X, currentLocation.Y - 1);
            }

            return allWarpPoints.OrderBy(x => x.GetTotalDistance(currentLocation)).First();
        }

        public static bool TryGetClosestWarpPointTo(this string originName, ref string destinationName, Point currentLocation, out GameLocation originLocation, out Point closestWarpPoint)
        {
            var originParts = originName.Split("|");
            var originTrueLocationName = originParts[0];
            originLocation = Game1.getLocationFromName(originTrueLocationName);
            var destinationParts = destinationName.Split("|");
            destinationName = destinationParts[0];
            var allWarpPoints = originLocation.GetAllWarpPointsTo(destinationName);
            if (!allWarpPoints.Any())
            {
                closestWarpPoint = new Point(currentLocation.X, currentLocation.Y - 1);
                return false;
            }

            if (originParts.Length >= 2 || destinationParts.Length >= 2)
            {
                var direction = originParts.Length >= 2 ? originParts[1] : destinationParts[1];
                Point referencePoint = Point.Zero;
                switch (direction)
                {
                    case "Left":
                        referencePoint = new Point(-9999, 0);
                        break;
                    case "Right":
                        referencePoint = new Point(9999, 0);
                        break;
                    case "Up":
                        referencePoint = new Point(0, -9999);
                        break;
                    case "Down":
                        referencePoint = new Point(0, 9999);
                        break;
                }

                closestWarpPoint = allWarpPoints.OrderBy(x => x.GetTotalDistance(referencePoint)).First();
                return true;
            }

            closestWarpPoint = allWarpPoints.OrderBy(x => x.GetTotalDistance(currentLocation)).First();
            return true;
        }
    }
}
