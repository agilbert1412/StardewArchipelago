using System;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using StardewArchipelago.Logging;
using StardewModdingAPI;
using StardewValley;

namespace StardewArchipelago.Textures
{
    public class TexturesLoader
    {
        public static Texture2D GetTexture(LogHandler logger, IModHelper modHelper, string texture, LogLevel failureLogLevel = LogLevel.Error)
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
                logger.Log($"Tried to load texture '{relativePathToTexture}', but it couldn't be found!", failureLogLevel);
                return null;
            }

            logger.LogDebug($"Loading Texture file '{relativePathToTexture}'");
            return Texture2D.FromFile(service.GraphicsDevice, relativePathToTexture);
        }
    }
}
