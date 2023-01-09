using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Tools;
using xTile.Dimensions;
using xTile.Tiles;
using Rectangle = xTile.Dimensions.Rectangle;

namespace StardewArchipelago.Locations.CodeInjections
{
    public static class MineshaftLogicInjections
    {

        private static IMonitor _monitor;

        public static void Initialize(IMonitor monitor)
        {
            _monitor = monitor;
        }

        public static bool SetLowestMineLevel_SkipToSkullCavern_Prefix(NetWorldState __instance, int value)
        {
            try
            {
                if (__instance.lowestMineLevel.Value < 120 && value > 120)
                {
                    return false;// don't run original logic
                }

                return true; // run original logic

            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(SetLowestMineLevel_SkipToSkullCavern_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }
    }
}
