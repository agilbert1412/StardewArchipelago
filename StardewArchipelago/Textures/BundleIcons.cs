using System;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace StardewArchipelago.Textures
{
    public static class BundleIcons
    {
        public const string BUNDLE_SUFFIX = "bundle";

        public static Texture2D GetBundleIcon(IModHelper modHelper, string bundleName)
        {
            var bundlesFolder = "Bundles";
            var cleanName = bundleName.Replace("'", "").Replace(" ", "_");
            var fileNameBundleName = $"{cleanName}_{BUNDLE_SUFFIX}.png";
            var pathToTexture = Path.Combine(bundlesFolder, fileNameBundleName);
            return TexturesLoader.GetTexture(modHelper, pathToTexture);
        }
    }
}
