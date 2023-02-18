using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Serialization;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;

namespace StardewArchipelago.GameModifications.Seasons
{
    public class SeasonsRandomizer
    {
        private const string _nextSeasonDialogKey = "NextSeason";
        public static readonly string[] ValidSeasons = new[] { "Spring", "Summer", "Fall", "Winter" };

        private static IMonitor _monitor;
        private static IModHelper _helper;
        private static ArchipelagoClient _archipelago;
        private static ArchipelagoStateDto _state;

        public SeasonsRandomizer(IMonitor monitor, IModHelper helper, ArchipelagoClient archipelago, ArchipelagoStateDto state)
        {
            _monitor = monitor;
            _helper = helper;
            _archipelago = archipelago;
            _state = state;
        }
        
        public string GetFirstSeason()
        {
            var firstReceivedSeason = _archipelago.GetAllReceivedItems().FirstOrDefault(x => ValidSeasons.Contains(x.ItemName.ToLower()));
            return firstReceivedSeason?.ItemName.ToLower() ?? ValidSeasons[1];
        }

        public static List<string> GetUnlockedSeasons()
        {
            var receivedSeasons = _archipelago.GetAllReceivedItems().Select(x => x.ItemName).Where(x => ValidSeasons.Contains(x.ToLower())).ToList();
            return receivedSeasons.Any() ? receivedSeasons : ValidSeasons.ToList();
        }

        public static void SetNextSeason(string season)
        {
            var currentSeasonNumber = (int)Game1.stats.DaysPlayed / 28;
            if (_state.SeasonsOrder.Count <= currentSeasonNumber)
            {
                _state.SeasonsOrder.Add(season);
            }
            else
            {
                _state.SeasonsOrder[currentSeasonNumber] = season;
            }
        }

        public static void SetSeason(string season)
        {
            Game1.currentSeason = season.ToLower();
            Game1.setGraphicsForSeason();
            Utility.ForAllLocations(l => l.seasonUpdate(Game1.GetSeasonForLocation(l)));
        }

        public static bool TotalDays_UseStats_Prefix(WorldDate __instance, ref int __result)
        {
            try
            {
                __result = (int)Game1.stats.DaysPlayed;
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(TotalDays_UseStats_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }


        public static bool NewDay_SeasonChoice_Prefix(float timeToPause)
        {
            try
            {
                if (Game1.dayOfMonth != 28)
                {
                    return true; // run original logic
                }

                var possibleResponses = new List<Response>();

                foreach (var season in GetUnlockedSeasons())
                {
                    possibleResponses.Add(new Response(season, season).SetHotKey(Keys.None));
                }
                
                Game1.currentLocation.createQuestionDialogue($"{Game1.CurrentSeasonDisplayName} has come to an end. What season is next?", possibleResponses.ToArray(), _nextSeasonDialogKey, null);
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(NewDay_SeasonChoice_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        public static bool AnswerDialogueAction_SeasonChoice_Prefix(GameLocation __instance, string questionAndAnswer, string[] questionParams, ref bool __result)
        {
            try
            {
                if (!questionAndAnswer.StartsWith(_nextSeasonDialogKey))
                {
                    return true; // run original logic
                }

                var parts = questionAndAnswer.Split("_");
                var chosenSeason = parts[1];
                SetNextSeason(chosenSeason);

                __result = true;
                NewDayOriginal(0);
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(AnswerDialogueAction_SeasonChoice_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        private static void NewDayOriginal(float timeToPause)
        {
            Game1.currentMinigame = null;
            Game1.newDay = true;
            Game1.newDaySync = new NewDaySynchronizer();
            if (Game1.player.isInBed.Value || Game1.player.passedOut)
            {
                Game1.nonWarpFade = true;
                var screenFadeField = _helper.Reflection.GetField<ScreenFade>(typeof(Game1), "screenFade");
                screenFadeField.GetValue().FadeScreenToBlack(Game1.player.passedOut ? 1.1f : 0.0f);
                Game1.player.Halt();
                Game1.player.currentEyes = 1;
                Game1.player.blinkTimer = -4000;
                Game1.player.CanMove = false;
                Game1.player.passedOut = false;
                Game1.pauseTime = timeToPause;
            }
            if (Game1.activeClickableMenu == null || Game1.dialogueUp)
                return;
            Game1.activeClickableMenu.emergencyShutDown();
            Game1.exitActiveMenu();
        }

        public static void SendMailHardcodedForToday()
        {
            GetVanillaValues(out var totalDays, out var year, out var seasonNumber, out var seasonName);
            var mailData = Game1.content.Load<Dictionary<string, string>>("Data\\mail");
            for (var i = 1; i <= year + 1; i++)
            {
                var key = seasonName + "_" + Game1.dayOfMonth + "_" + i;
                if (!mailData.ContainsKey(key) || Game1.player.hasOrWillReceiveMail(key))
                {
                    continue;
                }

                Game1.mailbox.Add(key);
            }

            var keyAnyYear = seasonName + "_" + Game1.dayOfMonth;
            if (!mailData.ContainsKey(keyAnyYear) || Game1.player.hasOrWillReceiveMail(keyAnyYear))
            {
                return;
            }

            Game1.mailbox.Add(keyAnyYear);
        }

        public static void PrepareDateForSaveGame()
        {
            GetVanillaValues(out var totalDays, out var year, out var seasonNumber, out _);
            Game1.year = year + 1;
            Game1.player.dayOfMonthForSaveGame = Game1.dayOfMonth;
            Game1.player.seasonForSaveGame = seasonNumber;
            Game1.player.yearForSaveGame = Game1.year;
        }

        private static void GetVanillaValues(out int totalDays, out int year, out int seasonNumber, out string seasonName)
        {
            totalDays = (int)Game1.stats.DaysPlayed;
            year = totalDays / 112;
            var daysThisYear = totalDays - (year * 112);
            seasonNumber = daysThisYear / 28;
            seasonName = SeasonsRandomizer.ValidSeasons[seasonNumber];
        }
    }
}
