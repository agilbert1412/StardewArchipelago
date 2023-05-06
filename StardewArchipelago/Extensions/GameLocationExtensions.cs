using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewArchipelago.GameModifications.EntranceRandomizer;
using StardewValley;

namespace StardewArchipelago.Extensions
{
    public static class GameLocationExtensions
    {
        public static IEnumerable<Point> GetAllWarpPointsTo(this GameLocation origin, string destinationName)
        {
            foreach (var warp in origin.warps)
            {
                if (warp.TargetName.Equals(destinationName))
                {
                    yield return new Point(warp.X, warp.Y);
                }

                if (warp.TargetName.Equals("BoatTunnel") && destinationName == "IslandSouth")
                {
                    yield return new Point(warp.X, warp.Y);
                }
            }
            foreach (var pair in origin.doors.Pairs)
            {
                if (pair.Value.Equals("BoatTunnel") && destinationName == "IslandSouth" ||
                    pair.Value.Equals(destinationName))
                {
                    yield return pair.Key;
                }
            }
        }

        public static Point GetClosestWarpPointTo(this GameLocation origin, string destinationName, Point currentLocation)
        {
            var allWarpPoints = origin.GetAllWarpPointsTo(destinationName).ToArray();
            if (!allWarpPoints.Any())
            {
                return new Point(currentLocation.X, currentLocation.Y - 1);
            }

            return allWarpPoints.OrderBy(x => x.GetTotalDistance(currentLocation)).First();
        }

        public static bool TryGetClosestWarpPointTo(this GameLocation origin, string destinationName, Point currentLocation, out Point closestWarpPoint)
        {
            var allWarpPoints = origin.GetAllWarpPointsTo(destinationName).ToArray();
            if (!allWarpPoints.Any())
            {
                closestWarpPoint = new Point(currentLocation.X, currentLocation.Y - 1);
                return false;
            }

            closestWarpPoint = allWarpPoints.OrderBy(x => x.GetTotalDistance(currentLocation)).First();
            return true;
        }
    }
}
