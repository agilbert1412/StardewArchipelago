using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewArchipelago.Constants;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.Textures;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Objects;
using System.Linq;
using Color = Microsoft.Xna.Framework.Color;

namespace StardewArchipelago.Registry.Definition
{
    internal class MemeBundleItem : Object
    {

        public MemeBundleItem(ParsedItemData parsedItemData)
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
            var donatedId = GetDonatedIdMatchingItem();
            this.AdjustMenuDrawForRecipes(ref transparency, ref scaleSize);
            if (drawShadow)
            {
                this.DrawShadow(spriteBatch, location, color, layerDepth);
            }
            float num = scaleSize;
            int offset = 0;
            var texture = GetWornItemTexture(donatedId);
            var sourceRect = GetWornItemSourceRect(donatedId);
            spriteBatch.Draw(texture, location + new Vector2(32f, 32f), sourceRect, color * transparency, 0.0f, new Vector2((float)(sourceRect.Width / 2), (float)(sourceRect.Height / 2)), 4f * num, SpriteEffects.None, layerDepth);
            this.DrawMenuIcons(spriteBatch, location, scaleSize, transparency, layerDepth, drawStackNumber, color);
        }

        private Texture2D GetWornItemTexture(string donatedId)
        {
            if (string.IsNullOrWhiteSpace(donatedId))
            {
                return null;
            }

            var dataOrErrorItem = StardewValley.ItemRegistry.GetDataOrErrorItem(donatedId);
            var spriteIndex = dataOrErrorItem.SpriteIndex;
            var texture = dataOrErrorItem.GetTexture();
            return texture;
        }

        private Rectangle GetWornItemSourceRect(string donatedId)
        {
            if (string.IsNullOrWhiteSpace(donatedId))
            {
                return new Rectangle(0, 0, 16, 16);
            }

            var dataOrErrorItem = StardewValley.ItemRegistry.GetDataOrErrorItem(donatedId);
            var spriteIndex = dataOrErrorItem.SpriteIndex;
            var texture = dataOrErrorItem.GetTexture();
            var rectangle = dataOrErrorItem.GetSourceRect();
            return rectangle;
        }

        private string GetDonatedIdMatchingItem()
        {
            var clothesDonated = ModEntry.Instance.State.QualifiedIdsClothesDonated;
            if (ItemId == MemeIDProvider.WORN_HAT)
            {
                return clothesDonated.First(x => StardewValley.ItemRegistry.Create(x) is Hat);
            }
            if (ItemId == MemeIDProvider.WORN_BOOTS)
            {
                return clothesDonated.First(x => StardewValley.ItemRegistry.Create(x) is Boots);
            }
            if (ItemId == MemeIDProvider.WORN_PANTS)
            {
                return clothesDonated.First(x => StardewValley.ItemRegistry.Create(x) is Clothing pants && pants.clothesType.Value == Clothing.ClothesType.PANTS);
            }
            if (ItemId == MemeIDProvider.WORN_SHIRT)
            {
                return clothesDonated.First(x => StardewValley.ItemRegistry.Create(x) is Clothing shirt && shirt.clothesType.Value == Clothing.ClothesType.SHIRT);
            }
            if (ItemId == MemeIDProvider.WORN_LEFT_RING)
            {
                return clothesDonated.First(x => StardewValley.ItemRegistry.Create(x) is Ring ring);
            }
            if (ItemId == MemeIDProvider.WORN_RIGHT_RING)
            {
                return clothesDonated.Last(x => StardewValley.ItemRegistry.Create(x) is Ring ring);
            }

            return "";
        }
    }
}
