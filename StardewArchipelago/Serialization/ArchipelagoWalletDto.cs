using System.Collections.Generic;

namespace StardewArchipelago.Serialization
{
    public class ArchipelagoWalletDto
    {
        public int StarTokens { get; set; }
        public int Blood { get; set; }
        public int Energy { get; set; }
        public int Time { get; set; }
        public int DeathLinks { get; set; }
        public CookieClickerDto CookieClicker { get; set; }
        public Dictionary<string, int> DeadCropsById { get; set; }
        public Dictionary<string, int> MissedFishById { get; set; }
        public HashSet<string> TouchedItems { get; set; }

        public ArchipelagoWalletDto()
        {
            StarTokens = 0;
            Blood = 0;
            Energy = 0;
            Time = 0;
            DeathLinks = 0;
            CookieClicker = new CookieClickerDto();
            DeadCropsById = new Dictionary<string, int>();
            MissedFishById = new Dictionary<string, int>();
            TouchedItems = new HashSet<string>();
        }
    }
}
