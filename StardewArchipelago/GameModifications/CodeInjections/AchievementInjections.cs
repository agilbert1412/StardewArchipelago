using System;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;
using StardewArchipelago.Extensions;
using StardewValley;

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

                if (moneyEarnedIfProfitMarginWas1 >= legendAmount)
                {
                    Game1.getAchievement((int)MoneyAchievement.Legend);
                }

                if (moneyEarnedIfProfitMarginWas1 >= millionaireAmount)
                {
                    Game1.getAchievement((int)MoneyAchievement.Millionaire);
                }

                if (moneyEarnedIfProfitMarginWas1 >= homesteaderAmount)
                {
                    Game1.getAchievement((int)MoneyAchievement.Homesteader);
                }

                if (moneyEarnedIfProfitMarginWas1 >= cowpokeAmount)
                {
                    Game1.getAchievement((int)MoneyAchievement.Cowpoke);
                }

                if (moneyEarnedIfProfitMarginWas1 >= greenhornAmount)
                {
                    Game1.getAchievement((int)MoneyAchievement.Greenhorn);
                }

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CheckForMoneyAchievements_GrantMoneyAchievementsFairly_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
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
