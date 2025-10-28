using System;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.Utilities.Interfaces;
using StardewArchipelago.Constants;
using StardewValley;
using StardewValley.Locations;

namespace StardewArchipelago.GameModifications.CodeInjections
{
    public class ForestInjections
    {
        private static ILogger _logger;
        private static ArchipelagoClient _archipelago;

        public static void Initialize(ILogger logger, ArchipelagoClient archipelago)
        {
            _logger = logger;
            _archipelago = archipelago;
        }

        public static bool IsWizardHouseUnlocked_UnlockAtWizardInvitation_Prefix(Forest __instance, ref bool __result)
        {
            try
            {
                __result = _archipelago.HasReceivedItem("Wizard Invitation");
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(IsWizardHouseUnlocked_UnlockAtWizardInvitation_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }
    }
}
