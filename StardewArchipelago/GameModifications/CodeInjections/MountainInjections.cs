﻿using System;
using Netcode;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Items.Unlocks.Vanilla;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using xTile.ObjectModel;
using xTile.Tiles;

namespace StardewArchipelago.GameModifications.CodeInjections
{
    public class MountainInjections
    {
        private const string LANDSLIDE_REMOVED_ITEM = "Landslide Removed";
        private const string RAILROAD_BOULDER_ITEM = "Railroad Boulder Removed";

        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;

        public static void Initialize(ILogger logger, IModHelper modHelper, ArchipelagoClient archipelago)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
        }

        // public void ApplyTreehouseIfNecessary()
        public static bool ApplyTreehouseIfNecessary_ApplyTreeHouseIfReceivedApItem_Prefix(Mountain __instance)
        {
            try
            {
                if (__instance.treehouseBuilt || !_archipelago.HasReceivedItem(VanillaUnlockManager.TREEHOUSE))
                {
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                var tileSheet = __instance.map.GetTileSheet("untitled tile sheet2");
                __instance.map.GetLayer("Buildings").Tiles[16, 6] = new StaticTile(__instance.map.GetLayer("Buildings"), tileSheet, BlendMode.Alpha, 197);
                __instance.map.GetLayer("Buildings").Tiles[16, 7] = new StaticTile(__instance.map.GetLayer("Buildings"), tileSheet, BlendMode.Alpha, 213);
                __instance.map.GetLayer("Back").Tiles[16, 8] = new StaticTile(__instance.map.GetLayer("Back"), tileSheet, BlendMode.Alpha, 229);
                __instance.map.GetLayer("Buildings").Tiles[16, 7].Properties["Action"] = new PropertyValue("LockedDoorWarp 3 8 LeoTreeHouse 600 2300");
                __instance.treehouseBuilt = true;
                if (!Game1.IsMasterGame)
                {
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                __instance.updateDoors();
                __instance.treehouseDoorDirty = true;

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(ApplyTreehouseIfNecessary_ApplyTreeHouseIfReceivedApItem_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public override void DayUpdate(int dayOfMonth)
        public static void DayUpdate_RailroadDependsOnApItem_Postfix(Mountain __instance, int dayOfMonth)
        {
            try
            {
                SetRailroadBlockedBasedOnArchipelagoItem(__instance);
                SetLandslideBasedOnArchipelagoItem(__instance);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(DayUpdate_RailroadDependsOnApItem_Postfix)}:\n{ex}");
                return;
            }
        }

        // protected override void resetSharedState()
        public static void ResetSharedState_RailroadDependsOnApItem_Postfix(Mountain __instance)
        {
            try
            {
                SetRailroadBlockedBasedOnArchipelagoItem(__instance);
                SetLandslideBasedOnArchipelagoItem(__instance);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(ResetSharedState_RailroadDependsOnApItem_Postfix)}:\n{ex}");
                return;
            }
        }

        public static void SetRailroadBlockedBasedOnArchipelagoItem(Mountain mountain)
        {
            // private readonly NetBool railroadAreaBlocked = new NetBool(Game1.stats.DaysPlayed < 31U);
            var railroadAreaBlockedField = _modHelper.Reflection.GetField<NetBool>(mountain, "railroadAreaBlocked");
            railroadAreaBlockedField.GetValue().Value = !_archipelago.HasReceivedItem(RAILROAD_BOULDER_ITEM);
        }

        public static void SetLandslideBasedOnArchipelagoItem(Mountain mountain)
        {
            var isLandslideRemoved = _archipelago.HasReceivedItem(LANDSLIDE_REMOVED_ITEM);

            // private readonly NetBool landslide = new NetBool(Game1.stats.DaysPlayed < 5U);
            var landslideField = _modHelper.Reflection.GetField<NetBool>(mountain, "landslide");
            landslideField.GetValue().Value = !isLandslideRemoved;
            SynchronizeLandslideLetterWithCurrentState(isLandslideRemoved);
        }

        private static void SynchronizeLandslideLetterWithCurrentState(bool isLandslideRemoved)
        {
            if (isLandslideRemoved && !Game1.player.hasOrWillReceiveMail("landslideDone"))
            {
                Game1.addMail("landslideDone", sendToEveryone: true);
            }
            if (!isLandslideRemoved && Game1.player.hasOrWillReceiveMail("landslideDone"))
            {
                Game1.player.RemoveMail("landslideDone");
            }
        }
    }
}
