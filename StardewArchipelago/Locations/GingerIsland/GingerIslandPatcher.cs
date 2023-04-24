using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Locations.CodeInjections;
using StardewArchipelago.Locations.GingerIsland.Boat;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewValley.Locations;

namespace StardewArchipelago.Locations.GingerIsland
{
    public class GingerIslandPatcher
    {
        private readonly ArchipelagoClient _archipelago;
        private readonly Harmony _harmony;

        public GingerIslandPatcher(IMonitor monitor, IModHelper modHelper, Harmony harmony, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _archipelago = archipelago;
            _harmony = harmony;
            GingerIslandInitializer.Initialize(monitor, modHelper, _archipelago, locationChecker);
        }

        public void PatchGingerIslandLocations()
        {
            if (_archipelago.SlotData.ExcludeGingerIsland)
            {
                return;
            }

            ReplaceBoatRepairWithChecks();
        }

        private void ReplaceBoatRepairWithChecks()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(BoatTunnel), nameof(BoatTunnel.checkAction)),
                prefix: new HarmonyMethod(typeof(BoatTunnelInjections), nameof(BoatTunnelInjections.CheckAction_BoatRepairAndUsage_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(BoatTunnel), nameof(BoatTunnel.answerDialogue)),
                postfix: new HarmonyMethod(typeof(BoatTunnelInjections), nameof(BoatTunnelInjections.AnswerDialogue_BoatRepairAndUsage_Prefix))
            );
        }
    }
}
