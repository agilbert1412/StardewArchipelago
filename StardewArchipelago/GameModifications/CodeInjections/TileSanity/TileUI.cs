using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Locations;
using StardewModdingAPI.Events;
using StardewValley;

namespace StardewArchipelago.GameModifications.CodeInjections.Tilesanity
{
    public static class TileUI
    {
        private static Texture2D _pixelTexture;
        private static int[,] _tileColors;
        private static GameLocation _currentLocation;
        private static LocationChecker _locationChecker;
        private static TileSanityManager _tileSanityManager;
        private static UIColor _showUI = UIColor.None;

        public static void Initialize(LocationChecker locationChecker, TileSanityManager tileSanityManager)
        {
            _locationChecker = locationChecker;
            _tileSanityManager = tileSanityManager;
        }

        private static Texture2D PixelTexture
        {
            get
            {
                if (_pixelTexture == null)
                {
                    _pixelTexture = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
                    _pixelTexture.SetData(new[] { Color.White });
                }
                return _pixelTexture;
            }
        }

        public static void RenderTiles(object sender, RenderedWorldEventArgs e)
        {
            if (_showUI == UIColor.None)
                return;

            var xMin = Math.Max(0, Game1.viewport.X / Game1.tileSize);
            var yMin = Math.Max(0, Game1.viewport.Y / Game1.tileSize);
            var location = Game1.player.currentLocation;
            var xMax = Math.Min(location.map.DisplayWidth / 64, (Game1.viewport.X + Game1.viewport.Width) / Game1.tileSize + 1);
            var yMax = Math.Min(location.map.DisplayHeight / 64, (Game1.viewport.Y + Game1.viewport.Height) / Game1.tileSize + 1);
            if (!ReferenceEquals(location, _currentLocation))
            {
                _tileColors = new int[location.map.DisplayWidth / 64, location.map.DisplayHeight / 64];
                _currentLocation = location;
            }

            const int period = 60 * 7;
            const float halfPeriod = period / 2f;
            const float thirdPeriod = period / 3f;
            var rainbow = new Color(
                Math.Abs(Game1.ticks % period - halfPeriod) / halfPeriod,
                Math.Abs((Game1.ticks + thirdPeriod) % period - halfPeriod) / halfPeriod,
                Math.Abs((Game1.ticks - thirdPeriod) % period - halfPeriod) / halfPeriod) * 0.5f;
            var black = _showUI == UIColor.Black ? Color.Black : Color.Black * 0.7f;

            for (var x = xMin; x < xMax; x++)
            {
                for (var y = yMin; y < yMax; y++)
                {
                    var tileColor = _tileColors[x, y];
                    if (tileColor == 0)
                    {
                        var tileName = _tileSanityManager.GetTileName(x, y, Game1.player);
                        if (!WalkSanityInjections.IsUnlocked(tileName))
                            _tileColors[x, y] = tileColor = 1;
                        else if (_locationChecker.IsLocationMissing(tileName))
                            _tileColors[x, y] = tileColor = 2;
                        else
                            _tileColors[x, y] = tileColor = -1;
                    }
                    var color = tileColor switch
                    {
                        -1 => Color.Transparent,
                        1 => black,
                        2 => rainbow,
                        _ => throw new Exception(),
                    };

                    // Draw only if the color is not transparent
                    if (color != Color.Transparent)
                    {
                        // Draw a transparent square
                        e.SpriteBatch.Draw(PixelTexture,
                            new Rectangle(x * Game1.tileSize - Game1.viewport.X, y * Game1.tileSize - Game1.viewport.Y, Game1.tileSize, Game1.tileSize),
                            color);
                    }
                }
            }
        }
        public static bool ProcessItem(ReceivedItem receivedItem)
        {
            if (!receivedItem.ItemName.Contains(TileSanityManager.TILESANITY_PREFIX))
                return false;
            if (_currentLocation == null)
                return true;
            var tiles = _tileSanityManager.GetTilesFromName(receivedItem.ItemName);
            foreach (var (map, x, y) in tiles)
            {
                var currentMap = _currentLocation.DisplayName;
                if (currentMap == $"{Game1.player.farmName} Farm")
                {
                    currentMap = _currentLocation.Name;
                }
                if (map == currentMap)
                {
                    _tileColors[x, y] = 2;
                }
            }
            return true;
        }
        public static void CheckLocation(string locationName)
        {
            if (_currentLocation == null)
                return;
            var tiles = _tileSanityManager.GetTilesFromName(locationName);
            var currentMap = _currentLocation.DisplayName;
            if (currentMap == $"{Game1.player.farmName} Farm")
            {
                currentMap = _currentLocation.Name;
            }
            foreach (var (map, x, y) in tiles)
            {
                if (map == currentMap)
                {
                    _tileColors[x, y] = -1;
                }
            }
        }

        public static bool ProcessCommand(string message)
        {
            switch (message)
            {
                case $"{ChatForwarder.COMMAND_PREFIX}tilesanity_ui":
                {
                    if (_showUI == UIColor.Normal)
                        _showUI = UIColor.None;
                    else
                        _showUI = UIColor.Normal;
                    return true;
                }
                case $"{ChatForwarder.COMMAND_PREFIX}tilesanity_ui_black":
                {
                    if (_showUI == UIColor.Black)
                        _showUI = UIColor.None;
                    else
                        _showUI = UIColor.Black;
                    return true;
                }
                default:
                    return false;
            }
        }

        public static void SwitchToDebug(List<Vector2> tiles)
        {
            for (var i = 0; i < _tileColors.GetLength(0); i++)
            {
                for (var j = 0; j < _tileColors.GetLength(1); j++)
                {
                    _tileColors[i, j] = -1;
                }
            }
            
            foreach (var (x, y) in tiles)
            {
                _tileColors[(int) x, (int) y] = 2;
            }
        }
    }
}
