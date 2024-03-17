using System;
using System.Collections.Generic;
using System.Linq;
using Archipelago.MultiClient.Net.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Textures;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Internal;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace StardewArchipelago.Locations
{
    internal class PurchaseableArchipelagoLocation : Item
    {
        private const string ARCHIPELAGO_PREFIX = "Archipelago: ";
        private const string ARCHIPELAGO_SHORT_PREFIX = "AP: ";
        public const string EXTRA_MATERIALS_KEY = "Extra Materials";
        private Texture2D _archipelagoTexture;

        private string _locationDisplayName;
        private string _description;
        private LocationChecker _locationChecker;

        private Dictionary<string, int> _extraMaterialsRequired;

        private Dictionary<string, int> ExtraMaterialsRequired
        {
            get
            {
                if (_extraMaterialsRequired != null)
                {
                    return _extraMaterialsRequired;
                }

                _extraMaterialsRequired = new Dictionary<string, int>();
                if (modData == null || !modData.ContainsKey(EXTRA_MATERIALS_KEY))
                {
                    return _extraMaterialsRequired;
                }

                var extraMaterialsString = modData[EXTRA_MATERIALS_KEY];
                foreach (var extraMaterialString in extraMaterialsString.Split(",", StringSplitOptions.RemoveEmptyEntries))
                {
                    var extraMaterialFields = extraMaterialString.Split(":");
                    var materialId = extraMaterialFields[0];
                    var materialAmount = int.Parse(extraMaterialFields[1]);
                    _extraMaterialsRequired.Add(materialId, materialAmount);
                }

                return _extraMaterialsRequired;
            }
        }

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

            var isHinted = myActiveHints.Any(hint => archipelago.GetLocationName(hint.LocationId).Equals(locationName, StringComparison.OrdinalIgnoreCase));
            var desiredTextureName = isHinted ? ArchipelagoTextures.PLEADING : ArchipelagoTextures.COLOR;
            _archipelagoTexture = ArchipelagoTextures.GetArchipelagoLogo(monitor, modHelper, 48, desiredTextureName);
        }

        public override bool CanBuyItem(Farmer who)
        {
            foreach (var (id, amount) in ExtraMaterialsRequired)
            {
                if (who.Items.CountId(id) < amount)
                {
                    return false;
                }
            }

            return true;
        }

        public override bool actionWhenPurchased(string shopId)
        {
            foreach (var (id, amount) in ExtraMaterialsRequired)
            {
                Game1.player.Items.ReduceId(id, amount);
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
            foreach (var (id, amount) in ExtraMaterialsRequired)
            {
                descriptionWithExtraMaterials += $"{Environment.NewLine}{amount} {DataLoader.Objects(Game1.content)[id].Name}";
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

        public static IEnumerable<ItemQueryResult> Create(string locationName, IMonitor monitor, IModHelper modHelper, LocationChecker locationChecker, ArchipelagoClient archipelago,
            Dictionary<string, object> contextCustomFields, Hint[] myActiveHints)
        {
            if (string.IsNullOrWhiteSpace(locationName))
            {
                throw new ArgumentException($"Could not create {nameof(PurchaseableArchipelagoLocation)} because there was no provided location name");
            }

            if (locationChecker.IsLocationMissing(locationName))
            {
                var purchaseableCheck = new PurchaseableArchipelagoLocation(locationName.Trim(), monitor, modHelper, locationChecker, archipelago, myActiveHints);
                return new ItemQueryResult[] { new(purchaseableCheck) };
            }

            return Enumerable.Empty<ItemQueryResult>();
        }
    }
}
