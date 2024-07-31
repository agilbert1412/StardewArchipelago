using System;
using Microsoft.Xna.Framework;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago;
using StardewValley;
using StardewValley.Buildings;

namespace StardewArchipelago.GameModifications.CodeInjections
{
    public static class GoldenClockInjections
    {
        private static ILogger _logger;
        private static StardewArchipelagoClient _archipelago;
        private static uint _lastDayGoldClockToggled;

        public static void Initialize(ILogger logger, StardewArchipelagoClient archipelago)
        {
            _logger = logger;
            _archipelago = archipelago;
            _lastDayGoldClockToggled = 0;
        }

        // public virtual bool doAction(Vector2 tileLocation, Farmer who)
        public static bool DoAction_GoldenClockIncreaseTime_Prefix(Building __instance, Vector2 tileLocation, Farmer who, ref bool __result)
        {
            try
            {
                if (Game1.MasterPlayer != who ||
                    !__instance.buildingType.Value.Equals("Gold Clock") ||
                    __instance.isTilePassable(tileLocation))
                {
                    return true; // run original logic
                }

                if (_lastDayGoldClockToggled != Game1.stats.DaysPlayed)
                {
                    _lastDayGoldClockToggled = Game1.stats.DaysPlayed;
                    return true; // run original logic
                }
                
                Game1.performTenMinuteClockUpdate();
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(DoAction_GoldenClockIncreaseTime_Prefix)}:\n{ex}");
                return true; // run original logic
            }
        }
    }
}
