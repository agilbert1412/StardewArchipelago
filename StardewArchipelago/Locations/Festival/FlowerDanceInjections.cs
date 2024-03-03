using System;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace StardewArchipelago.Locations.Festival
{
    public static class FlowerDanceInjections
    {
        private static IMonitor _monitor;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;
        private static ShopReplacer _shopReplacer;

        public static void Initialize(IMonitor monitor, IModHelper modHelper, ArchipelagoClient archipelago, LocationChecker locationChecker, ShopReplacer shopReplacer)
        {
            _monitor = monitor;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            _shopReplacer = shopReplacer;
        }

        // public void setUpFestivalMainEvent()
        public static void SetUpFestivalMainEvent_FlowerDance_Postfix(Event __instance)
        {
            try
            {
                if (!__instance.isSpecificFestival("spring24"))
                {
                    return;
                }

                if (Game1.player.dancePartner.Value == null)
                {
                    return;
                }

                _locationChecker.AddCheckedLocation(FestivalLocationNames.DANCE_WITH_SOMEONE);
                return;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(SetUpFestivalMainEvent_FlowerDance_Postfix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }
    }
}
