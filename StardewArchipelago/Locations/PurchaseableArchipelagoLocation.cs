using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Reflection;
using Archipelago.MultiClient.Net.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Textures;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Internal;
using xTile.Dimensions;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace StardewArchipelago.Locations
{
    internal class PurchaseableArchipelagoLocation : Item
    {
        public static readonly string PURCHASEABLE_AP_LOCATION_ID = $"{ModEntry.Instance.ModManifest.UniqueID}.{nameof(PurchaseableArchipelagoLocation)}";

        private const string ARCHIPELAGO_PREFIX = "Archipelago: ";
        private const string ARCHIPELAGO_SHORT_PREFIX = "AP: ";
        private Texture2D _archipelagoTexture;

        private string _locationDisplayName;
        private string _description;
        private LocationChecker _locationChecker;
        private List<Item> _extraMaterialsRequired;

        public string LocationName { get; }

        public PurchaseableArchipelagoLocation(string locationName, IMonitor monitor, IModHelper modHelper, LocationChecker locationChecker, ArchipelagoClient archipelago, Hint[] myActiveHints, Action purchaseCallback = null) : this(locationName, locationName, monitor, modHelper, locationChecker, archipelago, myActiveHints, purchaseCallback)
        {
        }

        public PurchaseableArchipelagoLocation(string locationDisplayName, string locationName, IMonitor monitor, IModHelper modHelper, LocationChecker locationChecker, ArchipelagoClient archipelago, Hint[] myActiveHints, Action purchaseCallback = null)
        {
            var prefix = locationDisplayName.Length < 18 ? ARCHIPELAGO_PREFIX : ARCHIPELAGO_SHORT_PREFIX;
            _locationDisplayName = $"{prefix}{locationDisplayName}";
            Name = _locationDisplayName;
            LocationName = locationName;
            var scoutedLocation = archipelago.ScoutSingleLocation(LocationName);
            _description = scoutedLocation == null ? ScoutedLocation.GenericItemName() : scoutedLocation.ToString();
            _locationChecker = locationChecker;
            _extraMaterialsRequired = new List<Item>();

            var isHinted = myActiveHints.Any(hint => archipelago.GetLocationName(hint.LocationId).Equals(locationName, StringComparison.OrdinalIgnoreCase));
            var desiredTextureName = isHinted ? ArchipelagoTextures.PLEADING : ArchipelagoTextures.COLOR;
            _archipelagoTexture = ArchipelagoTextures.GetArchipelagoLogo(monitor, modHelper, 48, desiredTextureName);
        }

        public void AddMaterialRequirement(Item requiredItem)
        {
            _extraMaterialsRequired.Add(requiredItem);
        }

        public override bool CanBuyItem(Farmer who)
        {
            foreach (var item in _extraMaterialsRequired)
            {
                if (who.Items.CountId(item.QualifiedItemId) < item.Stack)
                {
                    return false;
                }
            }

            return true;
        }

        public override bool actionWhenPurchased(string shopId)
        {
            foreach (var item in _extraMaterialsRequired)
            {
                Game1.player.Items.ReduceId(item.QualifiedItemId, item.Stack);
            }
            _locationChecker.AddCheckedLocation(LocationName);
            return true;
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth,
            StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            var position = location + new Vector2(16f, 16f);
            var sourceRectangle = new Rectangle(0, 0, 48, 48);
            var transparentColor = color * transparency;
            var origin = new Vector2(8f, 8f);
            spriteBatch.Draw(_archipelagoTexture, position, sourceRectangle, transparentColor, 0.0f, origin, scaleSize, SpriteEffects.None, layerDepth);
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

        public override int maximumStackSize()
        {
            return 1;
        }

        protected override Item GetOneNew()
        {
            return this;
        }

        public override string DisplayName => _locationDisplayName;

        public override string TypeDefinitionId => "(AP)";

        public static IEnumerable<ItemQueryResult> Create(string locationName, IModHelper modHelper, LocationChecker locationChecker, ArchipelagoClient archipelago, Hint[] myActiveHints)
        {
            if (string.IsNullOrWhiteSpace(locationName))
            {
                throw new ArgumentException($"Could not create {nameof(PurchaseableArchipelagoLocation)} because there was no provided location name");
            }

            if (locationChecker.IsLocationMissing(locationName))
            {
                return new ItemQueryResult[] { new(new PurchaseableArchipelagoLocation(locationName.Trim(), modHelper, locationChecker, archipelago, myActiveHints)) };
            }

            return Enumerable.Empty<ItemQueryResult>();
        }
    }
}
