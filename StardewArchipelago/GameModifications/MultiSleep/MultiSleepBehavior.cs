using StardewValley;

namespace StardewArchipelago.GameModifications.MultiSleep
{
    public abstract class MultiSleepBehavior
    {
        public abstract bool ShouldKeepSleeping();

        public virtual void KeepSleeping()
        {
            Game1.NewDay(0);
        }
    }
}
