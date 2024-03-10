using System.Linq;
using System.Reflection;

namespace StardewArchipelago.Constants
{
    public static class GameStateConditionProvider
    {
        public static readonly string HAS_RECEIVED_ITEM = CreateId("HasReceivedItem");
        public static readonly string CART_RANDOM_ITEM_STOCK_CHANCE = CreateId("TRAVELING_CART_RANDOM_ITEM_STOCK_CHANCE");
        public static readonly string CART_EXCLUSIVE_ITEM_STOCK_CHANCE = CreateId("TRAVELING_CART_EXCLUSIVE_ITEM_STOCK_CHANCE");

        public static string CreateHasReceivedItemCondition(string itemName, int amount = 1)
        {
            if (amount < 1)
            {
                return string.Empty;
            }

            var arguments = amount == 1 ? new[] { itemName } : new[] { itemName, amount.ToString() };
            return CreateCondition(HAS_RECEIVED_ITEM, arguments);
        }

        public static string CreateCondition(string condition, string[] arguments)
        {
            return !arguments.Any() ? condition : $"{condition} {string.Join(' ', arguments)}";
        }

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
