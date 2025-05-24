using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewArchipelago.Constants;
using StardewArchipelago.Textures;
using StardewValley.ItemTypeDefinitions;

namespace StardewArchipelago.Registry.Definition
{
    /// <summary>Manages the data for archipelago location items.</summary>
    public class ParsedMemeBundleItemData : ParsedItemData
    {
        private Texture2D _texture;
        private const int _textureSize = 48;

        public ParsedMemeBundleItemData(IItemDataDefinition itemType, string itemId, int spriteIndex, string textureName, string internalName, string displayName, string description, int category, string objectType, object rawData, bool isErrorItem = false, bool excludeFromRandomSale = false) :
            base(itemType, itemId, spriteIndex, textureName, internalName, displayName, description, category, objectType, rawData, isErrorItem, excludeFromRandomSale)
        {
            if (ItemId == MemeIDProvider.FUN_TRAP)
            {
                _texture = ArchipelagoTextures.GetArchipelagoLogo(32, ArchipelagoTextures.RED);
            }
            else
            {
                _texture = ArchipelagoTextures.GetArchipelagoLogo(_textureSize, ArchipelagoTextures.COLOR);
            }
        }

        public override Texture2D GetTexture()
        {
            return _texture;
        }

        public override Rectangle GetSourceRect(int offset = 0, int? spriteIndex = null)
        {
            if (ItemId == MemeIDProvider.FUN_TRAP)
            {
                return new Rectangle(0, 0, 32, 32);
            }
            else
            {
                return new Rectangle(0, 0, _textureSize, _textureSize);
            }
        }
    }
}
