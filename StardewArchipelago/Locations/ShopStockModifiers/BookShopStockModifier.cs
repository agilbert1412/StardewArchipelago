using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.GameData.Shops;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago;
using StardewValley.GameData;

namespace StardewArchipelago.Locations.ShopStockModifiers
{
    public class BookShopStockModifier : ShopStockModifier
    {
        public BookShopStockModifier(ILogger logger, IModHelper helper, StardewArchipelagoClient archipelago, StardewItemManager stardewItemManager) : base(logger, helper, archipelago, stardewItemManager)
        {
            _logger = logger;
            _helper = helper;
            _archipelago = archipelago;
        }

        public override void OnShopStockRequested(object sender, AssetRequestedEventArgs e)
        {
            if (!AssetIsShops(e))
            {
                return;
            }

            e.Edit(asset =>
            {
                var shopsData = asset.AsDictionary<string, ShopData>().Data;
                var booksellerShop = shopsData["Bookseller"];
                ApplyPriceMultiplier(booksellerShop);
            },
                AssetEditPriority.Late
            );
        }

        private void ApplyPriceMultiplier(ShopData shopData)
        {
            var multiplier = ModEntry.Instance.Config.BooksellerPriceMultiplier / 100f;
            for (var i = shopData.Items.Count - 1; i >= 0; i--)
            {
                var item = shopData.Items[i];
                var modifier = new QuantityModifier
                {
                    Amount = multiplier,
                    Id = nameof(ModEntry.Instance.Config.BooksellerPriceMultiplier),
                    Modification = QuantityModifier.ModificationType.Multiply,
                };
                item.PriceModifiers.Add(modifier);
            }
        }
    }
}
