using System;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago.ApworldData;
using StardewArchipelago.Constants.Vanilla;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Minigames;

namespace StardewArchipelago.Locations.Jojapocalypse.Consequences
{
    internal class ArcadeConsequences
    {
        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;
        private static JojaLocationChecker _jojaLocationChecker;

        public static void Initialize(ILogger logger, IModHelper modHelper, ArchipelagoClient archipelago, JojaLocationChecker jojaLocationChecker)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _jojaLocationChecker = jojaLocationChecker;
        }

        // public bool tick(GameTime time)
        public static bool Tick_IncreaseJotPKTimescale_Prefix(AbigailGame __instance, ref GameTime time, ref bool __result)
        {
            try
            {
                var numberArcadePurchased = _jojaLocationChecker.CountCheckedLocationsWithTag(LocationTag.JOTPK) +
                                               _jojaLocationChecker.CountCheckedLocationsWithTag(LocationTag.JUNIMO_KART);
                if (numberArcadePurchased <= 0)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                var timeScaleFactor = Math.Pow(1.05, numberArcadePurchased); // Max is 22
                time = new GameTime(time.TotalGameTime * timeScaleFactor, time.ElapsedGameTime * timeScaleFactor, time.IsRunningSlowly);

                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(Tick_IncreaseJotPKTimescale_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public bool tick(GameTime time)
        public static bool Tick_IncreaseJunimoKartTimescale_Prefix(MineCart __instance, ref GameTime time, ref bool __result)
        {
            try
            {
                var numberArcadePurchased = _jojaLocationChecker.CountCheckedLocationsWithTag(LocationTag.JOTPK) +
                                            _jojaLocationChecker.CountCheckedLocationsWithTag(LocationTag.JUNIMO_KART);
                if (numberArcadePurchased <= 0)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                var timeScaleFactor = Math.Pow(1.05, numberArcadePurchased); // Max is 22
                time = new GameTime(time.TotalGameTime * timeScaleFactor, time.ElapsedGameTime * timeScaleFactor, time.IsRunningSlowly);

                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(Tick_IncreaseJunimoKartTimescale_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }
    }
}
