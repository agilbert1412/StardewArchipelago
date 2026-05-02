using System;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.Utilities.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants.Modded;
using StardewArchipelago.Goals;
using StardewModdingAPI;
using StardewValley.Network;

namespace StardewArchipelago.GameModifications.CodeInjections
{
    public static class MineshaftLogicInjections
    {
        private static ILogger _logger;
        private static IModHelper _helper;
        private static StardewArchipelagoClient _archipelago;
        private static bool _hasSkullElevator;

        public static void Initialize(ILogger logger, IModHelper helper, StardewArchipelagoClient archipelago)
        {
            _logger = logger;
            _helper = helper;
            _archipelago = archipelago;
            var normalizedSkullElevatorName = ModsManager.GetNormalizedModName(ModNames.SKULL_CAVERN_ELEVATOR);
            _hasSkullElevator = _helper.ModRegistry.GetAll().ToList().Any(x => ModsManager.GetNormalizedModName(x.Manifest.Name).Equals(normalizedSkullElevatorName, StringComparison.InvariantCultureIgnoreCase));
        }

        public static bool SetLowestMineLevel_SkipToSkullCavern_Prefix(NetWorldState __instance, int value)
        {
            try
            {
                var currentLowestLevel = __instance.lowestMineLevel.Value;
                if (currentLowestLevel < 120 && value > 120)
                {
                    if (_hasSkullElevator)
                    {
                        __instance.lowestMineLevel.Set(Math.Max(currentLowestLevel, 5));
                    }
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
