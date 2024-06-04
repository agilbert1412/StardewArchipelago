using System;
using StardewArchipelago.Stardew.Ids.Vanilla;
using StardewModdingAPI;
using StardewArchipelago.Constants.Vanilla;

namespace StardewArchipelago.Constants.Modded
{
    internal class ModItemIds
    {
        public static readonly string TEMPERED_DAGGER = "FlashShifter.StardewValleyExpandedCP_Tempered_Galaxy_Dagger";
        public static readonly string TEMPERED_SWORD = "FlashShifter.StardewValleyExpandedCP_Tempered_Galaxy_Sword";
        public static readonly string TEMPERED_HAMMER = "FlashShifter.StardewValleyExpandedCP_Tempered_Galaxy_Hammer";

        private static string QualifiedObjectId(string objectId)
        {
            return $"{QualifiedItemIds.OBJECT_QUALIFIER}{objectId}";
        }

        private static string QualifiedWeaponId(string weaponId)
        {
            return $"{QualifiedItemIds.WEAPON_QUALIFIER}{weaponId}";
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