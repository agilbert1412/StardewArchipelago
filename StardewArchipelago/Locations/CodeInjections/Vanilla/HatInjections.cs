using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.Items.Unlocks.Vanilla;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData;
using StardewValley.Locations;
using StardewValley.Tools;
using xTile.Dimensions;
using Object = StardewValley.Object;
using Rectangle = xTile.Dimensions.Rectangle;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using Netcode;
using StardewValley.Objects;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public static class HatInjections
    {
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

                var hatName = hat.Name;
                if (_hatAliases.ContainsKey(hat.QualifiedItemId))
                {
                    _locationChecker.AddCheckedLocations(_hatAliases[hat.QualifiedItemId].Select(x => $"Wear {x}").ToArray());
                }
                _locationChecker.AddCheckedLocation($"Wear {hatName}");
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(OnEquip_EquippedHat_Postfix)}:\n{ex}");
                return;
            }
        }
    }
}
