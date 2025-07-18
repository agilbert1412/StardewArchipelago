﻿using System;
using System.Linq;
using Netcode;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using StardewArchipelago.Constants;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Events;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.ArchipelagoUtilities.Net;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public static class FarmEventInjections
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

        // public static FarmEvent pickFarmEvent()
        public static void PickFarmEvent_MrQiPlaneOnlyIfUnlocked_Postfix(ref FarmEvent __result)
        {
            try
            {
                __result = OverwriteQiPlaneEvent(__result);
                __result = OverwriteRaccoonStumpEvent(__result);
                __result = OverwriteAbandonedJojaMartEvent(__result);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(PickFarmEvent_MrQiPlaneOnlyIfUnlocked_Postfix)}:\n{ex}");
                return;
            }
        }

        private static FarmEvent OverwriteQiPlaneEvent(FarmEvent nightEvent)
        {
            var isConnected = _archipelago.MakeSureConnected();

            const string qiPlaneMail = "sawQiPlane";
            var hasUnlockedQiPlane = _archipelago.HasReceivedItem("Mr Qi's Plane Ride");
            var hasSeenQiPlaneEvent = Game1.player.mailReceived.Contains(qiPlaneMail);
            if (isConnected && !hasUnlockedQiPlane && hasSeenQiPlaneEvent)
            {
                Game1.player.mailReceived.Remove(qiPlaneMail);
            }

            if (isConnected && nightEvent == null && hasUnlockedQiPlane && !hasSeenQiPlaneEvent)
            {
                return new QiPlaneEvent();
            }

            if (nightEvent is QiPlaneEvent && !hasUnlockedQiPlane)
            {
                // Don't play that stupid sound. Yes this is jank. No I don't want to rewrite the entirety of Utility.PicKFarmEvent() just for this.
                Game1.delayedActions = Game1.delayedActions.Where(x => x.stringData != "planeflyby").ToList();
                return null;
            }

            return nightEvent;
        }

        // public static FarmEvent pickFarmEvent()
        public static FarmEvent OverwriteRaccoonStumpEvent(FarmEvent __result)
        {
            if (__result is not SoundInTheNightEvent soundInTheNightEvent)
            {
                return __result;
            }

            // private readonly NetInt behavior = new NetInt();
            var behaviorField = _modHelper.Reflection.GetField<NetInt>(soundInTheNightEvent, "behavior");
            var behavior = behaviorField.GetValue();
            const int raccoonStumpNightEvent = 5;
            if (behavior.Value != raccoonStumpNightEvent)
            {
                return __result;
            }

            return null;
        }

        // public static FarmEvent pickFarmEvent()
        public static FarmEvent OverwriteAbandonedJojaMartEvent(FarmEvent nightEvent)
        {
            if (Game1.weddingToday || nightEvent != null || !_archipelago.HasReceivedItem(APItem.MOVIE_THEATER))
            {
                return nightEvent;
            }

            if ((Game1.isRaining || Game1.isLightning || Game1.IsWinter) && !Game1.MasterPlayer.mailReceived.Contains("abandonedJojaMartAccessible"))
            {
                return new WorldChangeEvent(12);
            }

            if (_archipelago.GetReceivedItemCount(APItem.MOVIE_THEATER) < 2)
            {
                return nightEvent;
            }

            if (!Game1.player.mailReceived.Contains("ccMovieTheater%&NL&%") && !Game1.player.mailReceived.Contains("ccMovieTheater"))
            {
                Game1.player.mailReceived.Add("ccMovieTheater");
                return new WorldChangeEvent(11);
            }

            return nightEvent;
        }
    }
}
