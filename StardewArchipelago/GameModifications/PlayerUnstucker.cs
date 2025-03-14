﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewArchipelago.Extensions;
using StardewArchipelago.Items.Traps;
using StardewValley;
using StardewValley.Locations;

namespace StardewArchipelago.GameModifications
{
    public class PlayerUnstucker
    {
        private readonly TileChooser _tileChooser;

        public PlayerUnstucker(TileChooser tileChooser)
        {
            _tileChooser = tileChooser;
        }

        public bool Unstuck()
        {
            var player = Game1.player;
            var map = player.currentLocation;
            var tiles = new List<Point>();
            var islandSouth = map as IslandSouth;
            for (var x = 0; x < map.Map.Layers[0].LayerWidth; x++)
            {
                for (var y = 0; y < map.Map.Layers[0].LayerHeight; y++)
                {
                    var tilePosition = new Rectangle(x * 64 + 1, y * 64 + 1, 62, 62);
                    if (map.isCollidingPosition(tilePosition, Game1.viewport, true, 0, false, Game1.player))
                    {
                        continue;
                    }

                    tiles.Add(new Point(x, y));
                }
            }

            tiles = tiles.OrderBy(x => x.GetTotalDistance(player.TilePoint)).ToList();

            foreach (var tile in tiles)
            {
                if (_tileChooser.CanPathFindToAnyWarp(map, tile, 2))
                {
                    player.setTileLocation(new Vector2(tile.X, tile.Y));
                    return true;
                }
            }

            return false;
        }
    }
}
