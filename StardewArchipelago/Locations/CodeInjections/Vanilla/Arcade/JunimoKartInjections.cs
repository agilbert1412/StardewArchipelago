using System;
using System.Collections.Generic;
using System.Linq;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Netcode;
using Newtonsoft.Json.Linq;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Minigames;
using Color = Microsoft.Xna.Framework.Color;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Arcade
{
    public static class JunimoKartInjections
    {
        private const string JK_DATASTORAGE_SCORE = "JunimoKartEndlessScore";
        private static JToken EmptyScoresDictionary => JToken.FromObject(new Dictionary<string, int>());

        private const string JK_EXTRA_LIFE = "Junimo Kart: Extra Life";

        public const string JK_VICTORY = "Junimo Kart: Sunset Speedway (Victory)";
        private static readonly Dictionary<int, string> JK_LEVEL_LOCATIONS = new()
        {
            { 0, "Junimo Kart: Crumble Cavern" },
            { 1, "Junimo Kart: Slippery Slopes" },
            { 8, "Junimo Kart: Secret Level" },
            { 2, "Junimo Kart: The Gem Sea Giant" },
            { 5, "Junimo Kart: Slomp's Stomp" },
            { 3, "Junimo Kart: Ghastly Galleon" },
            { 9, "Junimo Kart: Glowshroom Grotto" },
            { 4, "Junimo Kart: Red Hot Rollercoaster" },
            { 6, JK_VICTORY },
        };

        private static readonly string[] JK_ALL_LOCATIONS = JK_LEVEL_LOCATIONS.Values.ToArray();

        private static ILogger _logger;
        private static IModHelper _helper;
        private static StardewArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        private static List<KeyValuePair<string, int>> _latestScores;

        public static void Initialize(ILogger logger, IModHelper helper, StardewArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _helper = helper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            _latestScores = new List<KeyValuePair<string, int>>();
        }

        public static void RestartLevel_NewGame_Postfix(MineCart __instance, bool new_game)
        {
            try
            {
                var livesLeftField = _helper.Reflection.GetField<int>(__instance, "livesLeft");
                var livesLeft = livesLeftField.GetValue();

                if (livesLeft != 3 || !new_game)
                {
                    return;
                }
                var numberExtraLives = GetJunimoKartExtraLives();
                livesLeftField.SetValue(livesLeft + numberExtraLives);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(RestartLevel_NewGame_Postfix)}:\n{ex}");
                return;
            }
        }

        public static void UpdateFruitsSummary_ExtraLives_Postfix(MineCart __instance, float time)
        {
            try
            {
                SendJunimoKartLevelsBeatChecks(__instance);

                var livesLeftField = _helper.Reflection.GetField<int>(__instance, "livesLeft");
                var livesLeft = livesLeftField.GetValue();
                var numberExtraLives = GetJunimoKartExtraLives();
                if (livesLeft >= 3 + numberExtraLives)
                {
                    return;
                }
                livesLeftField.SetValue(3 + numberExtraLives);

                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(UpdateFruitsSummary_ExtraLives_Postfix)}:\n{ex}");
                return;
            }
        }

        public static bool EndCutscene_JunimoKartLevelComplete_Prefix(MineCart __instance)
        {
            try
            {
                SendJunimoKartLevelsBeatChecks(__instance);
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(EndCutscene_JunimoKartLevelComplete_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static void SendJunimoKartLevelsBeatChecks(MineCart __instance)
        {
            var gamemode = _helper.Reflection.GetField<int>(__instance, "gameMode");
            var levelsBeat = _helper.Reflection.GetField<int>(__instance, "levelsBeat");
            var currentLevel = _helper.Reflection.GetField<int>(__instance, "currentTheme");
            var levelsFinishedThisRun =
                _helper.Reflection.GetField<List<int>>(__instance, "levelThemesFinishedThisRun");
            if (gamemode.GetValue() != 3 || levelsBeat.GetValue() < 1)
            {
                return;
            }

            foreach (var levelFinished in levelsFinishedThisRun.GetValue())
            {
                var location = JK_LEVEL_LOCATIONS[levelFinished];
                if (location == JK_VICTORY && _locationChecker.IsLocationMissing(JK_VICTORY))
                {
                    Game1.chatBox?.addMessage("You can now type '!!arcade_release jk' to release all remaining Junimo Kart checks", Color.Green);
                }
                _locationChecker.AddCheckedLocation(location);
            }
        }

        private static int GetJunimoKartExtraLives()
        {
            var numberExtraLives = 8;
            if (_archipelago.SlotData.ArcadeMachineLocations == ArcadeLocations.FullShuffling)
            {
                numberExtraLives = _archipelago.GetReceivedItemCount(JK_EXTRA_LIFE);
            }

            return numberExtraLives;
        }

        public static void ReleaseJunimoKart()
        {
            foreach (var junimoKartLocation in JK_ALL_LOCATIONS)
            {
                _locationChecker.AddCheckedLocation(junimoKartLocation);
            }
        }

        // public void submitHighScore()
        public static bool SubmitHighScore_AddScoreToMultiworld_Prefix(MineCart __instance)
        {
            try
            {
                // private int score;
                var scoreField = _helper.Reflection.GetField<int>(__instance, "score");
                var score = scoreField.GetValue();

                var name = _archipelago.SlotData.SlotName;

                if (Game1.player.team.junimoKartScores.GetScores()[0].Value < score)
                {
                    // Game1.multiplayer.globalChatInfoMessage("JunimoKartHighScore", Game1.player.Name);
                }
                Game1.player.team.junimoKartScores.AddScore(name, score);
                if (Game1.player.team.specialOrders != null)
                {
                    foreach (var specialOrder in Game1.player.team.specialOrders)
                    {
                        var onJkScoreAchieved = specialOrder.onJKScoreAchieved;
                        if (onJkScoreAchieved != null)
                        {
                            onJkScoreAchieved(Game1.player, score);
                        }
                    }
                }

                __instance.RefreshHighScore();

                var session = _archipelago.GetSession();
                session.DataStorage[Scope.Global, JK_DATASTORAGE_SCORE].Initialize(EmptyScoresDictionary);

                var existingScores = Game1.player.team.junimoKartScores.GetScores();
                var myScores = existingScores.Where(x => x.Key.Equals(name));
                var myBestScore = myScores.Max(x => x.Value);

                var newEntry = new Dictionary<string, int> { { name, myBestScore } };
                session.DataStorage[Scope.Global, JK_DATASTORAGE_SCORE] += Operation.Update(newEntry);

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(SubmitHighScore_AddScoreToMultiworld_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public List<KeyValuePair<string, int>> GetScores()
        public static bool GetScores_AddScoresFromMultiworld_Prefix(NetLeaderboards __instance, ref List<KeyValuePair<string, int>> __result)
        {
            try
            {
                AddJKScore(__instance.entries, "Lewis", 50000);
                AddJKScore(__instance.entries, "Shane", 25000);
                AddJKScore(__instance.entries, "Sam", 10000);
                AddJKScore(__instance.entries, "Abigail", 5000);
                AddJKScore(__instance.entries, "Vincent", 250);

                _latestScores = new List<KeyValuePair<string, int>>();
                foreach (var entry in __instance.entries)
                {
                    _latestScores.Add(new KeyValuePair<string, int>(entry.name.Value, entry.score.Value));
                }

                var session = _archipelago.GetSession();
                session.DataStorage[Scope.Global, JK_DATASTORAGE_SCORE].Initialize(EmptyScoresDictionary);
                var multiworldScores = session.DataStorage[Scope.Global, JK_DATASTORAGE_SCORE].To<Dictionary<string, int>>();
                foreach (var (name, score) in multiworldScores)
                {
                    if (!_latestScores.Any(x => x.Key.Equals(name)))
                    {
                        _latestScores.Add(new KeyValuePair<string, int>(name, score));
                    }
                }

                _latestScores.Sort(((a, b) => a.Value.CompareTo(b.Value)));
                _latestScores.Reverse();

                var names = new HashSet<string>();
                for (var i = 0; i < _latestScores.Count; i++)
                {
                    if (names.Contains(_latestScores[i].Key))
                    {
                        _latestScores.RemoveAt(i);
                        i--;
                        continue;
                    }

                    names.Add(_latestScores[i].Key);
                }

                var scoresForLeaderboard = _latestScores.Take(__instance.maxEntries.Value).ToList();

                __result = scoresForLeaderboard;

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(GetScores_AddScoresFromMultiworld_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static void AddJKScore(NetObjectList<NetLeaderboardsEntry> entries, string npcName, int score)
        {
            if (entries.Any(x => x.name.Value.Equals(npcName)))
            {
                return;
            }

            Game1.player.team.junimoKartScores.AddScore(Game1.RequireCharacter(npcName).displayName, score);
        }
    }
}
