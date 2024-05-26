using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants;
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
    public class ArchipelagoLocationDataDefinition : BaseItemDataDefinition
    {

        private static IMonitor _monitor;
        private static IModHelper _modHelper;
        private static LocationChecker _locationChecker;
        private static ArchipelagoClient _archipelago;

        public static void Initialize(IMonitor monitor, IModHelper modHelper, LocationChecker locationChecker, ArchipelagoClient archipelago)
        {
            _monitor = monitor;
            _modHelper = modHelper;
            _locationChecker = locationChecker;
            _archipelago = archipelago;
        }

        /// <inheritdoc />
        public override string Identifier => "(AP)";

        /// <inheritdoc />
        public override string StandardDescriptor => "AP";

        /// <inheritdoc />
        public override IEnumerable<string> GetAllIds() => Enumerable.Empty<string>();

        /// <inheritdoc />
        public override bool Exists(string itemId) => itemId != null && (itemId.StartsWith(Identifier + IDProvider.AP_LOCATION) || itemId.StartsWith(IDProvider.AP_LOCATION));

        /// <inheritdoc />
        public override ParsedItemData GetData(string itemId)
        {
            if (!itemId.StartsWith(Identifier + IDProvider.AP_LOCATION) && !itemId.StartsWith(IDProvider.AP_LOCATION))
            {
                return null;
            }
            var itemData = new ParsedItemData(this, itemId, 0, "", "", "", "", 0, "", null);
            return itemData;
        }

        /// <inheritdoc />
        public override Rectangle GetSourceRect(
          ParsedItemData data,
          Texture2D texture,
          int spriteIndex)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            return texture != null ? Game1.getSourceRectForStandardTileSheet(texture, spriteIndex, 16, 16) : throw new ArgumentNullException(nameof(texture));
        }

        /// <inheritdoc />
        public override Item CreateItem(ParsedItemData data)
        {
            var id = data.ItemId;
            var apLocationPrefix = IDProvider.AP_LOCATION;
            var indexOfPrefixStart = id.IndexOf(apLocationPrefix, StringComparison.InvariantCultureIgnoreCase);
            var locationName = id[(indexOfPrefixStart + apLocationPrefix.Length + 1)..];
            return new ArchipelagoLocation(locationName, _monitor, _modHelper, _locationChecker, _archipelago, _archipelago.GetMyActiveHints());
        }
    }
}
