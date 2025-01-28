namespace StardewArchipelago.Items.Traps
{
    public abstract class QueuedTrap
    {
        public string Name { get; set; }

        protected QueuedTrap(string name)
        {
            Name = name;
        }

        public abstract void ExecuteNow();
    }
}
