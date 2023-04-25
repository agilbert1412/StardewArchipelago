using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Locations.CodeInjections;
using StardewArchipelago.Locations.GingerIsland.Boat;
using StardewArchipelago.Locations.GingerIsland.Parrots;
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
            ReplaceParrotsWithChecks();
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

        private void ReplaceParrotsWithChecks()
        {
            _harmony.Patch(
                original: AccessTools.Constructor(typeof(IslandSouth), new[]{typeof(string), typeof(string)}),
                prefix: new HarmonyMethod(typeof(IslandSouthInjections), nameof(IslandSouthInjections.Constructor_ReplaceParrots_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.Constructor(typeof(IslandHut), new[] { typeof(string), typeof(string) }),
                prefix: new HarmonyMethod(typeof(IslandHutInjections), nameof(IslandHutInjections.Constructor_ReplaceParrots_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.Constructor(typeof(IslandNorth), new[] { typeof(string), typeof(string) }),
                prefix: new HarmonyMethod(typeof(IslandNorthInjections), nameof(IslandNorthInjections.Constructor_ReplaceParrots_Postfix))
            );
        }
    }
}
