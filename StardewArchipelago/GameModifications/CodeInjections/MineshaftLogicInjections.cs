using System;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.Utilities.Interfaces;
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
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(SetLowestMineLevel_SkipToSkullCavern_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }
    }
}
