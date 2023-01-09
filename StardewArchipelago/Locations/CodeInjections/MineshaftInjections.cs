using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.Tools;

namespace StardewArchipelago.Locations.CodeInjections
{
    public class MineshaftInjections
    {
        private static IMonitor _monitor;
        private static Action<string> _addCheckedLocation;

        public MineshaftInjections(IMonitor monitor, Action<string> addCheckedLocation)
        {
            _monitor = monitor;
            _addCheckedLocation = addCheckedLocation;
        }

        public static bool CheckForAction_MineshaftChest_Prefix(Chest __instance, Farmer who, bool justCheckingForActivity, ref bool __result)
        {
            try
            {
                if (justCheckingForActivity || __instance.giftbox.Value || __instance.playerChest.Value || Game1.mine == null)
                {
                    return true; // run original logic
                }

                if (__instance.items.Count <= 0)
                {
                    return true; // run original logic
                }

                who.currentLocation.playSound("openChest");
                if (__instance.synchronized.Value)
                    __instance.GetMutex().RequestLock(() => __instance.openChestEvent.Fire());
                else
                    __instance.performOpenChest();

                Game1.mine.chestConsumed();
                var obj = __instance.items[0];
                __instance.items[0] = null;
                __instance.items.RemoveAt(0);
                __result = true;

                _addCheckedLocation($"The Mines Floor {Game1.mine.mineLevel} Treasure");

                return false; // don't run original logic

            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(CheckForAction_MineshaftChest_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        public static bool AddLevelChests_Level120_Prefix(MineShaft __instance)
        {
            try
            {
                if (__instance.mineLevel != 120 || Game1.player.chestConsumedMineLevels.ContainsKey(120))
                {
                    return true; // run original logic
                }

                Game1.player.completeQuest(18);
                Game1.getSteamAchievement("Achievement_TheBottom");
                var chestPosition = new Vector2(9f, 9f);
                var items = new List<Item>();
                items.Add(new MeleeWeapon(8));
                __instance.overlayObjects[chestPosition] = new Chest(0, items, chestPosition)
                {
                    Tint = Color.Pink
                };

                return false; // don't run original logic

            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(AddLevelChests_Level120_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }
    }
}
