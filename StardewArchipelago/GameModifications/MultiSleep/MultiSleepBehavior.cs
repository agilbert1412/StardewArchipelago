using Microsoft.Xna.Framework;
using StardewArchipelago.GameModifications.Seasons;
using StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles;
using StardewValley;
using StardewValley.Menus;

namespace StardewArchipelago.GameModifications.MultiSleep
{
    public abstract class MultiSleepBehavior
    {
        private int _numberOfDaysSlept = 0;

        public abstract bool ShouldKeepSleeping();

        public static void SleepOnce()
        {
            NewDay(0);
            Game1.newDayAfterFade(AfterNewDay);
            return;

            static void AfterNewDay()
            {
                if (Game1.eventOver)
                {
                    Game1.eventFinished();
                    if (Game1.dayOfMonth == 0)
                        Game1.newDayAfterFade(() => Game1.player.Position = new Vector2(320f, 320f));
                }
                Game1.nonWarpFade = false;
                Game1.fadeIn = false;
            }
        }

        public virtual void KeepSleeping()
        {
            _numberOfDaysSlept++;
            ArchipelagoJunimoNoteMenu.HasBeenHibernatingFor(_numberOfDaysSlept);

            if (Game1.dayOfMonth == 28)
            {
                SeasonsRandomizer.ChooseNextSeasonBasedOnConfigPreference();
                return;
            }

            SleepOnce();
        }

        private static void NewDay(float timeToPause)
        {
            if (Game1.activeClickableMenu is ReadyCheckDialog activeClickableMenu && activeClickableMenu.checkName == "sleep" && !activeClickableMenu.isCancelable())
                activeClickableMenu.confirm();
            Game1.currentMinigame = null;
            Game1.newDay = true;
            Game1.newDaySync.create();
            if (Game1.player.isInBed.Value || Game1.player.passedOut)
            {
                Game1.nonWarpFade = true;
                // Game1.screenFade.FadeScreenToBlack(Game1.player.passedOut ? 1.1f : 0.0f);
                Game1.player.Halt();
                Game1.player.currentEyes = 1;
                Game1.player.blinkTimer = -4000;
                Game1.player.CanMove = false;
                Game1.player.passedOut = false;
                Game1.pauseTime = timeToPause;
            }
            if (Game1.activeClickableMenu == null || Game1.dialogueUp)
                return;
            Game1.activeClickableMenu.emergencyShutDown();
            Game1.exitActiveMenu();
        }
    }
}
