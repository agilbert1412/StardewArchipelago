using System;
using System.Collections.Generic;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.Utilities.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;
using StardewArchipelago.Extensions;
using StardewValley;
using StardewValley.TokenizableStrings;

namespace StardewArchipelago.GameModifications.CodeInjections
{
    public static class AchievementInjections
    {
        private static ILogger _logger;
        private static StardewArchipelagoClient _archipelago;

        public static void Initialize(ILogger logger, StardewArchipelagoClient archipelago)
        {
            _logger = logger;
            _archipelago = archipelago;
        }

        public static bool GetSteamAchievement_DisableUndeservedAchievements_Prefix(string which)
        {
            try
            {
                if (_archipelago.SlotData.ArcadeMachineLocations == ArcadeLocations.FullShuffling || _archipelago.SlotData.ArcadeMachineLocations == ArcadeLocations.VictoriesEasy)
                {
                    var jotpkVictory = "Achievement_PrairieKing";
                    var fector = "Achievement_FectorsChallenge";
                    if (which == jotpkVictory || which == fector)
                    {
                        return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                    }
                }

                if (_archipelago.SlotData.ElevatorProgression != ElevatorProgression.Vanilla)
                {
                    var bottom_of_the_mine = "Achievement_TheBottom";
                    if (which == bottom_of_the_mine)
                    {
                        return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                    }
                }

                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(GetSteamAchievement_DisableUndeservedAchievements_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public void checkForMoneyAchievements()
        public static bool CheckForMoneyAchievements_GrantMoneyAchievementsFairly_Prefix(Stats __instance)
        {
            try
            {
                if (_archipelago.SlotData.StartingMoney.IsUnlimited())
                {
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                var totalMoneyEarned = Game1.player.totalMoneyEarned;
                var moneyEarnedAfterStartingMoney = totalMoneyEarned - _archipelago.SlotData.StartingMoney;
                var moneyEarnedIfProfitMarginWas1 = moneyEarnedAfterStartingMoney / _archipelago.SlotData.ProfitMargin;

                const uint greenhornAmount = 15000U;
                const uint cowpokeAmount = 50000U;
                const uint homesteaderAmount = 250000U;
                const uint millionaireAmount = 1000000U;
                const uint legendAmount = 10000000U;
                var achievementsByAmount = new Dictionary<MoneyAchievement, uint>()
                {
                    { MoneyAchievement.Legend, legendAmount },
                    { MoneyAchievement.Millionaire, millionaireAmount },
                    { MoneyAchievement.Homesteader, homesteaderAmount },
                    { MoneyAchievement.Cowpoke, cowpokeAmount },
                    { MoneyAchievement.Greenhorn, greenhornAmount },
                };

                foreach (var (achievement, amountNeeded) in achievementsByAmount)
                {
                    if (moneyEarnedIfProfitMarginWas1 >= amountNeeded)
                    {
                        GrantSteamAchievement((int)achievement);
                    }
                    if (moneyEarnedAfterStartingMoney >= amountNeeded)
                    {
                        GrantAchievementInGame((int)achievement);
                    }
                }

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CheckForMoneyAchievements_GrantMoneyAchievementsFairly_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public static void getAchievement(int which, bool allowBroadcasting = true)
        private static void GrantAchievement(int which, bool allowBroadcasting = true)
        {
            GrantAchievementInGame(which, allowBroadcasting);
            GrantSteamAchievement(which);
        }

        private static void GrantAchievementInGame(int which, bool allowBroadcasting = true)
        {
            if (Game1.player.achievements.Contains(which) || Game1.gameMode != (byte)3 || !Game1.achievements.TryGetValue(which, out var str1))
            {
                return;
            }
            var achievementName = str1.Split('^')[0];
            Game1.player.achievements.Add(which);
            if (which < 32 & allowBroadcasting)
            {
                if (Game1.stats.isSharedAchievement(which))
                {
                    Game1.Multiplayer.sendSharedAchievementMessage(which);
                }
                else
                {
                    var str2 = Game1.player.Name;
                    if (str2 == "")
                    {
                        str2 = TokenStringBuilder.LocalizedText("Strings\\UI:Chat_PlayerJoinedNewName");
                    }
                    Game1.Multiplayer.globalChatInfoMessage("Achievement", str2, TokenStringBuilder.AchievementName(which));
                }
            }
            Game1.playSound("achievement");
            Game1.addHUDMessage(HUDMessage.ForAchievement(achievementName));
            Game1.player.autoGenerateActiveDialogueEvent("achievement_" + which.ToString());
            if (Game1.player.hasOrWillReceiveMail("hatter"))
            {
                return;
            }
            Game1.addMailForTomorrow("hatter");
        }

        private static void GrantSteamAchievement(int which)
        {
            Game1.getPlatformAchievement(which.ToString());
        }
    }

    public enum MoneyAchievement
    {
        Greenhorn = 0,
        Cowpoke = 1,
        Homesteader = 2,
        Millionaire = 3,
        Legend = 4,
    }
}
