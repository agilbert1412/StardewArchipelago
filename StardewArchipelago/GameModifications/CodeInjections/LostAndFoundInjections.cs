using System;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.SlotData;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;
using StardewArchipelago.Items.Mail;
using StardewValley;
using StardewValley.GameData.HomeRenovations;

namespace StardewArchipelago.GameModifications.CodeInjections
{
    public static class LostAndFoundInjections
    {
        private static ILogger _logger;
        private static StardewArchipelagoClient _archipelago;

        public static void Initialize(ILogger logger, StardewArchipelagoClient archipelago)
        {
            _logger = logger;
            _archipelago = archipelago;
        }

        // public void CheckReturnedDonations()
        public static bool CheckReturnedDonations_UpgradeToolsProperly_Prefix(FarmerTeam __instance)
        {
            try
            {
                foreach (var lostAndFoundItem in __instance.returnedDonations.ToArray())
                {
                    if (lostAndFoundItem is not Tool lostAndFoundTool)
                    {
                        continue;
                    }


                    lostAndFoundTool.UpgradeLevel = 0;
                    var baseName = lostAndFoundTool.Name;
                    var apName = $"Progressive {baseName}";
                    if (AnyIncomingLetterContainingKey(apName))
                    {
                        __instance.returnedDonations.Remove(lostAndFoundItem);
                        continue;
                    }

                    var receivedUpgrades = _archipelago.GetReceivedItemCount(apName);
                    var startedWithout = _archipelago.SlotData.ToolProgression.HasFlag(ToolProgression.NoStartingTools);

                    if (startedWithout && receivedUpgrades <= 0)
                    {
                        __instance.returnedDonations.Remove(lostAndFoundItem);
                        continue;
                    }

                    lostAndFoundTool.UpgradeLevel = receivedUpgrades;

                    if (startedWithout)
                    {
                        lostAndFoundTool.UpgradeLevel--;
                    }
                }

                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CheckReturnedDonations_UpgradeToolsProperly_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public static void fixProblems()
        public static void FixProblems_DontLostAndFoundUnreceivedTools_Postfix()
        {
            try
            {
                var team = Game1.player.team;
                foreach (var lostAndFoundItem in team.returnedDonations.ToArray())
                {
                    if (lostAndFoundItem is not Tool lostAndFoundTool)
                    {
                        continue;
                    }

                    var baseName = lostAndFoundTool.Name;
                    var apName = $"Progressive {baseName}";
                    if (AnyIncomingLetterContainingKey(apName))
                    {
                        team.returnedDonations.Remove(lostAndFoundItem);
                        continue;
                    }

                    var receivedUpgrades = _archipelago.GetReceivedItemCount(apName);
                    var startedWithout = _archipelago.SlotData.ToolProgression.HasFlag(ToolProgression.NoStartingTools);
                    if (startedWithout && receivedUpgrades <= 0)
                    {
                        team.returnedDonations.Remove(lostAndFoundItem);
                    }
                }

                if (!team.returnedDonations.Any())
                {
                    team.newLostAndFoundItems.Value = false;
                }

                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(FixProblems_DontLostAndFoundUnreceivedTools_Postfix)}:\n{ex}");
                return;
            }
        }

        public static bool AnyIncomingLetterContainingKey(string apItemName)
        {
            foreach (var mail in Game1.player.mailbox.Union(Game1.player.mailForTomorrow))
            {
                if (!mail.Contains(MailKey.GetBeginningOfKeyForItem(apItemName)))
                {
                    continue;
                }

                return true;
            }

            return false;
        }
    }
}
