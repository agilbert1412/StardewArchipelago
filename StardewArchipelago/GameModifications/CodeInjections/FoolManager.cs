using StardewValley;
using System;
using Microsoft.Xna.Framework;

namespace StardewArchipelago.GameModifications.CodeInjections
{
    public class FoolManager
    {
        private static bool _shouldPrankOnFishDay = true;
        private static bool _shouldPrankOnOtherDays = false;

        public static bool IsPrankMonth()
        {
            return DateTime.Now.Month == 4;
        }

        public static bool IsPrankDay()
        {
            return IsPrankMonth() && DateTime.Now.Day == 1;
        }

        public static bool ShouldDoZeldaPrank()
        {
            return false;
            // return ShouldPrank();
        }

        internal static bool ShouldPrank()
        {
            // return true;
            return IsPrankDay() ? _shouldPrankOnFishDay : _shouldPrankOnOtherDays;
        }

        internal static void TogglePrank(bool silent)
        {
            if (IsPrankDay())
            {
                if (!silent)
                {
                    if (_shouldPrankOnFishDay)
                    {
                        Game1.chatBox.addMessage("Oh, the fun's already over?", Color.Gold);
                    }
                    else
                    {
                        Game1.chatBox.addMessage("Welcome back", Color.Gold);
                    }
                }
                _shouldPrankOnFishDay = !_shouldPrankOnFishDay;
            }
            else
            {
                if (!silent)
                {
                    if (_shouldPrankOnOtherDays)
                    {
                        Game1.chatBox.addMessage("That's what I thought.", Color.Gold);
                    }
                    else
                    {
                        Game1.chatBox.addMessage("Really? You actually like this?", Color.Gold);
                    }
                }
                _shouldPrankOnOtherDays = !_shouldPrankOnOtherDays;
            }
        }
    }
}
