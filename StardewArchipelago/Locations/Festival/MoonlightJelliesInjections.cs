using System;
using Microsoft.Xna.Framework;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using StardewModdingAPI;
using StardewValley;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.Utilities.Interfaces;

namespace StardewArchipelago.Locations.Festival
{
    internal class MoonlightJelliesInjections
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

        // public void setUpFestivalMainEvent()
        public static void SetUpFestivalMainEvent_MoonlightJellies_Postfix(Event __instance)
        {
            try
            {
                if (!__instance.isSpecificFestival("summer28"))
                {
                    return;
                }

                Game1.chatBox?.addMessage("Watching the moonlight jellies fills you with determination", Color.Gold);
                _locationChecker.AddCheckedLocation(FestivalLocationNames.WATCH_MOONLIGHT_JELLIES);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(SetUpFestivalMainEvent_MoonlightJellies_Postfix)}:\n{ex}");
                return;
            }
        }
    }
}
