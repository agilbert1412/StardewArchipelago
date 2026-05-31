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
using System;
using System.Collections.Generic;
using System.Linq;

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
        public static bool DayUpdate_TemporaryBaby_Prefix(Child __instance, int dayOfMonth)
        {
            try
            {
                if (!__instance.modData.ContainsKey(TEMPORARY_BABY_KEY))
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                var despawnAge = _difficultyBalancer.BabiesDespawnAge[_archipelago.SlotData.TrapItemsDifficulty];
                if (__instance.daysOld.Value >= despawnAge)
                {
                    RemoveBaby(__instance);
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                DoBabyDayUpdate(__instance, dayOfMonth);
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(DayUpdate_TemporaryBaby_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }
        public static void DoBabyDayUpdate(Child baby, int dayOfMonth)
        {
            // baby.UpdateInvisibilityOnNewDay();
            baby.resetForNewDay(dayOfMonth);
            baby.mutex.ReleaseLock();
            // baby.moveUp = false;
            // baby.moveDown = false;
            // baby.moveLeft = false;
            // baby.moveRight = false;
            var num = (int)Game1.MasterPlayer.UniqueMultiplayerID;
            if (Game1.currentLocation is FarmHouse currentLocation && currentLocation.HasOwner)
            {
                num = (int)currentLocation.OwnerId;
            }
            var daySaveRandom = Utility.CreateDaySaveRandom(num * 2);
            ++baby.daysOld.Value;
            if (baby.daysOld.Value >= 55)
            {
                baby.Age = 3;
                baby.speed = 4;
            }
            else if (baby.daysOld.Value >= 27)
            {
                baby.Age = 2;
            }
            else if (baby.daysOld.Value >= 13)
            {
                baby.Age = 1;
            }
            if (baby.age.Value == 0 || baby.age.Value == 1)
            {
                baby.Position = new Vector2(16f, 4f) * 64f + new Vector2(0.0f, -24f);
            }
            if (baby.Age == 2)
            {
                baby.speed = 1;
                if (baby.currentLocation is FarmHouse house)
                {
                    var openPointInHouse = house.getRandomOpenPointInHouse(daySaveRandom, 1, 200);
                    if (!openPointInHouse.Equals(Point.Zero))
                    {
                        baby.setTilePosition(openPointInHouse);
                    }
                    else
                    {
                        baby.Position = new Vector2(31f, 14f) * 64f + new Vector2(0.0f, -24f);
                    }
                }
                else
                {
                    var tile = baby.currentLocation.getRandomTile() * 64f;
                    baby.Position = tile;
                }
                baby.Sprite.CurrentAnimation = null;
            }
            if (baby.Age == 3)
            {
                if (baby.currentLocation is FarmHouse house)
                {
                    var openPointInHouse = house.getRandomOpenPointInHouse(daySaveRandom, 1, 200);
                    if (!openPointInHouse.Equals(Point.Zero))
                    {
                        baby.setTilePosition(openPointInHouse);
                    }
                    else
                    {
                        var childBedSpot = house.GetChildBedSpot(baby.GetChildIndex());
                        if (!childBedSpot.Equals(Point.Zero))
                        {
                            baby.setTilePosition(childBedSpot);
                        }
                    }
                }
                else
                {
                    var tile = baby.currentLocation.getRandomTile() * 64f;
                    baby.Position = tile;
                }
                baby.Sprite.CurrentAnimation = null;
            }
            baby.reloadSprite(false);
            if (baby.Age != 2)
            {
                return;
            }

            var setCrawlerInNewDirectionMethod = _helper.Reflection.GetMethod(baby, "setCrawlerInNewDirection");
            setCrawlerInNewDirectionMethod.Invoke();
        }

        private static void RemoveBaby(Child baby)
        {
            if (Game1.player.friendshipData.ContainsKey(baby.Name))
            {
                Game1.player.friendshipData.Remove(baby.Name);
            }
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
