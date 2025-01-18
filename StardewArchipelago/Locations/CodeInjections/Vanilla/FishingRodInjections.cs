using System;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using StardewArchipelago.Stardew.Ids.Vanilla;
using StardewModdingAPI;
using StardewValley;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Constants;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public static class FishingRodInjections
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
        public static bool SkipEvent_BambooPole_Prefix(Event __instance)
        {
            try
            {
                if (__instance.id != EventIds.BAMBOO_POLE)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                EventInjections.BaseSkipEvent(__instance, CheckBambooPoleLocation);
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(SkipEvent_BambooPole_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public void skipEvent()
        public static bool SkipEvent_WillyFishingLesson_Prefix(Event __instance)
        {
            try
            {
                if (__instance.id != EventIds.WILLY_FISHING_LESSON)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                EventInjections.BaseSkipEvent(__instance, CheckFishingLessonLocations);
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(SkipEvent_WillyFishingLesson_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public static void AwardFestivalPrize(Event @event, string[] args, EventContext context)
        public static bool AwardFestivalPrize_BambooPole_Prefix(Event @event, string[] args, EventContext context)
        {
            try
            {
                if (@event.id != EventIds.BAMBOO_POLE || args.Length <= 1 || args[1].ToLower() != "rod")
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                CheckBambooPoleLocation();

                if (Game1.activeClickableMenu == null)
                    @event.CurrentCommand++;
                @event.CurrentCommand++;

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(AwardFestivalPrize_BambooPole_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public static void AwardFestivalPrize(Event @event, string[] args, EventContext context)
        public static bool AwardFestivalPrize_WillyFishingLesson_Prefix(Event @event, string[] args, EventContext context)
        {
            try
            {
                if (@event.id != EventIds.WILLY_FISHING_LESSON)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                CheckFishingLessonLocations();

                if (Game1.activeClickableMenu == null)
                    @event.CurrentCommand++;
                @event.CurrentCommand++;

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(AwardFestivalPrize_WillyFishingLesson_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public static void GainSkill(Event @event, string[] args, EventContext context)
        public static bool GainSkill_WillyFishingLesson_Prefix(Event @event, string[] args, EventContext context)
        {
            try
            {
                if (@event.id != EventIds.WILLY_FISHING_LESSON)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                CheckFishingLessonLocations();

                @event.CurrentCommand++;

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(GainSkill_WillyFishingLesson_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static void CheckBambooPoleLocation()
        {
            _locationChecker.AddCheckedLocation("Bamboo Pole Cutscene");
        }

        private static void CheckFishingLessonLocations()
        {
            _locationChecker.AddCheckedLocation("Level 1 Fishing");
            _locationChecker.AddCheckedLocation("Purchase Training Rod");
        }
    }
}
