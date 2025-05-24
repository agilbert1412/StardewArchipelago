using System.Collections.Generic;

namespace StardewArchipelago.Constants
{
    public static class MemeIDProvider
    {
        public static readonly string BLOOD = IDProvider.CreateId("Blood");
        public static readonly string ENERGY = IDProvider.CreateId("Energy");
        public static readonly string TIME = IDProvider.CreateId("Time");
        public static readonly string CLIC = IDProvider.CreateId("Clic");
        public static readonly string COOKIES_CLICKS = IDProvider.CreateId("CookieClics");
        public static readonly string DEATH = IDProvider.CreateId("Death");
        public static readonly string STEP = IDProvider.CreateId("Step");
        public static readonly string TIME_ELAPSED = IDProvider.CreateId("TimeElapsed");
        public static readonly string DEAD_CROP = IDProvider.CreateId("DeadCrop");
        public static readonly string DEAD_PUMPKIN = IDProvider.CreateId("DeadPumpkin");
        public static readonly string CHILD = IDProvider.CreateId("Child");
        public static readonly string MISSED_FISH = IDProvider.CreateId("MissedFish");
        public static readonly string HONEYWELL = IDProvider.CreateId("HoneyInWell");
        public static readonly string BANK_MONEY = IDProvider.CreateId("BankMoney");
        public static readonly string SLEEP_DAYS = IDProvider.CreateId("SleepDays");
        public static readonly string DEATHLINKS = IDProvider.CreateId("DeathLinks");

        public static readonly string WORN_BOOTS = IDProvider.CreateId("WornBoots");
        public static readonly string WORN_SHIRT = IDProvider.CreateId("WornShirt");
        public static readonly string WORN_PANTS = IDProvider.CreateId("WornPants");
        public static readonly string WORN_HAT = IDProvider.CreateId("WornHat");
        public static readonly string WORN_LEFT_RING = IDProvider.CreateId("WornLeftRing");
        public static readonly string WORN_RIGHT_RING = IDProvider.CreateId("WornRightRing");

        public static readonly string FUN_TRAP = IDProvider.CreateId("FunTrap");

        public static readonly Dictionary<string, string> MemeItemIds = new()
        {
            { "Worn Boots", WORN_BOOTS },
            { "Worn Shirt", WORN_SHIRT },
            { "Worn Pants", WORN_PANTS },
            { "Worn Hat", WORN_HAT },
            { "Worn Left Ring", WORN_LEFT_RING },
            { "Worn Right Ring", WORN_RIGHT_RING },
            { "Fun Trap", FUN_TRAP },
        };

        public static readonly Dictionary<string, string> MemeItemNames = new()
        {
            { WORN_BOOTS, "Worn Boots" },
            { WORN_SHIRT, "Worn Shirt" },
            { WORN_PANTS, "Worn Pants" },
            { WORN_HAT, "Worn Hat" },
            { WORN_LEFT_RING, "Worn Left Ring" },
            { WORN_RIGHT_RING, "Worn Right Ring" },
            { FUN_TRAP, "Fun Trap" },
        };
    }
}
