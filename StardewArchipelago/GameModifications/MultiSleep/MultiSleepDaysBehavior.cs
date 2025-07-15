
namespace StardewArchipelago.GameModifications.MultiSleep
{
    public class MultiSleepDaysBehavior : MultiSleepBehavior
    {
        private int _totalDays;
        private int _daysRemaining;

        public MultiSleepDaysBehavior(int days)
        {
            _totalDays = days;
            _daysRemaining = days;
        }

        public override bool ShouldKeepSleeping()
        {
            return _daysRemaining > 0;
        }

        public override void KeepSleeping()
        {
            _daysRemaining--;
            base.KeepSleeping();
        }

        public override bool ShouldPromptForMultisleep()
        {
            return false;
        }
    }
}
