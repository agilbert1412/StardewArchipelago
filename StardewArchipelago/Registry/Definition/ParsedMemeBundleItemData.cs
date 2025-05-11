using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewArchipelago.Constants;
using StardewArchipelago.Logging;
using StardewArchipelago.Textures;
using StardewModdingAPI;
using StardewValley.ItemTypeDefinitions;

namespace StardewArchipelago.Registry.Definition
{
    /// <summary>Manages the data for archipelago location items.</summary>
    public class ParsedMemeBundleItemData : ParsedItemData
    {
        private const int _textureSize = 48;

        public ParsedMemeBundleItemData(IItemDataDefinition itemType, string itemId, int spriteIndex, string textureName, string internalName, string displayName, string description, int category, string objectType, object rawData, bool isErrorItem = false, bool excludeFromRandomSale = false) :
            base(itemType, itemId, spriteIndex, textureName, internalName, displayName, description, category, objectType, rawData, isErrorItem, excludeFromRandomSale)
        {
        }

        public override Texture2D GetTexture()
        {
            if (ItemId == MemeIDProvider.FUN_TRAP)
            {
                return ArchipelagoTextures.GetArchipelagoLogo(32, ArchipelagoTextures.RED);
            }

            return ArchipelagoTextures.GetArchipelagoLogo(_textureSize, ArchipelagoTextures.COLOR);
        }

        public override Rectangle GetSourceRect(int offset = 0, int? spriteIndex = null)
        {
            if (ItemId == MemeIDProvider.FUN_TRAP)
            {
                return new Rectangle(0, 0, 32, 32);
            }

            return new Rectangle(0, 0, _textureSize, _textureSize);
        }
    }
}
