using StardewArchipelago.Archipelago;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Locations;
using StardewValley.Tools;
using xTile.Dimensions;

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

        private static int _previousScore = 0;
        public static bool CheckAction_GrandpaReevaluation_PreFix(Farm __instance, Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who, ref bool __result)
        {
            try
            {
                Game1.year += 2;
                _previousScore = __instance.grandpaScore.Value;
                __instance.grandpaScore.Value = 1;
                return true; // run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(DoAreaCompleteReward_CommunityCenterGoal_PostFix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        public static void CheckAction_GrandpaReevaluation_PostFix(Farm __instance, Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who, ref bool __result)
        {
            try
            {
                Game1.year -= 2;
                __instance.grandpaScore.Value = _previousScore;
                return;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(DoAreaCompleteReward_CommunityCenterGoal_PostFix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }

        public static bool SkipEvent_GrandpaReevaluation_PreFix(Event __instance)
        {
            try
            {
                if (__instance.id != 558292)
                {
                    return true; // run original logic
                }

                CheckGrandpaEvaluationGoalCompletion();
                return true; // run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(DoAreaCompleteReward_CommunityCenterGoal_PostFix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        public static void CommandGrandpaCandles_GrandpaGoal_PostFix(Event __instance, GameLocation location, GameTime time, string[] split)
        {
            try
            {
                CheckGrandpaEvaluationGoalCompletion();
                return;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(DoAreaCompleteReward_CommunityCenterGoal_PostFix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }

        public static void CommandEvaluation2_GrandpaGoal_PostFix(Event __instance, GameLocation location, GameTime time, string[] split)
        {
            try
            {
                CheckGrandpaEvaluationGoalCompletion();
                return;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(DoAreaCompleteReward_CommunityCenterGoal_PostFix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }
    }
}
