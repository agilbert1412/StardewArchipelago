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
        public static readonly string COOKIES_CLICKING = IDProvider.CreateId("CookiesClicking");
        public static readonly string DEATH = IDProvider.CreateId("Death");
        public static readonly string STEP = IDProvider.CreateId("Step");

        public static readonly string WORN_BOOTS = IDProvider.CreateId("Worn Boots");
        public static readonly string WORN_SHIRT = IDProvider.CreateId("Worn Shirt");
        public static readonly string WORN_PANTS = IDProvider.CreateId("Worn Pants");
        public static readonly string WORN_HAT = IDProvider.CreateId("Worn Hat");
        public static readonly string WORN_RING = IDProvider.CreateId("Worn Ring");

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
