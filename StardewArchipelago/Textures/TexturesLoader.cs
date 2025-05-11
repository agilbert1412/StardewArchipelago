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

        private static LogHandler _logger;
        private static IModHelper _modHelper;

        public static void Initialize(LogHandler logger, IModHelper modHelper)
        {
            _logger = logger;
            _modHelper = modHelper;
        }

        public static Texture2D GetTexture(string texture, LogLevel failureLogLevel = LogLevel.Error)
        {
            if (Game1.content.ServiceProvider.GetService(typeof(IGraphicsDeviceService)) is not IGraphicsDeviceService service)
            {
                throw new InvalidOperationException("No Graphics Device Service");
            }

            var currentModFolder = _modHelper.DirectoryPath;
            if (!Directory.Exists(currentModFolder))
            {
                throw new InvalidOperationException("Could not find StardewArchipelago folder");
            }

            const string texturesFolder = "Textures";
            var relativePathToTexture = Path.Combine(currentModFolder, texturesFolder, texture);
            if (!File.Exists(relativePathToTexture))
            {
                _logger.Log($"Tried to load texture '{relativePathToTexture}', but it couldn't be found!", failureLogLevel);
                return null;
            }

            _logger.LogDebug($"Loading Texture file '{relativePathToTexture}'");
            return Texture2D.FromFile(service.GraphicsDevice, relativePathToTexture);
        }
    }
}
