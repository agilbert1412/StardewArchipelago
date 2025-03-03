﻿using System;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Pathfinding;

namespace StardewArchipelago.Items.Traps
{
    internal class TemporaryBaby : Child
    {
        private static ILogger _logger;
        private static IModHelper _helper;

        public static void Initialize(ILogger logger, IModHelper helper)
        {
            _logger = logger;
            _helper = helper;
        }

        public TemporaryBaby(string name, bool isMale, bool isDarkSkinned, Farmer parent, int age) : base(name, isMale, isDarkSkinned, parent)
        {
            Age = age;
            speed = age + 1;
        }

        public override void dayUpdate(int dayOfMonth)
        {
            if (currentLocation?.characters is null || !currentLocation.characters.Any())
            {
                return;
            }

            currentLocation.characters.Remove(this);
        }

        private static void TenMinuteUpdate(TemporaryBaby instance)
        {
            if (instance.currentLocation == null || instance.currentLocation != Game1.currentLocation)
            {
                return;
            }

            if (Game1.IsMasterGame && instance.Age == 2)
            {
                var setCrawlerInNewDirectionMethod = _helper.Reflection.GetMethod(instance, "setCrawlerInNewDirection");
                setCrawlerInNewDirectionMethod.Invoke();
                return;
            }
            if (instance.Age == 3)
            {
                instance.IsWalkingInSquare = false;
                instance.Halt();
                var randomPoint = instance.currentLocation.getRandomTile();
                instance.controller = new PathFindController(instance, instance.currentLocation, new Point((int)randomPoint.X, (int)randomPoint.Y), -1, instance.toddlerReachedDestination);
                if (instance.controller.pathToEndPoint != null && instance.currentLocation.isTileOnMap(instance.controller.pathToEndPoint.Last().X, instance.controller.pathToEndPoint.Last().Y))
                {
                    return;
                }
                instance.controller = null;
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
                    if (character is TemporaryBaby baby)
                    {
                        TenMinuteUpdate(baby);
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

        // public void tenMinuteUpdate()
        public static bool ChildTenMinuteUpdate_MoveBabiesAnywhere_Prefix(Child __instance)
        {
            try
            {
                if (__instance is TemporaryBaby temporaryBaby)
                {
                    TenMinuteUpdate(temporaryBaby);
                    return false; // run original logic;
                }

                return MethodPrefix.RUN_ORIGINAL_METHOD;;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(ChildTenMinuteUpdate_MoveBabiesAnywhere_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }
    }
}
