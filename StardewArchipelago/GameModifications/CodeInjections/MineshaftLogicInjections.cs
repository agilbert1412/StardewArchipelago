using System;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewValley.Network;

namespace StardewArchipelago.GameModifications.CodeInjections
{
    public static class MineshaftLogicInjections
    {
        private static ILogger _logger;

        public static void Initialize(ILogger logger)
        {
            _logger = logger;
        }

        public static bool SetLowestMineLevel_SkipToSkullCavern_Prefix(NetWorldState __instance, int value)
        {
            try
            {
                if (__instance.lowestMineLevel.Value < 120 && value > 120)
                {
                    return false; // don't run original logic
                }

                return true; // run original logic
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(SetLowestMineLevel_SkipToSkullCavern_Prefix)}:\n{ex}");
                return true; // run original logic
            }
        }
    }
}
