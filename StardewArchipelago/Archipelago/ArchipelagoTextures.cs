using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace StardewArchipelago.Archipelago
{
    public static class ArchipelagoTextures
    {
        public static Texture2D GetColoredLogo(IModHelper modHelper, int size, string color)
        {
            if (!(Game1.content.ServiceProvider.GetService(typeof(IGraphicsDeviceService)) is IGraphicsDeviceService
                    service))
            {
                throw new InvalidOperationException("No Graphics Device Service");
            }

            var currentModFolder = modHelper.DirectoryPath;
            if (!Directory.Exists(currentModFolder))
            {
                throw new InvalidOperationException("Could not find StardewArchipelago folder");
            }

            var texturesFolder = "Textures";
            var archipelagoFolder = "Archipelago";
            var fileName = $"{size}x{size} {color} icon.png";
            var relativePathToTexture = Path.Combine(currentModFolder, texturesFolder, archipelagoFolder, fileName);
            if (!File.Exists(relativePathToTexture))
            {
                throw new InvalidOperationException($"Could not find archipelago logo size for color '{color}' and size '{size}x{size}'");
            }

            return Texture2D.FromFile(service.GraphicsDevice, relativePathToTexture);
        }
    }
}
