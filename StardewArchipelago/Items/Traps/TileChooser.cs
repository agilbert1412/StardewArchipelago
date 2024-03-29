﻿using Microsoft.Xna.Framework;
using StardewArchipelago.Extensions;
using StardewValley;
using xTile.Dimensions;

namespace StardewArchipelago.Items.Traps
{
    public class TileChooser
    {
        private const int MAX_RETRIES = 20;

        public Vector2? GetRandomTileInbounds(GameLocation area)
        {
            return GetRandomTileInbounds(area, Point.Zero, int.MaxValue);
        }

        public Vector2? GetRandomTileInbounds(GameLocation area, Point origin, int maxDistance)
        {
            var triesRemaining = MAX_RETRIES;
            var tile = area.getRandomTile();
            var tilePoint = Utility.Vector2ToPoint(tile);
            var tileLocation = new Location(tilePoint.X, tilePoint.Y);
            while (tilePoint.GetTotalDistance(origin) > maxDistance || area.isTileOccupied(tile) ||
                   area.isWaterTile(tilePoint.X, tilePoint.Y) || !area.isTileLocationTotallyClearAndPlaceable(tile) ||
                   !area.isTileLocationOpenIgnoreFrontLayers(tileLocation) || !CanPathFindToAnyWarp(area, tilePoint))
            {
                tile = area.getRandomTile();
                tilePoint = Utility.Vector2ToPoint(tile);
                tileLocation = new Location(tilePoint.X, tilePoint.Y);
                triesRemaining--;
                if (triesRemaining <= 0)
                {
                    return null;
                }
            }

            return tile;
        }

        public Vector2 GetRandomTileInboundsOffScreen(GameLocation area)
        {
            var numberRetries = MAX_RETRIES;
            var spawnPosition = GetRandomTileInbounds(area);
            if (spawnPosition == null)
            {
                return area.getRandomTile();
            }

            while (numberRetries > 0 && Utility.isOnScreen(Utility.Vector2ToPoint(spawnPosition.Value), 64, area))
            {
                numberRetries--;
                spawnPosition = GetRandomTileInbounds(area);
                if (spawnPosition == null)
                {
                    return area.getRandomTile();
                }
            }

            return spawnPosition.Value;
        }

        public bool CanPathFindToAnyWarp(GameLocation location, Point startPoint, int minimumDistance = 0, int maximumDistance = 500)
        {
            if (location.warps == null || location.warps.Count < 1)
            {
                return false;
            }

            if (location.isCollidingPosition(new Microsoft.Xna.Framework.Rectangle(startPoint.X * 64 + 1, startPoint.Y * 64 + 1, 62, 62),
                    Game1.viewport, true, 0, false, Game1.player, true))
            {
                return false;
            }

            foreach (var warp in location.warps)
            {
                var endPoint = new Point(warp.X, warp.Y);
                var endPointFunction = new PathFindController.isAtEnd(PathFindController.isAtEndPoint);
                var character = (Character)Game1.player;
                var path = PathFindController.findPath(startPoint, endPoint, endPointFunction, location, character, 250);
                if (path != null && path.Count >= minimumDistance && path.Count <= maximumDistance)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
