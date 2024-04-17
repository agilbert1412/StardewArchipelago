﻿using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using Object = StardewValley.Object;

namespace StardewArchipelago.GameModifications.CodeInjections
{
    internal class VoidMayoInjections
    {
        private const string VOID_MAYONNAISE = "(O)308";
        private static IMonitor _monitor;

        public static void Initialize(IMonitor monitor)
        {
            _monitor = monitor;
        }

        // public virtual Object getFish(float millisecondsAfterNibble, int bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, string locationName = null)
        public static bool GetFish_FishVoidMayo_PreFix(GameLocation __instance, float millisecondsAfterNibble, int bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, string locationName, ref Object __result)
        {
            try
            {
                if (__instance.Name.Equals("WitchSwamp") && Game1.random.NextDouble() < 0.25 && !Game1.player.Items.ContainsId(VOID_MAYONNAISE, 1))
                {
                    __result = new Object(VOID_MAYONNAISE, 1);
                    return false; // don't run original logic
                }

                return true; // run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(GetFish_FishVoidMayo_PreFix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }
    }
}
