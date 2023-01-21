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
                    return;
                case Goal.BottomOfMines:
                    GoalCodeInjection.CheckBottomOfTheMinesGoalCompletion();
                    return;
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
                    return;
                case Goal.BottomOfMines:
                    InjectBottomOfTheMinesGoalMethods();
                    return;
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

        private void InjectBottomOfTheMinesGoalMethods()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(Game1), nameof(Game1.enterMine)),
                postfix: new HarmonyMethod(typeof(GoalCodeInjection), nameof(GoalCodeInjection.EnterMine_Level120Goal_PostFix))
            );
        }
    }
}
