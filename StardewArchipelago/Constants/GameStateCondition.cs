using Newtonsoft.Json.Linq;
using System.Reflection;

namespace StardewArchipelago.Constants
{
    public static class GameStateCondition
    {
        public static readonly string HAS_RECEIVED_ITEM = CreateId("HasReceivedItem");
        public static readonly string HAS_RECEIVED_ITEM_EXACT_AMOUNT = CreateId("HasReceivedItemExactAmount");
        public static readonly string HAS_STOCK_SIZE = CreateId("HasCartStockSize");
        public static readonly string FOUND_ARTIFACT = CreateId("FoundArtifact");
        public static readonly string FOUND_MINERAL = CreateId("FoundMineral");
        public static readonly string CURRENT_MINE_FLOOR = CreateId("CurrentMineFloor");
        public static readonly string HAS_CRAFTING_RECIPE = "PLAYER_HAS_CRAFTING_RECIPE";
        public static readonly string HAS_COOKING_RECIPE = "PLAYER_HAS_COOKING_RECIPE";
        public static readonly string LOCATION_SEASON = "LOCATION_SEASON";
        public static readonly string IS_PASSIVE_FESTIVAL_OPEN = "IS_PASSIVE_FESTIVAL_OPEN";
        public static readonly string PLAYER_SPECIAL_ORDER_RULE_ACTIVE = "PLAYER_SPECIAL_ORDER_RULE_ACTIVE";
        public static readonly string WEATHER = "WEATHER";

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
