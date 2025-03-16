using System;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago;
using StardewValley;
using StardewArchipelago.Constants.Vanilla;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using Object = StardewValley.Object;
using StardewValley.GameData;

namespace StardewArchipelago.GameModifications.CodeInjections
{
    internal class EventInjections
    {
        private static ILogger _logger;
        private static StardewArchipelagoClient _archipelago;

        public static void Initialize(ILogger logger, StardewArchipelagoClient archipelago)
        {
            _logger = logger;
            _archipelago = archipelago;
        }

        // public void endBehaviors(string[] args, GameLocation location)
        public static bool EndBehaviors_LeoMoving_Prefix(Event __instance, string[] args, GameLocation location)
        {
            try
            {
                var str = ArgUtility.Get(args, 1);
                if (str != "Leo" || __instance.isMemory)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (Game1.getMusicTrackName().Contains(Game1.currentSeason) && ArgUtility.Get(__instance.eventCommands, 0) != "continue")
                {
                    Game1.stopMusicTrack(MusicContext.Default);
                }

                Game1.addMailForTomorrow("leoMoved", true, true);
                Game1.player.team.requestLeoMove.Fire();
                __instance.exitEvent();

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(EndBehaviors_LeoMoving_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }
    }
}