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
        public static readonly string HAS_CRAFTING_RECIPE = "PLAYER_HAS_CRAFTING_RECIPE";
        public static readonly string HAS_COOKING_RECIPE = "PLAYER_HAS_COOKING_RECIPE";

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
