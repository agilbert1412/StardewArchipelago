using System.Linq;
using System.Reflection;

namespace StardewArchipelago.Constants
{
    public static class TriggerActionProvider
    {
        public static readonly string TRAVELING_MERCHANT_PURCHASE = CreateId("TravelingMerchantPurchase");
        

        private static string CreateId(string name)
        {
            return $"{ModEntry.Instance.ModManifest.UniqueID}.TriggerAction.{name}";
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
