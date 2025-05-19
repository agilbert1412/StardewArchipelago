using System;
using System.Collections.Generic;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.Extensions;
using StardewArchipelago.Locations.InGameLocations;
using StardewArchipelago.Logging;
using StardewModdingAPI;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using Object = StardewValley.Object;

namespace StardewArchipelago.Registry.Definition
{
    /// <summary>Manages the data for archipelago location items.</summary>
    public class MemeBundleItemDataDefinition : BaseItemDataDefinition
    {
        /// <inheritdoc />
        public override string Identifier => QualifiedItemIds.MEME_BUNDLE_ITEM_QUALIFIER;

        /// <inheritdoc />
        public override string StandardDescriptor => QualifiedItemIds.MEME_BUNDLE_ITEM_QUALIFIER.Substring(1, 3);

        /// <inheritdoc />
        public override IEnumerable<string> GetAllIds() => new[]
        {
            MemeIDProvider.FUN_TRAP, MemeIDProvider.TRASH_TUNA,
            MemeIDProvider.WORN_BOOTS, MemeIDProvider.WORN_HAT, MemeIDProvider.WORN_PANTS,
            MemeIDProvider.WORN_SHIRT, MemeIDProvider.WORN_LEFT_RING, MemeIDProvider.WORN_RIGHT_RING,
        };

        /// <inheritdoc />
        public override bool Exists(string itemId) => itemId != null && GetAllIds().Any(x => itemId.Equals(x) || itemId.Equals(Identifier + x));

        /// <inheritdoc />
        public override ParsedItemData GetData(string itemId)
        {
            var type = "MemeBundleItem";
            var category = type.GetHash();
            var unqualifiedId = QualifiedItemIds.UnqualifyId(itemId);
            var name = MemeIDProvider.MemeItemNames[unqualifiedId];
            var displayName = name;
            var description = "";
            return new ParsedMemeBundleItemData(this, itemId, 0, null, name, displayName, description, category, type, null, excludeFromRandomSale: true);
        }

        /// <inheritdoc />
        public override Rectangle GetSourceRect(
            ParsedItemData data,
            Texture2D texture,
            int spriteIndex)
        {
            if (data is not ParsedMemeBundleItemData memeBundleItemData)
            {
                throw new ArgumentNullException(nameof(data));
            }
            return texture != null ? Game1.getSourceRectForStandardTileSheet(texture, spriteIndex, 16, 16) : throw new ArgumentNullException(nameof(texture));
        }

        /// <inheritdoc />
        public override Item CreateItem(ParsedItemData data)
        {
            //if (data is not ParsedMemeBundleItemData memeBundleItemData)
            //{
            //    throw new ArgumentNullException(nameof(data));
            //}

            return new MemeBundleItem(data);
        }
    }
}
