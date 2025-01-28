using System;

namespace StardewArchipelago.Items.Traps
{
    public class QueuedItemTrap : QueuedTrap
    {
        private Action _action;

        public QueuedItemTrap(string name, Action action) : base(name)
        {
            _action = action;
        }

        public override void ExecuteNow()
        {
            _action?.Invoke();
        }
    }
}
