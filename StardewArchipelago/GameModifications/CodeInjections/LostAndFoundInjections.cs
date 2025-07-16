using System;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Items.Mail;
using StardewValley;
using StardewValley.Internal;
using StardewValley.Tools;

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
                    var baseName = GetToolBaseName(lostAndFoundTool);

                    var apName = $"Progressive {baseName}";
                    if (AnyIncomingLetterContainingKey(apName))
                    {
                        __instance.returnedDonations.Remove(lostAndFoundItem);
                        continue;
                    }

                    var receivedUpgrades = _archipelago.GetReceivedItemCount(apName);

                    if (Game1.player.Items.ContainsId(lostAndFoundItem.QualifiedItemId))
                    {
                        __instance.returnedDonations.Remove(lostAndFoundItem);
                        continue;
                    }

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

        // public static void fixProblems()
        public static void FixProblems_DontLostAndFoundUnreceivedTools_Postfix()
        {
            try
            {
                CorrectlyUpgradeOwnedTools();
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(FixProblems_DontLostAndFoundUnreceivedTools_Postfix)}:\n{ex}");
                return;
            }
        }

        private static string GetToolBaseName(Tool tool)
        {
            var baseName = tool.Name;
            var prefixes = new[] { "Copper", "Steel", "Gold", "Iridium" };
            foreach (var prefix in prefixes)
            {
                var startString = $"{prefix} ";
                if (baseName.StartsWith(startString))
                {
                    baseName = baseName.Replace(startString, "");
                }
            }
            return baseName;
        }

        private static void CorrectlyUpgradeOwnedTools()
        {
            Utility.ForEachItemContext(MakeToolCorrectLevel);
        }

        private static bool MakeToolCorrectLevel(in ForEachItemContext context)
        {
            var item = context.Item;
            if (item is not Tool tool || item is FishingRod || item is MeleeWeapon || item is Slingshot)
            {
                return true;
            }

            var baseName = GetToolBaseName(tool);
            var apName = $"Progressive {baseName}";
            if (AnyIncomingLetterContainingKey(apName))
            {
                return true;
            }

            var receivedUpgrades = _archipelago.GetReceivedItemCount(apName);
            if (receivedUpgrades <= 0)
            {
                return true;
            }

            var expectedLevel = receivedUpgrades;
            var newTool = CreateNewTool(tool, expectedLevel);
            if (tool.QualifiedItemId == newTool.QualifiedItemId)
            {
                tool.UpgradeLevel = newTool.UpgradeLevel;
                DowngradeToolIfRequested(tool);
                return true;
            }

            DowngradeToolIfRequested(newTool);
            context.ReplaceItemWith(newTool);
            return true;
        }

        private static void DowngradeToolIfRequested(Tool tool)
        {
            if (!ModEntry.Instance.Config.LimitHoeWateringCanLevel)
            {
                return;
            }

            if (tool is Hoe || tool is WateringCan)
            {
                tool.UpgradeLevel = 0;
            }
        }

        private static Tool CreateNewTool(Tool tool, int expectedLevel)
        {
            foreach (var (toolId, toolData) in Game1.toolData)
            {
                if (tool.GetToolData().ClassName != toolData.ClassName)
                {
                    continue;
                }
                if (expectedLevel != toolData.UpgradeLevel)
                {
                    continue;
                }
                var newTool = (Tool)ItemRegistry.Create("(T)" + toolId);
                return newTool;
            }

            return tool;
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
