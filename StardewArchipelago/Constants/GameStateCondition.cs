using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using StardewArchipelago.Constants.Locations;

namespace StardewArchipelago.Constants
{
    public static class GameStateCondition
    {
        public static readonly string HAS_RECEIVED_ITEM = CreateId("HasReceivedItem");
        public static readonly string HAS_STOCK_SIZE = CreateId("HasCartStockSize");
        public static readonly string FOUND_ARTIFACT = CreateId("FoundArtifact");
        public static readonly string FOUND_MINERAL = CreateId("FoundMineral");

        private static string CreateId(string name)
        {
            return $"{ModEntry.Instance.ModManifest.UniqueID}.GameStateCondition.{name}";
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
