using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace StardewArchipelago.Locations
{
    internal class PurchaseableArchipelagoLocation : Item
    {
        private string _locationDisplayName;
        private string _description;
        private Texture2D _spriteSheet;
        private int _indexOfMenuItemView;
        private Action _purchaseCallBack;

        public PurchaseableArchipelagoLocation(string locationDisplayName, Texture2D spriteSheet, int indexOfMenuItemView, Action purchaseCallBack)
        {
            _locationDisplayName = locationDisplayName;
            _description = "";
            _spriteSheet = spriteSheet;
            _indexOfMenuItemView = indexOfMenuItemView;
            _purchaseCallBack = purchaseCallBack;
        }

        public override bool actionWhenPurchased()
        {
            _purchaseCallBack();
            return true;
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth,
            StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            spriteBatch.Draw(_spriteSheet, location + new Vector2(32f, 32f), new Rectangle?(Game1.getSquareSourceRectForNonStandardTileSheet(_spriteSheet, 16, 16, _indexOfMenuItemView)), color * transparency, 0.0f, new Vector2(8f, 8f), 4f * scaleSize, SpriteEffects.None, layerDepth);
        }

        public override string getDescription()
        {
            return _description;
        }

        public override bool isPlaceable()
        {
            return false;
        }

        public override Item getOne()
        {
            return this;
        }

        public override int maximumStackSize()
        {
            return 1;
        }

        public override int addToStack(Item stack)
        {
            return 1;
        }

        public override string DisplayName
        {
            get => _locationDisplayName;
            set => _locationDisplayName = value;
        }

        public override int Stack { get; set; }
    }
}
