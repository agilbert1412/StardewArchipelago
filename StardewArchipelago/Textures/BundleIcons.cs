using System.IO;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Logging;

namespace StardewArchipelago.Textures
{
    public static class BundleIcons
    {
        public const string BUNDLE_SUFFIX = "bundle";

        public static Texture2D GetBundleIcon(LogHandler logger, IModHelper modHelper, string bundleName, LogLevel failureLogLevel = LogLevel.Error)
        {
            var bundlesFolder = "Bundles";
            var cleanName = bundleName.Replace("'", "").Replace(" ", "_").ToLower();
            var fileNameBundleName = $"{cleanName}_{BUNDLE_SUFFIX}.png";
            var pathToTexture = Path.Combine(bundlesFolder, fileNameBundleName);
            logger.LogDebug($"Attempting to load bundle icon '{pathToTexture}'");
            return TexturesLoader.GetTexture(logger, modHelper, pathToTexture, failureLogLevel);
        }
    }
}
