﻿using System;
using System.Collections.Generic;
using System.IO;
using KaitoKid.ArchipelagoUtilities.Net.ItemSprites;
using Microsoft.Xna.Framework.Graphics;
using StardewArchipelago.Logging;
using StardewModdingAPI;

namespace StardewArchipelago.Textures
{
    public static class ArchipelagoTextures
    {
        public const string COLOR = "color";
        public const string WHITE = "white";
        public const string BLUE = "blue";
        public const string BLACK = "black";
        public const string RED = "red";
        public const string PLEADING = "pleading";
        public const string PROGRESSION = "progression";
        public const string JOJA = "joja";

        public const string ICON_SET_CUSTOM = "Custom";
        public const string ICON_SET_ORIGINAL = "Original";

        public static readonly string[] ValidLogos = { COLOR, WHITE, BLACK, RED, PLEADING };

        private static LogHandler _logger;
        private static IModHelper _modHelper;

        private static Dictionary<string, Texture2D> _loadedIcons;
        private static Dictionary<string, Texture2D> _loadedBushes;

        public static void Initialize(LogHandler logger, IModHelper modHelper)
        {
            _logger = logger;
            _modHelper = modHelper;
            TexturesLoader.Initialize(_logger, _modHelper);
            _loadedIcons = new Dictionary<string, Texture2D>();
            _loadedBushes = new Dictionary<string, Texture2D>();
        }

        public static Texture2D GetArchipelagoLogo(int size, string color, string preferredIconSet = null)
        {
            var cacheKey = $"{size}_{color}_{preferredIconSet ?? "-"}";
            if (_loadedIcons.ContainsKey(cacheKey))
            {
                return _loadedIcons[cacheKey];
            }

            var archipelagoFolder = "Archipelago";
            preferredIconSet = GetChosenIconSet(preferredIconSet);
            var fileName = $"{size}x{size} {color} icon.png";
            var relativePathToTexture = Path.Combine(archipelagoFolder, preferredIconSet, fileName);
            var texture = TexturesLoader.GetTexture(relativePathToTexture, LogLevel.Trace);
            if (texture == null)
            {
                // Let's try to get the icon from the other set
                preferredIconSet = GetOtherIconSet(preferredIconSet);
                fileName = $"{size}x{size} {color} icon.png";
                relativePathToTexture = Path.Combine(archipelagoFolder, preferredIconSet, fileName);
                texture = TexturesLoader.GetTexture(relativePathToTexture);
                if (texture == null)
                {
                    throw new InvalidOperationException($"Could not find texture {fileName}");
                }
            }

            _loadedIcons.Add(cacheKey, texture);
            return texture;
        }

        public static Texture2D GetArchipelagoBush(LogHandler logger, IModHelper modHelper, string preferredIconSet = null)
        {
            var cacheKey = preferredIconSet ?? "--";
            if (_loadedBushes.ContainsKey(cacheKey))
            {
                return _loadedBushes[cacheKey];
            }

            var archipelagoFolder = "Archipelago";
            preferredIconSet = GetChosenIconSet(preferredIconSet);
            var fileName = $"walnut_bush.png";
            var relativePathToTexture = Path.Combine(archipelagoFolder, preferredIconSet, fileName);
            var texture = TexturesLoader.GetTexture(relativePathToTexture, LogLevel.Trace);
            if (texture == null)
            {
                // Let's try to get the icon from the other set
                preferredIconSet = GetOtherIconSet(preferredIconSet);
                relativePathToTexture = Path.Combine(archipelagoFolder, preferredIconSet, fileName);
                texture = TexturesLoader.GetTexture(relativePathToTexture);
                if (texture == null)
                {
                    throw new InvalidOperationException($"Could not find texture {fileName}");
                }
            }

            _loadedBushes.Add(cacheKey, texture);
            return texture;
        }

        private static string GetPreferredIconSet()
        {
            return ModEntry.Instance.Config.UseCustomArchipelagoIcons ? ICON_SET_CUSTOM : ICON_SET_ORIGINAL;
        }

        private static string GetChosenIconSet(string iconSet)
        {
            if (iconSet == ICON_SET_ORIGINAL || iconSet == ICON_SET_CUSTOM)
            {
                return iconSet;
            }
            return GetPreferredIconSet();
        }

        private static string GetOtherIconSet(string iconSet)
        {
            if (iconSet == ICON_SET_CUSTOM)
            {
                return ICON_SET_ORIGINAL;
            }
            if (iconSet == ICON_SET_ORIGINAL)
            {
                return ICON_SET_CUSTOM;
            }
            return GetPreferredIconSet();
        }

        public static bool TryGetItemSprite(LogHandler logger, IModHelper modHelper, ItemSprite sprite, out Texture2D texture2D)
        {
            var customAssetsFolder = "Custom Assets";
            var fileName = sprite.FilePath;
            texture2D = TexturesLoader.GetTexture(fileName, LogLevel.Trace);
            return texture2D != null;
        }
    }
}
