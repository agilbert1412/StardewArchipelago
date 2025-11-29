using System;
using System.Collections.Generic;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using StardewModdingAPI;
using StardewValley;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.Utilities.Interfaces;

namespace StardewArchipelago.Locations.Festival
{
    public static class EggFestivalInjections
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

        // public static void AwardFestivalPrize(Event @event, string[] args, EventContext context)
        public static bool AwardFestivalPrize_Strawhat_Prefix(Event @event, string[] args, EventContext context)
        {
            try
            {
                // private HashSet<long> festivalWinners = new HashSet<long>();
                var festivalWinnersField = _modHelper.Reflection.GetField<HashSet<long>>(@event, "festivalWinners");

                // private Dictionary<string, string> festivalData;
                var festivalDataField = _modHelper.Reflection.GetField<Dictionary<string, string>>(@event, "festivalData");

                var festivalWinners = festivalWinnersField.GetValue();
                var festivalData = festivalDataField.GetValue();

                if (festivalWinners == null || festivalData == null)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                var playerWonFestival = festivalWinners.Contains(Game1.player.UniqueMultiplayerID);
                var isEggFestivalDay = festivalData["file"] == "spring13";

                if (!playerWonFestival || !isEggFestivalDay)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                _locationChecker.AddCheckedLocation(FestivalLocationNames.EGG_HUNT);
                if (Game1.player.mailReceived.Contains("Egg Festival"))
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                Game1.player.mailReceived.Add("Egg Festival");
                @event.CurrentCommand += 2;

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(AwardFestivalPrize_Strawhat_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }
    }
}
