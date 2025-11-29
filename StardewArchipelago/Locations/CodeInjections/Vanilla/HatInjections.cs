using System;
using System.Collections.Generic;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using StardewArchipelago.Constants.Vanilla;
using StardewModdingAPI;
using StardewValley;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.Utilities.Interfaces;
using StardewArchipelago.Goals;
using StardewValley.Objects;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public static class HatInjections
    {
        public const string HATSANITY_PREFIX = "Wear ";

        private static Dictionary<string, string[]> _hatAliases = new()
        {
            { QualifiedItemIds.STEEL_PAN_HAT, new[] { "Copper Pan" } },
            { QualifiedItemIds.GOLD_PAN_HAT, new[] { "Copper Pan", "Steel Pan" } },
            { QualifiedItemIds.IRIDIUM_PAN_HAT, new[] { "Copper Pan", "Steel Pan", "Gold Pan" } },
            { QualifiedItemIds.PARTY_HAT_RED, new[] { "Party Hat (Red)" } },
            { QualifiedItemIds.PARTY_HAT_BLUE, new[] { "Party Hat (Blue)" } },
            { QualifiedItemIds.PARTY_HAT_GREEN, new[] { "Party Hat (Green)" } },
        };

        private static ILogger _logger;
        private static IModHelper _helper;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(ILogger logger, IModHelper modHelper, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _helper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        // public virtual void onEquip(Farmer who)
        public static void OnEquip_EquippedHat_Postfix(Item __instance, Farmer who)
        {
            try
            {
                if (__instance is not Hat hat)
                {
                    return;
                }

                _locationChecker.AddCheckedLocations(GetHatLocations(hat).ToArray());
                GoalCodeInjection.CheckMadHatterGoalCompletion();
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(OnEquip_EquippedHat_Postfix)}:\n{ex}");
                return;
            }
        }

        public static IEnumerable<string> GetHatLocations(Hat hat)
        {
            if (hat == null)
            {
                yield break;
            }

            var hatName = hat.Name;
            if (_hatAliases.ContainsKey(hat.QualifiedItemId))
            {
                foreach (var hatLocation in _hatAliases[hat.QualifiedItemId].Select(x => $"{HATSANITY_PREFIX}{x}"))
                {
                    yield return hatLocation;
                }
            }

            yield return $"{HATSANITY_PREFIX}{hatName}";
        }
    }
}
