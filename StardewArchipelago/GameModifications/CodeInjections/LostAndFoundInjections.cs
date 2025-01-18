using System;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewValley;

namespace StardewArchipelago.GameModifications.CodeInjections
{
    public static class LostAndFoundInjections
    {
        private static ILogger _logger;
        private static ArchipelagoClient _archipelago;

        public static void Initialize(ILogger logger, ArchipelagoClient archipelago)
        {
            _logger = logger;
            _archipelago = archipelago;
        }

        // public void CheckReturnedDonations()
        public static bool CheckReturnedDonations_UpgradeToolsProperly_Prefix(FarmerTeam __instance)
        {
            try
            {
                foreach (var lostAndFoundItem in __instance.returnedDonations)
                {
                    if (lostAndFoundItem is not Tool lostAndFoundTool)
                    {
                        continue;
                    }

                    lostAndFoundTool.UpgradeLevel = 0;
                    var baseName = lostAndFoundTool.Name;
                    var apName = $"Progressive {baseName}";
                    var receivedUpgrades = _archipelago.GetReceivedItemCount(apName);
                    lostAndFoundTool.UpgradeLevel = receivedUpgrades;
                }

                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CheckReturnedDonations_UpgradeToolsProperly_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }
    }
}
