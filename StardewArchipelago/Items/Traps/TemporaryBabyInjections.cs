using System;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.Utilities.Interfaces;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Pathfinding;

namespace StardewArchipelago.Items.Traps
{
    internal class TemporaryBabyInjections
    {
        public const string TEMPORARY_BABY_KEY = "TemporaryBaby";

        private static ILogger _logger;
        private static IModHelper _helper;
        private static StardewArchipelagoClient _archipelago;
        private static TrapDifficultyBalancer _difficultyBalancer;

        public static void Initialize(ILogger logger, IModHelper helper, StardewArchipelagoClient archipelago)
        {
            _logger = logger;
            _helper = helper;
            _archipelago = archipelago;
            _difficultyBalancer = new TrapDifficultyBalancer();
        }

        // public override void dayUpdate(int dayOfMonth)
        public static void DayUpdate_TemporaryBaby_Postfix(Child __instance, int dayOfMonth)
        {
            try
            {
                if (!__instance.modData.ContainsKey(TEMPORARY_BABY_KEY))
                {
                    return;
                }

                var despawnAge = _difficultyBalancer.BabiesDespawnAge[_archipelago.SlotData.TrapItemsDifficulty];
                if (__instance.daysOld.Value >= despawnAge)
                {
                    RemoveBaby(__instance);
                    return;
                }

                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(DayUpdate_TemporaryBaby_Postfix)}:\n{ex}");
                return;
            }
        }

        private static void RemoveBaby(Child baby)
        {
            if (baby.currentLocation?.characters is null || !baby.currentLocation.characters.Any())
            {
                return;
            }

            baby.currentLocation.characters.Remove(baby);
        }

        // public void tenMinuteUpdate()
        public static bool TenMinuteUpdate_TemporaryBaby_Prefix(Child __instance)
        {
            try
            {
                if (!__instance.modData.ContainsKey(TEMPORARY_BABY_KEY))
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (__instance.currentLocation == null || __instance.currentLocation != Game1.currentLocation)
                {
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                if (Game1.IsMasterGame && __instance.Age == 2)
                {
                    var setCrawlerInNewDirectionMethod = _helper.Reflection.GetMethod(__instance, "setCrawlerInNewDirection");
                    setCrawlerInNewDirectionMethod.Invoke();
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }
                if (__instance.Age == 3)
                {
                    __instance.IsWalkingInSquare = false;
                    __instance.Halt();
                    var randomPoint = __instance.currentLocation.getRandomTile();
                    __instance.controller = new PathFindController(__instance, __instance.currentLocation, new Point((int)randomPoint.X, (int)randomPoint.Y), -1, __instance.toddlerReachedDestination);
                    if (__instance.controller.pathToEndPoint != null && __instance.currentLocation.isTileOnMap(__instance.controller.pathToEndPoint.Last().X, __instance.controller.pathToEndPoint.Last().Y))
                    {
                        return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                    }
                    __instance.controller = null;
                }

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(TenMinuteUpdate_TemporaryBaby_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public override void performTenMinuteUpdate(int timeOfDay)
        public static bool GameLocationPerformTenMinuteUpdate_MoveBabiesAnywhere_Prefix(GameLocation __instance, int timeOfDay)
        {
            try
            {
                if (__instance is Farm || __instance is FarmHouse || __instance != Game1.currentLocation)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;;
                }

                foreach (var character in __instance.characters)
                {
                    if (character is Child baby)
                    {
                        baby.tenMinuteUpdate();
                    }
                }

                return MethodPrefix.RUN_ORIGINAL_METHOD;;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(GameLocationPerformTenMinuteUpdate_MoveBabiesAnywhere_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }
    }
}
