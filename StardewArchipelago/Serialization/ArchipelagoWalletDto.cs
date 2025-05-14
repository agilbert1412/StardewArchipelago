using System.Collections.Generic;

namespace StardewArchipelago.Serialization
{
    public class ArchipelagoWalletDto
    {
        public int StarTokens { get; set; }
        public int Blood { get; set; }
        public int Energy { get; set; }
        public int Time { get; set; }
        public int Deaths { get; set; }
        public CookieClickerDto CookieClicker { get; set; }
        public Dictionary<string, int> DeadCropsById { get; set; }
        public Dictionary<string, int> UncaughtFishById { get; set; }

        public ArchipelagoWalletDto()
        {
            StarTokens = 0;
            Blood = 0;
            Energy = 0;
            Time = 0;
            Deaths = 0;
            CookieClicker = new CookieClickerDto();
            DeadCropsById = new Dictionary<string, int>();
            UncaughtFishById = new Dictionary<string, int>();
        }
    }
}
