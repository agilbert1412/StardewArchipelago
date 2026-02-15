using System;
using System.Collections.Generic;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Locations;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace StardewArchipelago.GameModifications.CodeInjections.Tilesanity
{
    internal static class TileColor
    {
        public const int EMPTY = -1;
        public const int UNINITIALIZED = 0;
        public const int LOCKED = 1;
        public const int LOCATION = 2;
    }
    
    public static class TileUI
    {
        private static Texture2D _pixelTexture;
        private static int[,] _tileColors;
        private static GameLocation _currentLocation;
        private static StardewLocationChecker _locationChecker;
        private static TileSanityManager _tileSanityManager;
        private static UIColor _showUI = UIColor.None;

        public static void Initialize(StardewLocationChecker locationChecker, TileSanityManager tileSanityManager)
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
            {
                return;
            }

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
                    if (tileColor == TileColor.UNINITIALIZED)
                    {
                        var tileName = TileSanityManager.GetTileName(x, y, Game1.player);
                        if (!WalkSanityInjections.IsUnlocked(tileName))
                        {
                            _tileColors[x, y] = tileColor = TileColor.LOCKED;
                        }
                        else if (_locationChecker.LocationExists(tileName) && _tileSanityManager.HasLocationLeft(tileName))
                        {
                            _tileColors[x, y] = tileColor = TileColor.LOCATION;
                        }
                        else
                        {
                            _tileColors[x, y] = tileColor = TileColor.EMPTY;
                        }
                    }
                    var color = tileColor switch
                    {
                        TileColor.EMPTY => Color.Transparent,
                        TileColor.LOCKED => black,
                        TileColor.LOCATION => rainbow,
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
            var itemName = receivedItem.ItemName;
            if (!itemName.Contains(TileSanityManager.TILESANITY_PREFIX))
            {
                return false;
            }
            if (_currentLocation == null)
            {
                return true;
            }
            var currentMap = TileSanityManager.GetMapName(_currentLocation);

            var tiles = _tileSanityManager.GetTilesFromName(itemName);
            foreach (var (map, x, y) in tiles)
            {
                // x/y here are unchecked
                if (map == currentMap && _tileColors.GetLength(0) > x && _tileColors.GetLength(1) > y)
                {
                    if (_tileSanityManager.HasLocationLeft(itemName))
                    {
                        _tileColors[x, y] = TileColor.LOCATION;
                    }
                    else
                    {
                        _tileColors[x, y] = TileColor.EMPTY;
                    }
                }
            }
            return true;
        }
        public static void CheckLocation(string locationName, IMonitor monitor)
        {
            if (_currentLocation == null)
            {
                return;
            }
            var currentMap = TileSanityManager.GetMapName(_currentLocation);
            
            var tiles = _tileSanityManager.GetTilesFromName(locationName);
            foreach (var (map, x, y) in tiles)
            {
                if (map == currentMap)
                {
                    if (x < 0 || y < 0 || x >= _tileColors.GetLength(0) || y >= _tileColors.GetLength(1))
                    {
                        monitor.Log($"Tile index out of bounds : ({x}, {y}) max values : {_tileColors.GetLength(0)}, {_tileColors.GetLength(1)}");
                    }
                    else if (_tileColors[x, y] == TileColor.LOCATION) // Only switch color when tile is unlocked
                    {
                        _tileColors[x, y] = TileColor.EMPTY;
                    }
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
                    {
                        _showUI = UIColor.None;
                    }
                    else
                    {
                        _showUI = UIColor.Normal;
                    }
                    return true;
                }
                case $"{ChatForwarder.COMMAND_PREFIX}tilesanity_ui_black":
                {
                    if (_showUI == UIColor.Black)
                    {
                        _showUI = UIColor.None;
                    }
                    else
                    {
                        _showUI = UIColor.Black;
                    }
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
                _tileColors[(int)x, (int)y] = 2;
            }
        }
    }
}
