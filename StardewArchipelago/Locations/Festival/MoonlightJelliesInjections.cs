﻿using System;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewModdingAPI;
using StardewValley;

namespace StardewArchipelago.Locations.Festival
{
    internal class MoonlightJelliesInjections
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
                _monitor.Log($"Failed in {nameof(SetUpFestivalMainEvent_MoonlightJellies_Postfix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }
    }
}
