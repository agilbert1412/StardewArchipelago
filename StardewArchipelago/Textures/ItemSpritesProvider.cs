using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using KaitoKid.ArchipelagoUtilities.AssetDownloader.ItemSprites;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using Microsoft.Xna.Framework.Graphics;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Logging;
using StardewModdingAPI;
using System;
using System.Text.Json;
using KaitoKid.Utilities.Interfaces;

namespace StardewArchipelago.Textures
{
    public static class ItemSpritesProvider
    {
        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static ArchipelagoItemSprites _itemSprites = null;

        public static void Initialize(ILogger logger, IModHelper modHelper)
        {
            if (_itemSprites != null || !ModEntry.Instance.Config.CustomAssets)
            {
                return;
            }

            _logger = logger;
            _modHelper = modHelper;
            var redownloadDelay = TimeSpan.FromDays(28);
            _itemSprites = new ArchipelagoItemSprites(_logger, DeserializeAliases, redownloadDelay);
        }

        private static ItemSpriteAliases DeserializeAliases(string jsonAliases)
        {
            var options = new JsonSerializerOptions { AllowTrailingCommas = true };
            var aliases = JsonSerializer.Deserialize<ItemSpriteAliases>(jsonAliases, options);
            return aliases;
        }
        public static Texture2D GetCorrectTexture(ILogger logger, IModHelper modHelper, StardewArchipelagoClient archipelago, ScoutedLocation scoutedLocation, Hint relatedHint)
        {
            Initialize(logger, modHelper);
            return GetCorrectTexture(archipelago, scoutedLocation, relatedHint);
        }

        public static Texture2D GetCorrectTexture(StardewArchipelagoClient archipelago, ScoutedLocation scoutedLocation, Hint relatedHint)
        {
            if (TryGetItemSprite(archipelago, scoutedLocation, out var itemSprite))
            {
                return itemSprite;
            }

            var genericTextureName = GetCorrectGenericTextureName(scoutedLocation, relatedHint);
            return ArchipelagoTextures.GetArchipelagoLogo(48, genericTextureName);
        }

        public static bool TryGetItemSprite(StardewArchipelagoClient archipelago, ScoutedLocation scoutedLocation, out Texture2D itemSprite)
        {
            var config = ModEntry.Instance.Config;
            if (config.CustomAssets && _itemSprites != null && _itemSprites.TryGetCustomAsset(scoutedLocation, archipelago.GameName, config.CustomAssetGameFlexible, config.CustomAssetGenericGame, out var sprite))
            {
                if (ArchipelagoTextures.TryGetItemSprite(_logger, _modHelper, sprite, out itemSprite))
                {
                    return true;
                }
            }

            itemSprite = null;
            return false;
        }

        private static string GetCorrectGenericTextureName(ScoutedLocation scoutedLocation, Hint relatedHint)
        {
            if (scoutedLocation == null)
            {
                return ArchipelagoTextures.WHITE;
            }

            var hintTexture = GetHintTexture(relatedHint);
            if (hintTexture != null)
            {
                return hintTexture;
            }

            if (scoutedLocation.ClassificationFlags.HasFlag(ItemFlags.Advancement))
            {
                return ArchipelagoTextures.PROGRESSION;
            }

            if (scoutedLocation.ClassificationFlags.HasFlag(ItemFlags.Trap))
            {
                return ArchipelagoTextures.RED;
            }

            if (scoutedLocation.ClassificationFlags.HasFlag(ItemFlags.NeverExclude))
            {
                return ArchipelagoTextures.COLOR;
            }

            return ArchipelagoTextures.BLACK;
        }

        private static string GetHintTexture(Hint relatedHint)
        {
            if (relatedHint == null || relatedHint.Found)
            {
                return null;
            }

            if (relatedHint.Status == HintStatus.Priority)
            {
                return ArchipelagoTextures.PLEADING;
            }

            if (relatedHint.Status == HintStatus.Avoid)
            {
                return ArchipelagoTextures.RED;
            }

            return null;
        }
    }
}
