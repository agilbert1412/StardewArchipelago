using System;
using System.Collections.Generic;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Minigames;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Arcade
{
    public static class JunimoKartInjections
    {
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

        public static void Initialize(ILogger logger, IModHelper helper, StardewArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _helper = helper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
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
    }
}
