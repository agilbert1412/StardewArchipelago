using System;
using System.Collections.Generic;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.Utilities.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.Items.Mail;
using StardewArchipelago.Locations.InGameLocations;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Delegates;
using StardewValley.Internal;
using StardewValley.Objects;
using StardewValley.SpecialOrders;
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
                    var startedWithout = _archipelago.SlotData.StartWithout.HasFlag(StartWithout.Tools);

                    if (startedWithout && receivedUpgrades <= 0)
                    {
                        __instance.returnedDonations.Remove(lostAndFoundItem);
                        continue;
                    }

                    if (Game1.player.Items.ContainsId(lostAndFoundItem.QualifiedItemId))
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
                DontLostAndFoundUnreceivedTools();
                CorrectlyUpgradeOwnedTools();
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(FixProblems_DontLostAndFoundUnreceivedTools_Postfix)}:\n{ex}");
                return;
            }
        }

        private static void DontLostAndFoundUnreceivedTools()
        {
            var team = Game1.player.team;
            foreach (var lostAndFoundItem in team.returnedDonations.ToArray())
            {
                if (lostAndFoundItem is not Tool lostAndFoundTool)
                {
                    continue;
                }

                var baseName = GetToolBaseName(lostAndFoundTool);
                var apName = $"Progressive {baseName}";
                if (AnyIncomingLetterContainingKey(apName))
                {
                    team.returnedDonations.Remove(lostAndFoundItem);
                    continue;
                }

                var receivedUpgrades = _archipelago.GetReceivedItemCount(apName);
                var startedWithout = _archipelago.SlotData.StartWithout.HasFlag(StartWithout.Tools);
                if (startedWithout && receivedUpgrades <= 0)
                {
                    team.returnedDonations.Remove(lostAndFoundItem);
                    continue;
                }

                continue;
            }

            if (!team.returnedDonations.Any())
            {
                team.newLostAndFoundItems.Value = false;
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
            var startedWithout = _archipelago.SlotData.StartWithout.HasFlag(StartWithout.Tools) && tool is not Pan;
            if (receivedUpgrades <= 0 || startedWithout && receivedUpgrades <= 1)
            {
                return true;
            }

            var expectedLevel = startedWithout ? receivedUpgrades - 1 : receivedUpgrades;
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
