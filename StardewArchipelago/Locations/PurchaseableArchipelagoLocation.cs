using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewArchipelago.Archipelago;
using StardewValley;

namespace StardewArchipelago.Locations
{
    internal class PurchaseableArchipelagoLocation : Item
    {
        private const string ARCHIPELAGO_PREFIX = "Archipelago: ";
        private Texture2D _archipelagoTexture;

        private string _locationDisplayName;
        private string _apLocationName;
        private string _description;
        private Action _purchaseCallBack;
        private List<Item> _extraMaterialsRequired;

        public PurchaseableArchipelagoLocation(string locationDisplayName, string apLocationName, Action purchaseCallBack, ArchipelagoClient archipelago)
        {
            _locationDisplayName = $"{ARCHIPELAGO_PREFIX}{locationDisplayName}";
            _apLocationName = apLocationName;
            var scoutedLocation = archipelago.ScoutSingleLocation(_apLocationName);
            _description = scoutedLocation == null ? ScoutedLocation.GenericItemName() : scoutedLocation.ToString();
            _purchaseCallBack = purchaseCallBack;
            _extraMaterialsRequired = new List<Item>();


            if (!(Game1.content.ServiceProvider.GetService(typeof(IGraphicsDeviceService)) is IGraphicsDeviceService service))
                throw new InvalidOperationException("No Graphics Device Service");

            _archipelagoTexture = Texture2D.FromFile(service.GraphicsDevice, @"Mods\StardewArchipelago\Textures\archipelago.png");
        }

        public void AddMaterialRequirement(Item requiredItem)
        {
            _extraMaterialsRequired.Add(requiredItem);
        }

        public override bool CanBuyItem(Farmer who)
        {
            if (!base.CanBuyItem(who))
            {
                return false;
            }

            foreach (var item in _extraMaterialsRequired)
            {
                if (!who.hasItemInInventory(item.ParentSheetIndex, item.Stack))
                {
                    return false;
                }
            }

            return true;
        }

        public override bool actionWhenPurchased()
        {
            foreach (var item in _extraMaterialsRequired)
            {
                Game1.player.removeItemsFromInventory(item.ParentSheetIndex, item.Stack);
            }
            _purchaseCallBack();
            return true;
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth,
            StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            spriteBatch.Draw(_archipelagoTexture, location + new Vector2(16f, 16f),
                new Rectangle(0, 0, 48, 48),
                color * transparency, 0.0f, new Vector2(8f, 8f), scaleSize, SpriteEffects.None, layerDepth);
        }

        public override string getDescription()
        {
            var descriptionWithExtraMaterials = $"{_description}{Environment.NewLine}";
            foreach (var material in _extraMaterialsRequired)
            {
                descriptionWithExtraMaterials += $"{Environment.NewLine}{material.Stack} {material.DisplayName}";
            }

            return descriptionWithExtraMaterials;
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
