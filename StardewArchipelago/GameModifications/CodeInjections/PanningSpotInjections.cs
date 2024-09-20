using System;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Tools;

namespace StardewArchipelago.GameModifications.CodeInjections
{
    public static class PanningSpotInjections
    {
        private static ILogger _logger;
        private static ArchipelagoClient _archipelago;

        public static void Initialize(ILogger logger, ArchipelagoClient archipelago)
        {
            _logger = logger;
            _archipelago = archipelago;
        }

        // public virtual bool performOrePanTenMinuteUpdate(Random r)
        public static bool PerformOrePanTenMinuteUpdate_AllowPanningSpotsAlways_Prefix(GameLocation __instance, Random r, ref bool __result)
        {
            try
            {
                Point point;
                // No need for the glittering boulder, just the pan is enough
                if (__instance is not Beach)
                {
                    point = __instance.orePanPoint.Value;
                    if (point.Equals(Point.Zero) && r.NextBool())
                    {
                        for (var index = 0; index < 8; ++index)
                        {
                            var p = new Point(r.Next(0, __instance.Map.RequireLayer("Back").LayerWidth), r.Next(0, __instance.Map.RequireLayer("Back").LayerHeight));
                            if (!__instance.isOpenWater(p.X, p.Y) || FishingRod.distanceToLand(p.X, p.Y, __instance, true) > 1 || __instance.getTileIndexAt(p, "Buildings") != -1)
                            {
                                continue;
                            }
                            if (Game1.player.currentLocation.Equals(__instance))
                            {
                                __instance.playSound("slosh");
                            }
                            __instance.orePanPoint.Value = p;
                            __result = true;
                            return false; // don't run original logic
                        }
                        __result = false;
                        return false; // don't run original logic
                    }
                }
                point = __instance.orePanPoint.Value;
                if (!point.Equals(Point.Zero) && r.NextDouble() < 0.1)
                {
                    __instance.orePanPoint.Value = Point.Zero;
                }
                __result = false;
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(PerformOrePanTenMinuteUpdate_AllowPanningSpotsAlways_Prefix)}:\n{ex}");
                return true; // run original logic
            }
        }

        // public override bool performOrePanTenMinuteUpdate(Random r)
        public static bool PerformOrePanTenMinuteUpdateOnIslandNorth_AllowPanningSpotsAlways_Prefix(IslandNorth __instance, Random r, ref bool __result)
        {
            try
            {
                var point = __instance.orePanPoint.Value;
                if (point.Equals(Point.Zero) && r.NextBool())
                {
                    for (var index = 0; index < 3; ++index)
                    {
                        var p = new Point(r.Next(4, 15), r.Next(45, 70));
                        if (!__instance.isOpenWater(p.X, p.Y) || FishingRod.distanceToLand(p.X, p.Y, (GameLocation)__instance) > 1 || __instance.getTileIndexAt(p, "Buildings") != -1)
                        {
                            continue;
                        }
                        if (Game1.player.currentLocation.Equals((GameLocation)__instance))
                        {
                            __instance.playSound("slosh");
                        }
                        __instance.orePanPoint.Value = p;
                        __result = true;
                        return false; // don't run original logic
                    }
                    __result = false;
                    return false; // don't run original logic
                }
                point = __instance.orePanPoint.Value;
                if (!point.Equals(Point.Zero) && r.NextDouble() < 0.2)
                {
                    __instance.orePanPoint.Value = Point.Zero;
                }
                __result = false;
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(PerformOrePanTenMinuteUpdateOnIslandNorth_AllowPanningSpotsAlways_Prefix)}:\n{ex}");
                return true; // run original logic
            }
        }
    }
}
