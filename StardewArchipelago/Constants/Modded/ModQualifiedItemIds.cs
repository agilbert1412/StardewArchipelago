using System;
using StardewArchipelago.Stardew.Ids.Vanilla;
using StardewModdingAPI;
using StardewArchipelago.Constants.Vanilla;

namespace StardewArchipelago.Constants.Modded
{
    internal class ModQualifiedItemIds
    {
        private static readonly string TEMPERED_DAGGER = QualifiedObjectId("Tempered Galaxy Dagger");

        private static string QualifiedObjectId(string objectId)
        {
            return $"{QualifiedItemIds.OBJECT_QUALIFIER}{objectId}";
        }

        private static bool IsType(string itemId, string qualifier)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return false;
            }

            return itemId.StartsWith(qualifier, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}