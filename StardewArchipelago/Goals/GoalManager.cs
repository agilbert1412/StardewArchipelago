using System;
using HarmonyLib;
using StardewArchipelago.Archipelago;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;

namespace StardewArchipelago.Goals
{
    public class GoalManager
    {
        private static IMonitor _monitor;
        private IModHelper _modHelper;
        private Harmony _harmony;
        private ArchipelagoClient _archipelago;
        private GoalCodeInjection _codeInjection;

        public GoalManager(IMonitor monitor, IModHelper modHelper, Harmony harmony, ArchipelagoClient archipelago)
        {
            _monitor = monitor;
            _modHelper = modHelper;
            _harmony = harmony;
            _archipelago = archipelago;
            _codeInjection = new GoalCodeInjection(_monitor, _modHelper, _archipelago);
        }

        public void CheckGoalCompletion()
        {
            switch (_archipelago.SlotData.Goal)
            {
                case Goal.CommunityCenter:
                    GoalCodeInjection.CheckCommunityCenterGoalCompletion();
                    return;
                case Goal.GrandpaEvaluation:
                    GoalCodeInjection.CheckGrandpaEvaluationGoalCompletion();
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Goal [{_archipelago.SlotData.Goal}] is not supported in this version of the mod.");
            }
        }

        public void InjectGoalMethods()
        {
            switch (_archipelago.SlotData.Goal)
            {
                case Goal.CommunityCenter:
                    InjectCommunityCenterGoalMethods();
                    return;
                case Goal.GrandpaEvaluation:
                    InjectGrandpaEvaluationGoalMethods();
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Goal [{_archipelago.SlotData.Goal}] is not supported in this version of the mod.");
            }
        }

        private void InjectCommunityCenterGoalMethods()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(CommunityCenter), "doAreaCompleteReward"),
                postfix: new HarmonyMethod(typeof(GoalCodeInjection), nameof(GoalCodeInjection.DoAreaCompleteReward_CommunityCenterGoal_PostFix))
            );
        }

        private void InjectGrandpaEvaluationGoalMethods()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(Farm), nameof(Farm.checkAction)),
                prefix: new HarmonyMethod(typeof(GoalCodeInjection), nameof(GoalCodeInjection.CheckAction_GrandpaReevaluation_PreFix)),
                postfix: new HarmonyMethod(typeof(GoalCodeInjection), nameof(GoalCodeInjection.CheckAction_GrandpaReevaluation_PostFix))
            );
            _harmony.Patch(
                original: AccessTools.Method(typeof(Event), nameof(Event.skipEvent)),
                prefix: new HarmonyMethod(typeof(GoalCodeInjection), nameof(GoalCodeInjection.SkipEvent_GrandpaReevaluation_PreFix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Event), nameof(Event.command_grandpaCandles)),
                postfix: new HarmonyMethod(typeof(GoalCodeInjection), nameof(GoalCodeInjection.CommandGrandpaCandles_GrandpaGoal_PostFix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Event), nameof(Event.command_grandpaEvaluation2)),
                postfix: new HarmonyMethod(typeof(GoalCodeInjection), nameof(GoalCodeInjection.CommandEvaluation2_GrandpaGoal_PostFix))
            );
        }
    }
}
