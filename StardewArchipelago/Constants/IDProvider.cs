using System.Reflection;
using StardewArchipelago.Locations;

namespace StardewArchipelago.Constants
{
    public static class IDProvider
    {
        public static readonly string PURCHASEABLE_AP_LOCATION = CreateId<PurchaseableArchipelagoLocation>();
        public static readonly string MONEY = CreateId("Money");
        public static readonly string QI_GEM = CreateId("QiGem");
        public static readonly string QI_COIN = CreateId("QiCoin");
        public static readonly string STAR_TOKEN = CreateId("StarToken");
        public static readonly string METAL_DETECTOR_ITEMS = CreateId("MetalDetectorItems");
        public static readonly string TRAVELING_CART_DAILY_CHECK = CreateId("TravelingCartDailyCheck");

        private static string CreateId(string name)
        {
            return $"{ModEntry.Instance.ModManifest.UniqueID}.{name}";
        }

        private static string CreateId(MemberInfo t)
        {
            return CreateId(t.Name);
        }

        private static string CreateId<T>()
        {
            return CreateId(typeof(T));
        }
    }
}
