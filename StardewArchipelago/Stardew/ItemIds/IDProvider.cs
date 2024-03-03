using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using StardewArchipelago.Locations;

namespace StardewArchipelago.Stardew.ItemIds
{
    public static class IDProvider
    {
        public static readonly string PURCHASEABLE_AP_LOCATION = CreateId<PurchaseableArchipelagoLocation>();
        public static readonly string MONEY = CreateId("Money");
        public static readonly string QI_GEM = CreateId("QiGem");
        public static readonly string QI_COIN = CreateId("QiCoin");
        public static readonly string STAR_TOKEN = CreateId("StarToken");

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
