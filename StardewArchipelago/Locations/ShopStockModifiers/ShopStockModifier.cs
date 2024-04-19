using System;
using System.Collections.Generic;
using Force.DeepCloner;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.GameData.Shops;

namespace StardewArchipelago.Locations.ShopStockModifiers
{
    public abstract class ShopStockModifier
    {
        protected IMonitor _monitor;
        protected IModHelper _helper;
        protected ArchipelagoClient _archipelago;
        protected StardewItemManager _stardewItemManager;

        public ShopStockModifier(IMonitor monitor, IModHelper helper, ArchipelagoClient archipelago, StardewItemManager stardewItemManager)
        {
            _monitor = monitor;
            _helper = helper;
            _archipelago = archipelago;
            _stardewItemManager = stardewItemManager;
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

            if (apShopItem.Price <= 0 && string.IsNullOrWhiteSpace(apShopItem.TradeItemId))
            {
                apShopItem.Price = _stardewItemManager.GetItemByQualifiedId(item.ItemId)?.SellPrice ?? throw new Exception($"Could not find price for purchasable location {location}");
            }

            return apShopItem;
        }

        protected void ReplaceWithArchipelagoCondition(ShopItemData shopItem, string item, int amount = 1)
        {
            if (string.IsNullOrWhiteSpace(item))
            {
                return;
            }

            shopItem.Condition = GameStateConditionProvider.CreateHasReceivedItemCondition(item, amount);
        }
    }
}
