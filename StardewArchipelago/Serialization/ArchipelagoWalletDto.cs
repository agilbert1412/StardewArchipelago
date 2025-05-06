namespace StardewArchipelago.Serialization
{
    public class ArchipelagoWalletDto
    {
        public int StarTokens { get; set; }
        public int Blood { get; set; }
        public int Energy { get; set; }
        public int Time { get; set; }
        public int Cookies { get; set; }

        public ArchipelagoWalletDto()
        {
            StarTokens = 0;
        }
    }
}
