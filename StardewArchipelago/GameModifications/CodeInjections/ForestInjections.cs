using System;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewValley;
using StardewValley.Locations;

namespace StardewArchipelago.GameModifications.CodeInjections
{
    public class ForestInjections
    {
        private const string RAT_PROBLEM_ID = "26";

        private static ILogger _logger;
        private static ArchipelagoClient _archipelago;

        public static void Initialize(ILogger logger, ArchipelagoClient archipelago)
        {
            _logger = logger;
            _archipelago = archipelago;
        }

        public static bool IsWizardHouseUnlocked_UnlockAtRatProblem_Prefix(Forest __instance, ref bool __result)
        {
            try
            {
                if (!Game1.player.hasQuest(RAT_PROBLEM_ID))
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                __result = true;
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(IsWizardHouseUnlocked_UnlockAtRatProblem_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }
    }
}
