using System;
using System.Linq;
using Netcode;
using StardewArchipelago.Archipelago;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Minigames;
using StardewValley.Quests;

namespace StardewArchipelago.Locations.CodeInjections
{
    public static class ArcadeMachineInjections
    {
        private static IMonitor _monitor;
        private static IModHelper _helper;
        private static LocationChecker _locationChecker;

        public static void Initialize(IMonitor monitor, IModHelper helper, LocationChecker locationChecker)
        {
            _monitor = monitor;
            _helper = helper;
            _locationChecker = locationChecker;
        }

        public static bool UsePowerup_PrairieKingVictory_Prefix(AbigailGame __instance, int which)
        {
            try
            {
                if (__instance.activePowerups.ContainsKey(which) || which != -3)
                {
                    return true; // run original logic
                }

                _locationChecker.AddCheckedLocation("Journey of the Prairie King Victory");

                return true; // run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(UsePowerup_PrairieKingVictory_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        public static bool EndCutscene_JunimoKartVictory_Prefix(MineCart __instance)
        {
            try
            {
                var gamemode = _helper.Reflection.GetField<int>(__instance, "gamemode");
                if (gamemode.GetValue() != 3)
                {
                    return true; // run original logic
                }

                _locationChecker.AddCheckedLocation("Junimo Kart Victory");
                return true; // run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(EndCutscene_JunimoKartVictory_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }
    }
}
