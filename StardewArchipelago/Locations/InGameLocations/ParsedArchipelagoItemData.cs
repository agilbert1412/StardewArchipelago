using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.Textures;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.GameData.Objects;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.TokenizableStrings;

namespace StardewArchipelago.Locations.InGameLocations
{
    /// <summary>Manages the data for archipelago location items.</summary>
    public class ParsedArchipelagoItemData : ParsedItemData
    {
        private const int _textureSize = 48;

        private static IMonitor _monitor;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public ParsedArchipelagoItemData(IMonitor monitor, IModHelper modHelper, ArchipelagoClient archipelago, LocationChecker locationChecker, IItemDataDefinition itemType, string itemId, int spriteIndex, string textureName, string internalName, string displayName, string description, int category, string objectType, object rawData, bool isErrorItem = false, bool excludeFromRandomSale = false) : base(itemType, itemId, spriteIndex, textureName, internalName, displayName, description, category, objectType, rawData, isErrorItem, excludeFromRandomSale)
        {
            _monitor = monitor;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        public override Texture2D GetTexture()
        {
            return ArchipelagoTextures.GetArchipelagoLogo(_monitor, _modHelper, _textureSize, ArchipelagoTextures.COLOR);
        }

        public override Rectangle GetSourceRect(int offset = 0, int? spriteIndex = null)
        {
            return new Rectangle(0, 0, _textureSize, _textureSize);
        }
    }
}
