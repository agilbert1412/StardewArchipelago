using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants.Locations;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Shops;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public class CraftingRecipePurchaseStockModifier : ShopStockModifier
    {
        public CraftingRecipePurchaseStockModifier(IMonitor monitor, IModHelper helper, ArchipelagoClient archipelago) : base(monitor, helper, archipelago)
        {
            _monitor = monitor;
            _helper = helper;
            _archipelago = archipelago;
        }

        public override void OnShopStockRequested(object sender, AssetRequestedEventArgs e)
        {
            if (_archipelago.SlotData.Craftsanity == Craftsanity.None)
            {
                return;
            }

            if (!AssetIsShops(e))
            {
                return;
            }

            e.Edit(asset =>
                {
                    var shopsData = asset.AsDictionary<string, ShopData>().Data;

                    foreach (var (shopId, shopData) in shopsData)
                    {
                        ReplaceCraftingRecipesWithCraftsanityChecks(shopId, shopData);
                    }
                },
                AssetEditPriority.Late
            );
        }

        private void ReplaceCraftingRecipesWithCraftsanityChecks(string shopId, ShopData shopData)
        {
            var itemsData = DataLoader.Objects(Game1.content);
            for (var i = shopData.Items.Count - 1; i >= 0; i--)
            {
                var item = shopData.Items[i];
                var itemData = itemsData[item.ItemId];
                if (!item.IsRecipe || !CraftingRecipe.craftingRecipes.ContainsKey(itemData.Name))
                {
                    continue;
                }

                var location = $"{itemData.Name}{Suffix.CRAFTSANITY}";
                var apShopItem = CreateArchipelagoLocation(item, location);
                shopData.Items.RemoveAt(i);
                shopData.Items.Insert(i, apShopItem);
            }
        }
    }
}
