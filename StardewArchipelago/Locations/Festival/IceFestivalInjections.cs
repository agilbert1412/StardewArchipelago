using System;
using System.Collections.Generic;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using StardewModdingAPI;
using StardewValley;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.ArchipelagoUtilities.Net;
using StardewArchipelago.Archipelago;

namespace StardewArchipelago.Locations.Festival
{
    internal class IceFestivalInjections
    {
        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static StardewArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(ILogger logger, IModHelper modHelper, StardewArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        // public static void AwardFestivalPrize(Event @event, string[] args, EventContext context)
        public static bool AwardFestivalPrize_FishingCompetition_Prefix(Event @event, string[] args, EventContext context)
        {
            try
            {
                var festivalWinnersField = _modHelper.Reflection.GetField<HashSet<long>>(@event, "festivalWinners");
                var festivalWinners = festivalWinnersField.GetValue();
                var festivalDataField = _modHelper.Reflection.GetField<Dictionary<string, string>>(@event, "festivalData");
                var festivalData = festivalDataField.GetValue();

                if (festivalWinners == null || festivalData == null)
                {
                    return true; // run original logic
                }

                var playerWonFestival = festivalWinners.Contains(Game1.player.UniqueMultiplayerID);
                var isIceFestivalDay = festivalData["file"] == "winter8";

                if (!playerWonFestival || !isIceFestivalDay)
                {
                    return true; // run original logic
                }

                _locationChecker.AddCheckedLocation(FestivalLocationNames.FISHING_COMPETITION);
                if (Game1.player.mailReceived.Contains("Ice Festival"))
                {
                    return true; // run original logic
                }

                Game1.player.mailReceived.Add("Ice Festival");
                @event.CurrentCommand += 2;

                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(AwardFestivalPrize_FishingCompetition_Prefix)}:\n{ex}");
                return true; // run original logic
            }
        }
    }
}
