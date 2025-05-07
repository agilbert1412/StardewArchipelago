using System.Collections.Generic;
using System.Reflection;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.Locations.InGameLocations;

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

        public static readonly string WORN_BOOTS = IDProvider.CreateId("WornBoots");
        public static readonly string WORN_SHIRT = IDProvider.CreateId("WornShirt");
        public static readonly string WORN_PANTS = IDProvider.CreateId("WornPants");
        public static readonly string WORN_HAT = IDProvider.CreateId("WornHat");
        public static readonly string WORN_RING = IDProvider.CreateId("WornRing");

        public static readonly Dictionary<string, string> MemeItemIds = new()
        {
            {"Worn Boots", WORN_BOOTS},
            {"Worn Shirt", WORN_SHIRT},
            {"Worn Pants", WORN_PANTS},
            {"Worn Hat", WORN_HAT},
            {"Worn Ring", WORN_RING},
        };
    }
}
