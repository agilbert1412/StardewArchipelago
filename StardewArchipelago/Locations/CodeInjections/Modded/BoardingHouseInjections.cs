using System;
using System.Linq;
using System.Reflection;
using Netcode;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewArchipelago.Archipelago;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System.Collections.Generic;

namespace StardewArchipelago.Locations.CodeInjections.Modded
{
    public static class BoardingHouseInjections
    {
        private const string CHEST_1A = "Abandoned Treasure - Floor 1A";
        private const string CHEST_1B = "Abandoned Treasure - Floor 1B";
        private const string CHEST_2A = "Abandoned Treasure - Floor 2A";
        private const string CHEST_2B = "Abandoned Treasure - Floor 2B";
        private const string CHEST_3 = "Abandoned Treasure - Floor 3";
        private const string CHEST_4 = "Abandoned Treasure - Floor 4";
        private const string CHEST_5 = "Abandoned Treasure - Floor 5";
        private static readonly Dictionary<string, string> abandonedMines = new(){
            {"Custom_BoardingHouse_AbandonedMine1A", CHEST_1A}, {"Custom_BoardingHouse_AbandonedMine1B", CHEST_1B}, 
            {"Custom_BoardingHouse_AbandonedMine2A", CHEST_2A}, {"Custom_BoardingHouse_AbandonedMine2B", CHEST_2B}, 
            {"Custom_BoardingHouse_AbandonedMine3", CHEST_3}, {"Custom_BoardingHouse_AbandonedMine4", CHEST_4},
            {"Custom_BoardingHouse_AbandonedMine5", CHEST_5},
        };
        private static IMonitor _monitor;
        private static LocationChecker _locationChecker;

        public static void Initialize(IMonitor monitor, LocationChecker locationChecker)
        {
            _monitor = monitor;
            _locationChecker = locationChecker;
        }
        
        public static bool CheckForAction_TreasureChestLocation_Prefix(Chest __instance, Farmer who, bool justCheckingForActivity, ref bool __result)
        {
            try
            {
                var playerLocation = Game1.player.currentLocation.Name;
                if (justCheckingForActivity || !abandonedMines.Keys.Contains(playerLocation))
                    return false; //don't run original logic
                if (__instance.items.Count <= 0)
                {
                    return true; // run original logic
                }
                var currentChest = abandonedMines[playerLocation];
                who.currentLocation.playSound("openChest");
                if (__instance.synchronized.Value)
                    __instance.GetMutex().RequestLock(() => __instance.openChestEvent.Fire());
                else
                    __instance.performOpenChest();
                var obj = __instance.items[0];
                __instance.items[0] = null;
                __instance.items.RemoveAt(0);
                __result = true;
                _locationChecker.AddCheckedLocation(currentChest);
                Game1.playSound("openChest");
                return false; //don't run original logic (first chest is a check)

            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(CheckForAction_TreasureChestLocation_Prefix)}:\n{ex}", LogLevel.Error);
                return true;
            }
        }
    }
}