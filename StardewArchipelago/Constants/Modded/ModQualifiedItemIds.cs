using System;
using StardewArchipelago.Stardew.Ids.Vanilla;
using StardewModdingAPI;
using StardewArchipelago.Constants.Vanilla;

namespace StardewArchipelago.Constants.Modded
{
    internal class ModQualifiedItemIds
    {
        public static readonly string TEMPERED_DAGGER = QualifiedWeaponId("Tempered Galaxy Dagger");
        public static readonly string TEMPERED_SWORD = QualifiedWeaponId("Tempered Galaxy Sword");
        public static readonly string TEMPERED_HAMMER = QualifiedWeaponId("Tempered Galaxy Hammer");

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