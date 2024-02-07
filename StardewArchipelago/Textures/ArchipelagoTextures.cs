using System;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
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

        public static readonly string[] ValidLogos = { COLOR, WHITE, BLUE, BLACK, RED, PLEADING };

        public static Texture2D GetColoredLogo(IModHelper modHelper, int size, string color)
        {
            var archipelagoFolder = "Archipelago";
            var fileName = $"{size}x{size} {color} icon.png";
            var relativePathToTexture = Path.Combine(archipelagoFolder, fileName);
            var texture = TexturesLoader.GetTexture(modHelper, relativePathToTexture);
            if (texture == null)
            {
                throw new InvalidOperationException($"Could not find texture {fileName}");
            }

            return texture;
        }
    }
}
