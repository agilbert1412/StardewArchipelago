namespace StardewArchipelago.Items.Traps.Shuffle
{
    public class Inventory
    {
        public InventoryInfo Info { get; set; }
        public InventoryContent Content { get; set; }

        public Inventory(InventoryInfo info, InventoryContent content)
        {
            Info = info;
            Content = content;
        }
    }
}
