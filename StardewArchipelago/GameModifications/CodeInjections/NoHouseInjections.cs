using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.Utilities.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;
using StardewValley;
using StardewValley.Characters;
using StardewValley.GameData.Characters;
using StardewValley.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using StardewArchipelago.Locations.CodeInjections.Vanilla;
using StardewValley.Buildings;

namespace StardewArchipelago.GameModifications.CodeInjections
{
    internal class NoHouseInjections
    {
        private static ILogger _logger;
        private static StardewArchipelagoClient _archipelago;

        public static void Initialize(ILogger logger, StardewArchipelagoClient archipelago)
        {
            _logger = logger;
            _archipelago = archipelago;
        }

        public static void BeHomelessIfNeeded()
        {
            if (!_archipelago.SlotData.StartWithout.HasFlag(StartWithout.House))
            {
                return;
            }

            if (_archipelago.HasReceivedItem(CarpenterInjections.BUILDING_PROGRESSIVE_HOUSE))
            {
                return;
            }

            Game1.player.currentLocation = Game1.RequireLocation("Farm");
            Game1.currentLocation = Game1.player.currentLocation;
        }

        private static void ConstructHouseIfNeeded()
        {

        }

        // public static bool AddCharacterIfNecessary(string characterId, bool bypassConditions = false)
        //public static bool AddCharacterIfNecessary_ConsiderArrivals_Prefix(string characterId, ref bool bypassConditions)
        //{
        //    try
        //    {
        //        var allowed = AllowedToExist(characterId);

        //        if (!allowed)
        //        {
        //            return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
        //        }

        //        bypassConditions = true;
        //        return MethodPrefix.RUN_ORIGINAL_METHOD;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"Failed in {nameof(AddCharacterIfNecessary_ConsiderArrivals_Prefix)}:\n{ex}");
        //        return MethodPrefix.RUN_ORIGINAL_METHOD;
        //    }
        //}
    }
}