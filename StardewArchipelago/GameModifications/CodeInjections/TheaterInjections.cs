using System;
using Netcode;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Items.Unlocks;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Events;
using StardewValley.Locations;
using xTile.ObjectModel;
using xTile.Tiles;

namespace StardewArchipelago.GameModifications.CodeInjections
{
    public class TheaterInjections
    {
        private const string MOVIE_THEATER_ITEM = "Progressive Movie Theater";
        private const string MOVIE_THEATER_MAIL = "ccMovieTheater";
        private const int CC_EVENT_ID = 191393;
        private static IMonitor _monitor;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;

        public static void Initialize(IMonitor monitor, IModHelper modHelper, ArchipelagoClient archipelago)
        {
            _monitor = monitor;
            _modHelper = modHelper;
            _archipelago = archipelago;
        }

        // public static FarmEvent pickFarmEvent()
        public static void PickFarmEvent_BreakJojaDoor_Postfix(ref FarmEvent __result)
        {
            try
            {
                if (Game1.weddingToday || __result != null || !_archipelago.HasReceivedItem(MOVIE_THEATER_ITEM))
                {
                    return;
                }

                if ((Game1.isRaining || Game1.isLightning || Game1.IsWinter) && !Game1.MasterPlayer.mailReceived.Contains("abandonedJojaMartAccessible"))
                {
                    __result = new WorldChangeEvent(12);
                    return;
                }

                if (_archipelago.GetReceivedItemCount(MOVIE_THEATER_ITEM) < 2)
                {
                    return;
                }

                if (!Game1.player.mailReceived.Contains("ccMovieTheater%&NL&%") && !Game1.player.mailReceived.Contains("ccMovieTheater"))
                {
                    __result = new WorldChangeEvent(11);
                    Game1.player.mailReceived.Add("ccMovieTheater");
                    return;
                }

                return;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(PickFarmEvent_BreakJojaDoor_Postfix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }

        private static bool hasSeenCCCeremonyCutscene;
        private static bool hasPamHouseUpgrade;
        private static bool hasShortcuts;

        // public override void MakeMapModifications(bool force = false)
        public static bool MakeMapModifications_JojamartAndTheater_Prefix(Town __instance, bool force)
        {
            try
            {
                if (_archipelago.GetReceivedItemCount(MOVIE_THEATER_ITEM) >= 2)
                {
                    var rectangle = new Microsoft.Xna.Framework.Rectangle(84, 41, 27, 15);
                    __instance.ApplyMapOverride("Town-Theater", rectangle, rectangle);
                    return true; // run original logic
                }

                if (_archipelago.GetReceivedItemCount(MOVIE_THEATER_ITEM) >= 1)
                {
                    // private void showDestroyedJoja()
                    var showDestroyedJojaMethod = _modHelper.Reflection.GetMethod(__instance, "showDestroyedJoja");
                    showDestroyedJojaMethod.Invoke();
                    if (Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("abandonedJojaMartAccessible"))
                    {
                        __instance.crackOpenAbandonedJojaMartDoor();
                    }
                }
                else
                {
                    hasSeenCCCeremonyCutscene = Utility.HasAnyPlayerSeenEvent(CC_EVENT_ID);
                    if (hasSeenCCCeremonyCutscene)
                    {
                        Game1.player.eventsSeen.Remove(CC_EVENT_ID);
                    }
                }

                return true; // run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(MakeMapModifications_JojamartAndTheater_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        // public override void MakeMapModifications(bool force = false)
        public static void MakeMapModifications_JojamartAndTheater_Postfix(Town __instance, bool force)
        {
            try
            {
                if (hasSeenCCCeremonyCutscene && !Game1.player­.eventsSeen.Contains(CC_EVENT_ID))
                {
                    Game1.player.eventsSeen.Add(CC_EVENT_ID);
                }

                return;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(MakeMapModifications_JojamartAndTheater_Postfix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }
    }
}
