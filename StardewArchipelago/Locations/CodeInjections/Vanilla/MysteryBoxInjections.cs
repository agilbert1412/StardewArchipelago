using System;
using StardewArchipelago.Archipelago;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Events;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public static class MysteryBoxInjections
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

        // public static FarmEvent pickFarmEvent()
        public static void PickFarmEvent_MrQiPlaneOnlyIfUnlocked_Postfix(ref FarmEvent __result)
        {
            try
            {
                const string qiPlaneMail = "sawQiPlane";
                var hasUnlockedQiPlane = _archipelago.HasReceivedItem("Mr Qi's Plane Ride");
                var hasSeenQiPlaneEvent = Game1.player.mailReceived.Contains(qiPlaneMail);
                if (!hasUnlockedQiPlane && hasSeenQiPlaneEvent)
                {
                    Game1.player.mailReceived.Remove(qiPlaneMail);
                }

                if (__result == null && hasUnlockedQiPlane && !hasSeenQiPlaneEvent)
                {
                    __result = new QiPlaneEvent();
                    return;
                }

                if (__result is QiPlaneEvent && !hasUnlockedQiPlane)
                {
                    __result = null;
                    return;
                }
                
                return;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(PickFarmEvent_MrQiPlaneOnlyIfUnlocked_Postfix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }
    }
}
