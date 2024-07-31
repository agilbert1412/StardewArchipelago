using System;
using Microsoft.Xna.Framework;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using StardewArchipelago.Serialization;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.ArchipelagoUtilities.Net;
using StardewArchipelago.Archipelago;

namespace StardewArchipelago.Locations.Festival
{
    internal class FairInjections
    {
        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static StardewArchipelagoClient _archipelago;
        private static ArchipelagoStateDto _state;
        private static LocationChecker _locationChecker;

        public static void Initialize(ILogger logger, IModHelper modHelper, StardewArchipelagoClient archipelago, ArchipelagoStateDto state, LocationChecker locationChecker)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _state = state;
            _locationChecker = locationChecker;
        }

        // public override void update(GameTime time)
        public static bool StrengthGameUpdate_StrongEnough_Prefix(StrengthGame __instance, GameTime time)
        {
            try
            {
                var changeSpeedField = _modHelper.Reflection.GetField<float>(__instance, "changeSpeed");
                var changeSpeed = changeSpeedField.GetValue();
                if (changeSpeed != 0.0)
                {
                    return true; // run original logic
                }

                var powerField = _modHelper.Reflection.GetField<float>(__instance, "power");
                var power = powerField.GetValue();
                if (power >= 99.0 || (power < 2.0 && _archipelago.SlotData.FestivalLocations != FestivalLocations.Hard))
                {
                    _locationChecker.AddCheckedLocation(FestivalLocationNames.STRENGTH_GAME);
                }
                return true; // run original logic
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(StrengthGameUpdate_StrongEnough_Prefix)}:\n{ex}");
                return true; // run original logic
            }
        }

        // public void interpretGrangeResults()
        public static void InterpretGrangeResults_Success_Postfix(Event __instance)
        {
            try
            {
                var isEasyMode = _archipelago.SlotData.FestivalLocations != FestivalLocations.Hard;
                if (__instance.grangeScore >= 90 || ((__instance.grangeScore >= 60 || __instance.grangeScore == -666) && isEasyMode))
                {
                    _locationChecker.AddCheckedLocation(FestivalLocationNames.GRANGE_DISPLAY);
                }

                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(InterpretGrangeResults_Success_Postfix)}:\n{ex}");
                return;
            }
        }

        // public void forceEndFestival(Farmer who)
        public static bool ForceEndFestival_KeepStarTokens_Prefix(Event __instance, Farmer who)
        {
            try
            {
                if (!__instance.isSpecificFestival("fall16"))
                {
                    return true; // run original logic
                }

                _state.StoredStarTokens += who.festivalScore;
                return true; // run original logic
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(ForceEndFestival_KeepStarTokens_Prefix)}:\n{ex}");
                return true; // run original logic
            }
        }
    }
}
