using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.Locations.ShopStockModifiers;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.GameData.Shops;

namespace StardewArchipelago.GameModifications.CodeInjections
{
    public class AnimalShopStockModifier : ShopStockModifier
    {
        public AnimalShopStockModifier(ILogger logger, IModHelper helper, StardewArchipelagoClient archipelago, StardewItemManager stardewItemManager) : base(logger, helper, archipelago, stardewItemManager)
        {
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
                    var marnieShopData = shopsData["AnimalShop"];
                    foreach (var shopItemData in marnieShopData.Items)
                    {
                        if (shopItemData.Id != QualifiedItemIds.GOLDEN_EGG)
                        {
                            continue;
                        }

                        shopItemData.Condition = GameStateConditionProvider.CreateHasReceivedItemCondition("Golden Egg");
                    }
                },
                AssetEditPriority.Late
            );
        }
    }
}
