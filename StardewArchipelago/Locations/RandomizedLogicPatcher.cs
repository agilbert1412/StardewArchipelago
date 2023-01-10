using HarmonyLib;
using StardewArchipelago.Locations.CodeInjections;
using StardewModdingAPI;
using StardewValley.Network;

namespace StardewArchipelago.Locations
{
    public class RandomizedLogicPatcher
    {
        private readonly Harmony _harmony;

        public RandomizedLogicPatcher(IMonitor monitor, Harmony harmony)
        {
            _harmony = harmony;
            MineshaftLogicInjections.Initialize(monitor);
        }

        public void PatchAllGameLogic()
        {
            PatchMineMaxFloorReached();
        }

        private void PatchMineMaxFloorReached()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(NetWorldState), typeof(NetWorldState).GetProperty(nameof(NetWorldState.LowestMineLevel)).GetSetMethod().Name),
                prefix: new HarmonyMethod(typeof(MineshaftLogicInjections), nameof(MineshaftLogicInjections.SetLowestMineLevel_SkipToSkullCavern_Prefix))
            );
        }
    }
}
