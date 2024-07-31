using KaitoKid.ArchipelagoUtilities.Net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Logging;
using StardewArchipelago.Textures;
using StardewModdingAPI;
using StardewValley.ItemTypeDefinitions;

namespace StardewArchipelago.Locations.InGameLocations
{
    /// <summary>Manages the data for archipelago location items.</summary>
    public class ParsedArchipelagoItemData : ParsedItemData
    {
        private const int _textureSize = 48;

        private LogHandler _logger;
        private IModHelper _modHelper;
        private ArchipelagoClient _archipelago;
        private LocationChecker _locationChecker;

        public ParsedArchipelagoItemData(LogHandler logger, IModHelper modHelper, ArchipelagoClient archipelago, LocationChecker locationChecker, IItemDataDefinition itemType, string itemId, int spriteIndex, string textureName, string internalName, string displayName, string description, int category, string objectType, object rawData, bool isErrorItem = false, bool excludeFromRandomSale = false) : base(itemType, itemId, spriteIndex, textureName, internalName, displayName, description, category, objectType, rawData, isErrorItem, excludeFromRandomSale)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        public override Texture2D GetTexture()
        {
            return ArchipelagoTextures.GetArchipelagoLogo(_logger, _modHelper, _textureSize, ArchipelagoTextures.COLOR);
        }

        public override Rectangle GetSourceRect(int offset = 0, int? spriteIndex = null)
        {
            return new Rectangle(0, 0, _textureSize, _textureSize);
        }
    }
}
