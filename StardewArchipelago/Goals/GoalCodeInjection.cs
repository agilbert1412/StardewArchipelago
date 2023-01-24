using StardewArchipelago.Archipelago;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Linq;
using StardewValley.Locations;

namespace StardewArchipelago.Goals
{
    internal class GoalCodeInjection
    {
        private static IMonitor _monitor;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;

        public GoalCodeInjection(IMonitor monitor, IModHelper modHelper, ArchipelagoClient archipelago)
        {
            _monitor = monitor;
            _modHelper = modHelper;
            _archipelago = archipelago;
        }

        public static void CheckCommunityCenterGoalCompletion()
        {
            if (!_archipelago.IsConnected || _archipelago.SlotData.Goal != Goal.CommunityCenter)
            {
                return;
            }

            var communityCenter = Game1.locations.OfType<CommunityCenter>().First();
            if (!communityCenter.areAllAreasComplete())
            {
                return;
            }

            _archipelago.ReportGoalCompletion();
        }

        public static void CheckGrandpaEvaluationGoalCompletion()
        {
            if (!_archipelago.IsConnected || _archipelago.SlotData.Goal != Goal.GrandpaEvaluation)
            {
                return;
            }

            var farm = Game1.getFarm();
            int candlesFromScore = Utility.getGrandpaCandlesFromScore(Utility.getGrandpaScore());
            farm.grandpaScore.Value = candlesFromScore;
            for (int index = 0; index < candlesFromScore; ++index)
            {
                DelayedAction.playSoundAfterDelay("fireball", 100 * index);
            }
            farm.addGrandpaCandles();

            if (farm.grandpaScore.Value < 4)
            {
                return;
            }

            _archipelago.ReportGoalCompletion();
        }

        public static void CheckBottomOfTheMinesGoalCompletion()
        {
            if (!_archipelago.IsConnected || _archipelago.SlotData.Goal != Goal.BottomOfMines)
            {
                return;
            }

            var lowestMineLevel = Game1.netWorldState.Value.LowestMineLevel;

            if (lowestMineLevel < 120)
            {
                return;
            }

            _archipelago.ReportGoalCompletion();
        }

        public static void DoAreaCompleteReward_CommunityCenterGoal_PostFix(CommunityCenter __instance, int whichArea)
        {
            try
            {
                CheckCommunityCenterGoalCompletion();
                return;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(DoAreaCompleteReward_CommunityCenterGoal_PostFix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }

        public static void EnterMine_Level120Goal_PostFix(int whatLevel)
        {
            try
            {
                if (whatLevel != 120)
                {
                    return;
                }

                _archipelago.ReportGoalCompletion();
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(EnterMine_Level120Goal_PostFix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }
    }
}
