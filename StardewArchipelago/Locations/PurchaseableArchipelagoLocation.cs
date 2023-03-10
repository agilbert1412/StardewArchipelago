using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewArchipelago.Archipelago;
using StardewValley;

namespace StardewArchipelago.Locations
{
    internal class PurchaseableArchipelagoLocation : Item
    {
        private const string ARCHIPELAGO_PREFIX = "Archipelago: ";
        private const string ARCHIPELAGO_SHORT_PREFIX = "AP: ";
        private Texture2D _archipelagoTexture;

        private string _locationDisplayName;
        private string _apLocationName;
        private string _description;
        private LocationChecker _locationChecker;
        private List<Item> _extraMaterialsRequired;
        private Action _purchaseCallBack;

        public PurchaseableArchipelagoLocation(string locationDisplayName, string apLocationName, LocationChecker locationChecker, ArchipelagoClient archipelago, Action purchaseCallback = null)
        {
            var prefix = locationDisplayName.Length < 18 ? ARCHIPELAGO_PREFIX : ARCHIPELAGO_SHORT_PREFIX;
            _locationDisplayName = $"{prefix}{locationDisplayName}";
            _apLocationName = apLocationName;
            var scoutedLocation = archipelago.ScoutSingleLocation(_apLocationName);
            _description = scoutedLocation == null ? ScoutedLocation.GenericItemName() : scoutedLocation.ToString();
            _locationChecker = locationChecker;
            _extraMaterialsRequired = new List<Item>();
            _purchaseCallBack = purchaseCallback;


            if (!(Game1.content.ServiceProvider.GetService(typeof(IGraphicsDeviceService)) is IGraphicsDeviceService
                    service))
            {
                throw new InvalidOperationException("No Graphics Device Service");
            }

            var directories = Directory.EnumerateDirectories("Mods", "StardewArchipelago", SearchOption.AllDirectories).ToArray();
            if (!directories.Any())
            {
                throw new InvalidOperationException("Could not find StardewArchipelago folder");
            }

            foreach (var directory in directories)
            {
                var subfolders = Directory.GetDirectories(directory);
                var texturesFolder = "Textures";
                if (subfolders.Any(x => x.EndsWith(texturesFolder)))
                {
                    var relativePathToTexture = Path.Combine(directory, texturesFolder, "archipelago.png");
                    _archipelagoTexture = Texture2D.FromFile(service.GraphicsDevice, relativePathToTexture);
                    return;
                }
            }

            throw new InvalidOperationException("Could not find StardewArchipelago mod folder");
        }

        public void AddMaterialRequirement(Item requiredItem)
        {
            _extraMaterialsRequired.Add(requiredItem);
        }

        public override bool CanBuyItem(Farmer who)
        {
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
            _locationChecker.AddCheckedLocation(_apLocationName);
            _purchaseCallBack?.Invoke();
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
