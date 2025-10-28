using System;
using Microsoft.Xna.Framework;
using StardewArchipelago.Serialization;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.Utilities.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;

namespace StardewArchipelago.Locations.Festival
{
    internal class FairInjections
    {
        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static StardewArchipelagoClient _archipelago;
        private static ArchipelagoWalletDto _wallet;
        private static LocationChecker _locationChecker;

        public static void Initialize(ILogger logger, IModHelper modHelper, StardewArchipelagoClient archipelago, ArchipelagoWalletDto wallet, LocationChecker locationChecker)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _wallet = wallet;
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
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                var powerField = _modHelper.Reflection.GetField<float>(__instance, "power");
                var power = powerField.GetValue();
                if (power >= 99.0 || (power < 2.0 && _archipelago.SlotData.FestivalLocations != FestivalLocations.Hard))
                {
                    _locationChecker.AddCheckedLocation(FestivalLocationNames.STRENGTH_GAME);
                }
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(StrengthGameUpdate_StrongEnough_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
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
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                _wallet.StarTokens += who.festivalScore;
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(ForceEndFestival_KeepStarTokens_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }
    }
}
