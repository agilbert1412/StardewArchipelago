using System;

namespace StardewArchipelago.Items.Traps
{
    public class QueuedGiftTrap : QueuedTrap
    {
        private double _quality;
        private double _duration;
        private Action<double, double> _action;

        public QueuedGiftTrap(string name, Action<double, double> action, double quality, double duration) : base(name)
        {
            _action = action;
            _quality = quality;
            _duration = duration;
        }

        public override void ExecuteNow()
        {
            _action?.Invoke(_quality, _duration);
        }
    }
}
