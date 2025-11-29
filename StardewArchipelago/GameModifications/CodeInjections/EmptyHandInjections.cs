using System;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.Utilities.Interfaces;
using Microsoft.Xna.Framework;
using StardewArchipelago.Stardew;
using StardewValley;
using StardewValley.TerrainFeatures;
using xTile.Dimensions;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace StardewArchipelago.GameModifications.CodeInjections
{
    public class EmptyHandInjections
    {
        private static ILogger _logger;
        private static ArchipelagoClient _archipelago;
        private static StardewItemManager _stardewItemManager;

        private static Object _lastHitObject = null;
        private static TerrainFeature _lastHitTerrainFeature = null;
        private static int _hitsRemaining = -1;

        public static void Initialize(ILogger logger, ArchipelagoClient archipelago, StardewItemManager stardewItemManager)
        {
            _logger = logger;
            _archipelago = archipelago;
            _stardewItemManager = stardewItemManager;
        }

        // public virtual bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
        public static bool CheckAction_BreakSomethingByHand_Prefix(GameLocation __instance, Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who, ref bool __result)
        {
            try
            {
                var toolLocation = who.GetToolLocation();
                var tileVector = new Vector2((int)(toolLocation.X / 64), (int)(toolLocation.Y / 64));
                if (!__instance.objects.ContainsKey(tileVector))
                {
                    return BreakBigObjectBareHanded(__instance, tileVector, tileLocation, who, ref __result);
                }

                var objectToBreak = __instance.objects[tileVector];

                if (!objectToBreak.IsWeeds() && !objectToBreak.IsBreakableStone() && !objectToBreak.IsTwig())
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (who.CurrentTool != null || who.ActiveObject != null)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (objectToBreak != _lastHitObject)
                {
                    _lastHitObject = objectToBreak;
                    _hitsRemaining = CalculateHitsToBreak(objectToBreak);
                }

                _hitsRemaining--;
                Game1.player.Stamina -= GetStaminaCostToBreak(objectToBreak);
                if (_hitsRemaining > 0)
                {
                    objectToBreak.shakeTimer = 100;
                    objectToBreak.playNearbySoundAll("hammer");
                    __result = true;
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                objectToBreak.playNearbySoundAll("hammer");
                if (objectToBreak.IsTwig())
                {
                    Game1.createRadialDebris(__instance, 12, tileLocation.X, tileLocation.Y, Game1.random.Next(4, 10), false);
                }
                else if (objectToBreak.IsWeeds())
                {
                    Game1.createRadialDebris(__instance, 13, tileLocation.X, tileLocation.Y, Game1.random.Next(4, 10), false, color: Color.Green);
                }
                else if (objectToBreak.IsBreakableStone())
                {
                    Game1.createRadialDebris(__instance, 14, tileLocation.X, tileLocation.Y, Game1.random.Next(4, 10), false);
                }
                objectToBreak.performRemoveAction();
                __instance.objects.Remove(objectToBreak.TileLocation);
                __result = true;
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CheckAction_BreakSomethingByHand_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }
        private static float GetStaminaCostToBreak(Object objectToBreak)
        {
            if (objectToBreak.IsTwig())
            {
                return 3 - Game1.player.ForagingLevel * 0.15f;
            }
            if (objectToBreak.IsBreakableStone())
            {
                return 3 - Game1.player.MiningLevel * 0.15f;
            }
            if (objectToBreak.IsTwig())
            {
                return 3 - Game1.player.FarmingLevel * 0.15f;
            }

            return 3;
        }

        // public override bool performUseAction(Vector2 tileLocation)
        public static bool PerformUseAction_BreakTreeByHand_Prefix(Tree __instance, Vector2 tileLocation, ref bool __result)
        {
            try
            {
                if (Game1.player.CurrentTool != null || Game1.player.ActiveObject != null)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (__instance.growthStage.Value <= 0 || __instance.growthStage.Value >= 5 || __instance.tapped.Value)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (__instance != _lastHitTerrainFeature)
                {
                    _lastHitTerrainFeature = __instance;
                    _hitsRemaining = CalculateHitsToBreak(__instance);
                }

                _hitsRemaining--;
                Game1.player.Stamina -= 4 - Game1.player.ForagingLevel * 0.2f;
                if (_hitsRemaining > 0)
                {
                    __instance.shakeTimer = 100;
                    __instance.shake(tileLocation, false);
                    __instance.Location.localSound("hammer");
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                var tileRect = new Rectangle((int)(tileLocation.X * 64), (int)(tileLocation.Y * 64), 64, 64);
                var location = __instance.Location;
                location.playSound("axchop", tileLocation);
                Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(10, 20), false);
                location.terrainFeatures.Remove(tileLocation);
                location.largeTerrainFeatures?.RemoveWhere(largeFeature => largeFeature.getBoundingBox().Intersects(tileRect));

                __result = true;
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(PerformUseAction_BreakTreeByHand_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static int CalculateHitsToBreak(Object objectToBreak)
        {
            if (objectToBreak.IsWeeds())
            {
                return 4;
            }
            if (objectToBreak.IsTwig())
            {
                return 6;
            }
            if (objectToBreak.IsBreakableStone())
            {
                return 8;
            }

            return 12;
        }

        private static int CalculateHitsToBreak(TerrainFeature terrainFeatureToBreak)
        {
            if (terrainFeatureToBreak is Tree tree)
            {
                return tree.growthStage.Value * 6;
            }

            return 20;
        }

        private static bool BreakBigObjectBareHanded(GameLocation gameLocation, Vector2 tileVector, Location tileLocation, Farmer who, ref bool __result)
        {
            //Only allow removing debris in the farm
            if (gameLocation?.name.Value != "Farm")
                return MethodPrefix.RUN_ORIGINAL_METHOD;

            ResourceClump objectToBreak = null;
            foreach (var resourceClump in gameLocation.resourceClumps)
            {
                if (resourceClump.occupiesTile((int)tileVector.X, (int)tileVector.Y))
                {
                    objectToBreak = resourceClump;
                    break;
                }
            }
            if (objectToBreak == null)
                return MethodPrefix.RUN_ORIGINAL_METHOD;

            if (who.CurrentTool != null || who.ActiveObject != null)
            {
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            
            if (objectToBreak != _lastHitTerrainFeature)
            {
                _lastHitTerrainFeature = objectToBreak;
                _hitsRemaining = 20;
            }
            _hitsRemaining--;
            // We need to only try to break it every so often or it's too quick
            if (_hitsRemaining > 0)
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            _hitsRemaining = 20;

            var objectType = objectToBreak.parentSheetIndex.Value;
            int radialDebris;
            switch (objectType)
            {
                case 600: //stump
                case 602: //hard twig
                    Game1.player.Stamina -= 4 - Game1.player.ForagingLevel * 0.2f;
                    radialDebris = 12;
                    break;
                case 672: //boulder
                    Game1.player.Stamina -= 4 - Game1.player.MiningLevel * 0.2f;
                    radialDebris = 14;
                    break;
                default:
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            Game1.createRadialDebris(Game1.currentLocation, radialDebris, tileLocation.X + Game1.random.Next(objectToBreak.width.Value / 2 + 1), tileLocation.Y + Game1.random.Next(objectToBreak.height.Value / 2 + 1), Game1.random.Next(4, 9), resource: false);

            objectToBreak.health.Value -= 0.1f;

            objectToBreak.Location.localSound("hammer");
            if (objectToBreak.health.Value > 0)
            {
                objectToBreak.shakeTimer = 100;
                __result = true;
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }

            if (objectToBreak.parentSheetIndex.Value == 672)
            {
                objectToBreak.Location.localSound("boulderBreak");
                Game1.createRadialDebris(Game1.currentLocation, 32, tileLocation.X, tileLocation.Y, Game1.random.Next(4, 9), resource: false);
            }
            else
            {
                objectToBreak.Location.localSound("stumpCrack");
                Game1.createRadialDebris(Game1.currentLocation, 34, tileLocation.X, tileLocation.Y, Game1.random.Next(4, 9), resource: false);
            }
            gameLocation.resourceClumps.Remove(objectToBreak);
            __result = true;
            return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
        }
    }
}
