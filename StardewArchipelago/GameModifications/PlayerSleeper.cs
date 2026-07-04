using KaitoKid.Utilities.Interfaces;
using Microsoft.Xna.Framework;
using StardewArchipelago.Extensions;
using StardewArchipelago.GameModifications.MultiSleep;
using StardewArchipelago.Items.Traps;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StardewArchipelago.GameModifications
{
    public class PlayerSleeper
    {
        private readonly ILogger _logger;

        public PlayerSleeper(ILogger logger)
        {
            _logger = logger;
        }

        public bool SleepCommand(string message)
        {
            var numberOfDays = 1;
            var messageParts = message.Split(" ");
            if (messageParts[0].Equals("sleep", StringComparison.InvariantCultureIgnoreCase))
            {
                messageParts = messageParts.Skip(1).ToArray();
            }

            SleepImmediately(messageParts);
            return true;
        }

        public void SleepImmediately(string[] messageParts)
        {
            void SleepImmediatelyMethod()
            {
                SetMultisleep(messageParts);
                Game1.player.startToPassOut();
            }

            EscapeCurrentFestival(SleepImmediatelyMethod);
        }

        private bool EscapeCurrentFestival(Action sleepImmediatelyMethod)
        {
            if (!Game1.eventUp || Game1.CurrentEvent == null || !Game1.CurrentEvent.isFestival || Game1.activeClickableMenu != null)
            {
                sleepImmediatelyMethod();
                return false;
            }

            var festival = Game1.CurrentEvent;
            Game1.netReady.SetLocalReady("festivalEnd", true);
            Game1.activeClickableMenu = new ReadyCheckDialog("festivalEnd", true, (who) => EndFestivalThenSleep(festival, who, sleepImmediatelyMethod) );
            return true;
        }

        private void EndFestivalThenSleep(Event festival, Farmer who, Action sleepImmediatelyMethod)
        {
            festival.forceEndFestival(who);
            sleepImmediatelyMethod();
        }

        private void SetMultisleep(string[] messageParts)
        {
            SetLastBedToFarmhouse();
            if (messageParts == null || messageParts.Length <= 0)
            {
                return;
            }

            var argument = string.Join("", messageParts);
            argument = argument.Replace(" ", "").Replace(",", "").Replace("_", "").Replace("-", "");
            if (int.TryParse(argument, out var numberOfDays))
            {
                MultiSleepManager.SetDaysToSkip(numberOfDays - 1);
                return;
            }

            MultiSleepManager.SetCurrentUntilBehavior(argument);
        }

        private void SetLastBedToFarmhouse()
        {
            try
            {
                var location = Game1.getLocationFromName("FarmHouse");
                if (location is not FarmHouse farmhouse)
                {
                    return;
                }

                Game1.player.lastSleepLocation.Set(farmhouse.NameOrUniqueName);
                var bedSpot = farmhouse.GetPlayerBedSpot();
                Game1.player.lastSleepPoint.Set(bedSpot);
                var bed = farmhouse.GetBed();
                if (bed == null)
                {
                    return;
                }

                Game1.player.mostRecentBed = bed.TileLocation;
                Game1.player.currentLocation.locationContextId = "Default";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed at setting last bed. Error: {ex}");
            }
        }
    }
}
