using System;
using StardewArchipelago.Archipelago;
using StardewValley;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.Utilities.Interfaces;
using StardewArchipelago.Locations;
using StardewValley.GameData;

namespace StardewArchipelago.GameModifications.CodeInjections
{
    internal class EventInjections
    {
        private static ILogger _logger;
        private static StardewArchipelagoClient _archipelago;
        private static StardewLocationChecker _locationChecker;

        public static void Initialize(ILogger logger, StardewArchipelagoClient archipelago, StardewLocationChecker locationChecker)
        {
            _logger = logger;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
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

                //Game1.addMailForTomorrow("leoMoved", true, true);
                //Game1.player.team.requestLeoMove.Fire();
                __instance.exitEvent();

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(EndBehaviors_LeoMoving_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public static void AddMailReceived(Event @event, string[] args, EventContext context)
        public static bool AddMailReceived_BlockSomeSpecificLetters_Prefix(Event @event, string[] args, EventContext context)
        {
            try
            {
                if (!ArgUtility.TryGet(args, 1, out var str, out var error, name: "string mailId") || !ArgUtility.TryGetOptionalBool(args, 2, out var add, out error, true, "bool add"))
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (str.Equals("ccDoorUnlock"))
                {
                    _locationChecker.AddCheckedLocation("Rat Problem Cutscene");
                    ++@event.CurrentCommand;
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(AddMailReceived_BlockSomeSpecificLetters_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }
    }
}