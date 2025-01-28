using System;

namespace StardewArchipelago.Items.Traps
{
    public class QueuedGiftTrap : QueuedTrap
    {
        private double _quality;
        private double _duration;
        private Action<double, double> _action;

        public QueuedGiftTrap(string name, Action<double, double> action) : base(name)
        {
            _action = action;
        }

        public override void ExecuteNow()
        {
            _action?.Invoke(_quality, _duration);
        }
    }
}
