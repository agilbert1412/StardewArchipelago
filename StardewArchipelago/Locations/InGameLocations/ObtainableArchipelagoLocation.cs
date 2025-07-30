﻿using System;
using System.Collections.Generic;
using System.Linq;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.ItemSprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.Logging;
using StardewArchipelago.Textures;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Internal;
using StardewValley.Objects;
using Color = Microsoft.Xna.Framework.Color;

namespace StardewArchipelago.Locations.InGameLocations
{
    internal class ObtainableArchipelagoLocation : SpecialItem
    {
        private static ArchipelagoItemSprites _itemSprites = null;

        private const string ARCHIPELAGO_PREFIX = "Archipelago: ";
        private const string ARCHIPELAGO_SHORT_PREFIX = "AP: ";
        private const string ARCHIPELAGO_NO_PREFIX = "";
        public const string EXTRA_MATERIALS_KEY = "Extra Materials";
        protected static Hint[] _activeHints;
        protected static uint _lastTimeUpdatedActiveHints;

        private readonly Texture2D _archipelagoTexture;

        protected ILocationChecker _locationChecker;
        protected string _locationDisplayName;

        public string LocationName { get; set; }

        private readonly string _description;

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
                    var materialQualifiedId = extraMaterialFields[0];
                    var materialAmount = int.Parse(extraMaterialFields[1]);
                    _extraMaterialsRequired.Add(materialQualifiedId, materialAmount);
                }

                return _extraMaterialsRequired;
            }
        }

        public ObtainableArchipelagoLocation()
        {
            var locationName = string.IsNullOrWhiteSpace(LocationName) ? "Unknown Location" : LocationName;
            ModEntry.Instance.Logger.LogError($"The game attempted to use the parameterless constructor in {nameof(ObtainableArchipelagoLocation)} ({locationName}), potentially as part of a serialization process");
        }

        public ObtainableArchipelagoLocation(string locationName, LogHandler logger, IModHelper modHelper, ILocationChecker locationChecker, StardewArchipelagoClient archipelago, Hint[] myActiveHints, bool allowScouting) : this(locationName, locationName, logger, modHelper, locationChecker, archipelago, myActiveHints, allowScouting)
        {
        }

        public ObtainableArchipelagoLocation(string locationDisplayName, string locationName, LogHandler logger, IModHelper modHelper, ILocationChecker locationChecker, StardewArchipelagoClient archipelago, Hint[] myActiveHints, bool allowScouting)
        {
            if (_itemSprites == null)
            {
                _itemSprites = new ArchipelagoItemSprites(logger);
            }

            // Prefix removed for now, because the inconsistency makes it ugly
            // var prefix = locationDisplayName.Length < 18 ? ARCHIPELAGO_PREFIX : ARCHIPELAGO_NO_PREFIX;
            var prefix = ARCHIPELAGO_NO_PREFIX;
            _locationDisplayName = $"{prefix}{locationDisplayName}";
            Name = _locationDisplayName;
            LocationName = locationName;
            ItemId = $"{IDProvider.AP_LOCATION}_{LocationName /*.Replace(" ", "_")*/}";

            _locationChecker = locationChecker;

            var relatedHint = myActiveHints.FirstOrDefault(hint => archipelago.GetLocationName(hint).Equals(locationName, StringComparison.OrdinalIgnoreCase));

            if (allowScouting)
            {
                var scoutedLocation = archipelago.ScoutStardewLocation(LocationName);
                _archipelagoTexture = GetCorrectTexture(logger, modHelper, scoutedLocation, archipelago, relatedHint);
                _description = scoutedLocation == null ? ScoutedLocation.GenericItemName() : scoutedLocation.ToString();
            }
            else if (relatedHint != null)
            {
                _archipelagoTexture = GetCorrectTexture(logger, modHelper, null, archipelago, relatedHint);
                _description = archipelago.GetHintString(relatedHint);
            }
            else
            {
                _archipelagoTexture = GetCorrectTexture(logger, modHelper, null, archipelago, null);
                _description = ScoutedLocation.GenericItemName();
            }
        }

        protected virtual Texture2D GetCorrectTexture(LogHandler logger, IModHelper modHelper, ScoutedLocation scoutedLocation, StardewArchipelagoClient archipelago, Hint relatedHint)
        {
            var config = ModEntry.Instance.Config;
            if (config.CustomAssets && _itemSprites.TryGetCustomAsset(scoutedLocation, archipelago.GameName, config.CustomAssetGameFlexible, config.CustomAssetGenericGame, out var sprite))
            {
                if (ArchipelagoTextures.TryGetItemSprite(logger, modHelper, sprite, out var texture2D))
                {
                    return texture2D;
                }
            }

            var genericTextureName = GetCorrectGenericTextureName(scoutedLocation, relatedHint);
            return ArchipelagoTextures.GetArchipelagoLogo(48, genericTextureName);
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

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth,
            StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            var position = location + new Vector2(16f, 16f);
            var sourceRectangle = new Rectangle(0, 0, 48, 48);
            var transparentColor = color * transparency;
            var origin = new Vector2(8f, 8f);
            spriteBatch.Draw(_archipelagoTexture, position, sourceRectangle, transparentColor, 0.0f, origin, scaleSize, SpriteEffects.None, layerDepth);
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

        public override string TypeDefinitionId => QualifiedItemIds.ARCHIPELAGO_QUALIFER;

        public override string DisplayName => _locationDisplayName;

        public override bool CanBuyItem(Farmer who)
        {
            foreach (var (qualifiedId, amount) in ExtraMaterialsRequired)
            {
                if (who.Items.CountId(qualifiedId) < amount)
                {
                    return false;
                }
            }

            return true;
        }

        public override bool actionWhenPurchased(string shopId)
        {
            foreach (var (qualifiedId, amount) in ExtraMaterialsRequired)
            {
                Game1.player.Items.ReduceId(qualifiedId, amount);
            }
            SendCheck();
            return true;
        }

        public virtual void SendCheck()
        {
            _locationChecker.AddCheckedLocation(LocationName);
        }

        public override string getDescription()
        {
            var descriptionWithExtraMaterials = $"{_description}{Environment.NewLine}";
            foreach (var (qualifiedId, amount) in ExtraMaterialsRequired)
            {
                var resolvedItem = ItemRegistry.Create(qualifiedId, amount);
                descriptionWithExtraMaterials += $"{Environment.NewLine}{amount} {resolvedItem.Name}";
            }

            return descriptionWithExtraMaterials;
        }

        public static IEnumerable<ItemQueryResult> Create(string locationName, LogHandler logger, IModHelper modHelper, StardewArchipelagoClient archipelago, StardewLocationChecker locationChecker)
        {
            if (string.IsNullOrWhiteSpace(locationName))
            {
                throw new ArgumentException($"Could not create {nameof(ObtainableArchipelagoLocation)} because there was no provided location name");
            }

            if (locationChecker.IsLocationMissing(locationName))
            {
                var currentTime = GetUniqueTimeIdentifier();
                if (currentTime != _lastTimeUpdatedActiveHints || _activeHints == null)
                {
                    _activeHints = archipelago.GetMyActiveHints();
                    _lastTimeUpdatedActiveHints = currentTime;
                }
                var item = new ObtainableArchipelagoLocation(locationName.Trim(), logger, modHelper, locationChecker, archipelago, _activeHints, true);
                return new ItemQueryResult[] { new(item) };
            }

            return Enumerable.Empty<ItemQueryResult>();
        }

        protected static uint GetUniqueTimeIdentifier()
        {
            return Game1.stats.DaysPlayed * 3000 + (uint)Game1.timeOfDay;
        }
    }
}
