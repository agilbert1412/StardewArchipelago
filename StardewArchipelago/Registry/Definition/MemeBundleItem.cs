using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewArchipelago.Constants;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.Textures;
using StardewValley;
using StardewValley.Objects;
using Color = Microsoft.Xna.Framework.Color;

namespace StardewArchipelago.Registry.Definition
{
    internal class MemeBundleItem : Object
    {

        public MemeBundleItem(ParsedMemeBundleItemData parsedItemData)
        {
            ItemId = parsedItemData.ItemId;
            netName.Value = parsedItemData.InternalName;
        }

        public override bool isPlaceable()
        {
            return false;
        }

        public override int maximumStackSize()
        {
            return 1;
        }

        protected override Item GetOneNew()
        {
            return this;
        }

        public override bool canBeTrashed()
        {
            return false;
        }

        public override string TypeDefinitionId => QualifiedItemIds.MEME_BUNDLE_ITEM_QUALIFIER;

        public override string DisplayName => Name;

        public override bool CanBuyItem(Farmer who)
        {
            return false;
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            base.drawInMenu(spriteBatch, location, scaleSize / 2, transparency, layerDepth, drawStackNumber, color, drawShadow);
        }
    }
}
