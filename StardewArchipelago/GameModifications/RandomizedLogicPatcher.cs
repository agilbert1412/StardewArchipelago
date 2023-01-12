using HarmonyLib;
using StardewArchipelago.GameModifications.CodeInjections;
using StardewArchipelago.Locations;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Network;

namespace StardewArchipelago.GameModifications
{
    public class RandomizedLogicPatcher
    {
        private readonly Harmony _harmony;

        public RandomizedLogicPatcher(IMonitor monitor, Harmony harmony, LocationChecker locationChecker)
        {
            _harmony = harmony;
            MineshaftLogicInjections.Initialize(monitor);
            CommunityCenterLogicInjections.Initialize(monitor, locationChecker);
        }

        public void PatchAllGameLogic()
        {
            PatchMineMaxFloorReached();
            PatchDefinitionOfCommunityCenterComplete();
        }

        private void PatchMineMaxFloorReached()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(NetWorldState), typeof(NetWorldState).GetProperty(nameof(NetWorldState.LowestMineLevel)).GetSetMethod().Name),
                prefix: new HarmonyMethod(typeof(MineshaftLogicInjections), nameof(MineshaftLogicInjections.SetLowestMineLevel_SkipToSkullCavern_Prefix))
            );
        }

        private void PatchDefinitionOfCommunityCenterComplete()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.hasCompletedCommunityCenter)),
                prefix: new HarmonyMethod(typeof(CommunityCenterLogicInjections), nameof(CommunityCenterLogicInjections.HasCompletedCommunityCenter_CheckGameStateInsteadOfLetters_Prefix))
            );
        }
    }
}
