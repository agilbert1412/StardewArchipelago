using System;
using StardewModdingAPI;
using StardewValley;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.ArchipelagoUtilities.Net;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.SlotData;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;

namespace StardewArchipelago.Locations.Festival
{
    public static class LuauInjections
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

        // public static void SwitchEvent(Event @event, string[] args, EventContext context)
        public static void SwitchEvent_GovernorReactionToSoup_Postfix(Event @event, string[] args, EventContext context)
        {
            try
            {
                var switchEventKey = args[1];
                var governorReactionKey = "governorReaction";
                if (!@event.isSpecificFestival("summer11") || !switchEventKey.StartsWith(governorReactionKey))
                {
                    return;
                }

                var soupScore = int.Parse(switchEventKey[governorReactionKey.Length..]);
                var isEasyMode = _archipelago.SlotData.FestivalLocations != FestivalLocations.Hard;

                if (soupScore == 4 || (isEasyMode && soupScore is 2 or 3))
                {
                    _locationChecker.AddCheckedLocation(FestivalLocationNames.LUAU_SOUP);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(SwitchEvent_GovernorReactionToSoup_Postfix)}:\n{ex}");
                return;
            }
        }
    }
}
