using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace StardewArchipelago.Textures
{
    public class TexturesLoader
    {
        public static Texture2D GetTexture(IModHelper modHelper, string texture)
        {
            if (Game1.content.ServiceProvider.GetService(typeof(IGraphicsDeviceService)) is not IGraphicsDeviceService service)
            {
                throw new InvalidOperationException("No Graphics Device Service");
            }

            var currentModFolder = modHelper.DirectoryPath;
            if (!Directory.Exists(currentModFolder))
            {
                throw new InvalidOperationException("Could not find StardewArchipelago folder");
            }

            const string texturesFolder = "Textures";
            var relativePathToTexture = Path.Combine(currentModFolder, texturesFolder, texture);
            if (!File.Exists(relativePathToTexture))
            {
                throw new InvalidOperationException($"Could not find texture {texture}");
            }

            return Texture2D.FromFile(service.GraphicsDevice, relativePathToTexture);
        }
    }
}
