using System.Collections.Generic;
using Force.DeepCloner;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.GameData.Shops;

namespace StardewArchipelago.Locations.ShopStockModifiers
{
    public abstract class ShopStockModifier
    {
        protected static IMonitor _monitor;
        protected static IModHelper _helper;
        protected static ArchipelagoClient _archipelago;

        public ShopStockModifier(IMonitor monitor, IModHelper helper, ArchipelagoClient archipelago)
        {
            _monitor = monitor;
            _helper = helper;
            _archipelago = archipelago;
        }

        public abstract void OnShopStockRequested(object sender, AssetRequestedEventArgs e);

        protected bool AssetIsShops(AssetRequestedEventArgs e)
        {
            return e.NameWithoutLocale.IsEquivalentTo("Data/Shops");
        }

        protected ShopItemData CreateArchipelagoLocation(ShopItemData item, string location)
        {
            var id = $"{IDProvider.PURCHASEABLE_AP_LOCATION} {location}";
            var apShopItem = item.DeepClone();
            apShopItem.Id = id;
            apShopItem.ItemId = id;
            apShopItem.AvailableStock = 1;
            apShopItem.IsRecipe = false;
            return apShopItem;
        }

        protected void AddArchipelagoCondition(ShopItemData shopItem, string[] existingConditions, string item, int amount = 1)
        {
            var apCondition = GameStateConditionProvider.CreateHasReceivedItemCondition(item, amount);
            var newConditions = new List<string>();
            newConditions.AddRange(existingConditions);
            newConditions.Add(apCondition);

            shopItem.Condition = string.Join(',', newConditions);
        }
    }
}
