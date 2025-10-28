using System;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using StardewModdingAPI;
using StardewValley;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.Utilities.Interfaces;
using StardewArchipelago.Constants.Vanilla;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public static class CopperPanInjections
    {
        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(ILogger logger, IModHelper modHelper, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        // public void skipEvent()
        public static bool SkipEvent_CopperPan_Prefix(Event __instance)
        {
            try
            {
                if (__instance.id != EventIds.COPPER_PAN)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                EventInjections.BaseSkipEvent(__instance, CheckCopperPanLocation);
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(SkipEvent_CopperPan_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public static void AwardFestivalPrize(Event @event, string[] args, EventContext context)
        public static bool AwardFestivalPrize_CopperPan_Prefix(Event @event, string[] args, EventContext context)
        {
            try
            {
                if (@event.id != EventIds.COPPER_PAN || args.Length <= 1 || args[1].ToLower() != "pan")
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                CheckCopperPanLocation();

                if (Game1.activeClickableMenu == null)
                {
                    @event.CurrentCommand++;
                }
                @event.CurrentCommand++;

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(AwardFestivalPrize_CopperPan_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static void CheckCopperPanLocation()
        {
            _locationChecker.AddCheckedLocation("Copper Pan Cutscene");
        }
    }
}
