namespace StardewArchipelago.Archipelago.Gifting
{
    public class ItemAmount
    {
        public string ItemName { get; set; }
        public int Amount { get; set; }

        public ItemAmount(string itemName, int amount)
        {
            ItemName = itemName;
            Amount = amount;
        }

        public static implicit operator ItemAmount((string, int) tuple) => new(tuple.Item1, tuple.Item2);
    }
}
