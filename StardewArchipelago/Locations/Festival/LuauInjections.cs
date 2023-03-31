using System;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Locations.Events;
using StardewModdingAPI;
using StardewValley;

namespace StardewArchipelago.Locations.Festival
{
    public static class LuauInjections
    {
        private static IMonitor _monitor;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(IMonitor monitor, IModHelper modHelper, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _monitor = monitor;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        // public virtual void command_switchEvent(GameLocation location, GameTime time, string[] split)
        public static void SwitchEvent_GovernorReactionToSoup_Postfix(Event __instance, GameLocation location, GameTime time, string[] split)
        {
            try
            {
                var switchEventKey = split[1];
                var governorReactionKey = "governorReaction";
                if (!__instance.isSpecificFestival("summer11") || !switchEventKey.StartsWith(governorReactionKey))
                {
                    return;
                }

                var soupScore = int.Parse(switchEventKey[governorReactionKey.Length..]);
                var isEasyMode = _archipelago.SlotData.FestivalObjectives != FestivalObjectives.Difficult;

                if (soupScore == 4 || (isEasyMode && soupScore is 2 or 3))
                {
                    _locationChecker.AddCheckedLocation(FestivalLocationNames.LUAU_SOUP);
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(SwitchEvent_GovernorReactionToSoup_Postfix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }
    }
}
