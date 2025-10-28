using System.Collections.Generic;
using StardewArchipelago.Constants.Locations;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.GameData.Shops;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.Utilities.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants;

namespace StardewArchipelago.Locations.ShopStockModifiers
{
    public class CataloguesShopStockModifier : ShopStockModifier
    {
        private static readonly Dictionary<string, string> CATALOGUE_NAMES = new()
        {
            {QualifiedItemIds.CATALOGUE, "Catalogue"},
            {QualifiedItemIds.FURNITURE_CATALOGUE, "Furniture Catalogue"},
            {QualifiedItemIds.JOJA_CATALOGUE, "Joja Furniture Catalogue"},
            {QualifiedItemIds.JUNIMO_CATALOGUE, "Junimo Catalogue"},
            {QualifiedItemIds.RETRO_CATALOGUE, "Retro Catalogue"},
            {QualifiedItemIds.TRASH_CATALOGUE, "Trash Catalogue"},
            {QualifiedItemIds.WIZARD_CATALOGUE, "Wizard Catalogue"},
        };

        public CataloguesShopStockModifier(ILogger logger, IModHelper helper, StardewArchipelagoClient archipelago, StardewItemManager stardewItemManager) : base(logger, helper, archipelago, stardewItemManager)
        {
        }

        public override void OnShopStockRequested(object sender, AssetRequestedEventArgs e)
        {
            if (!_archipelago.SlotData.IncludeEndgameLocations)
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
                        ReplaceCataloguesWithEndgameChecks(shopId, shopData);
                    }
                },
                AssetEditPriority.Late
            );
        }

        private void ReplaceCataloguesWithEndgameChecks(string shopId, ShopData shopData)
        {
            for (var i = shopData.Items.Count - 1; i >= 0; i--)
            {
                var item = shopData.Items[i];

                if (!CATALOGUE_NAMES.ContainsKey(item.Id))
                {
                    continue;
                }

                string catalogueName;
                if (!CATALOGUE_NAMES.TryGetValue(item.Id, out catalogueName))
                {
                    catalogueName = CATALOGUE_NAMES[item.ItemId];
                }
                
                var locationName = $"{Prefix.PURCHASE}{catalogueName}";

                item.Condition = item.Condition.AddCondition(GameStateConditionProvider.CreateHasReceivedItemCondition(catalogueName));
                
                // shopData.Items.RemoveAt(i);

                var apShopItem = CreateArchipelagoLocation(item, locationName);
                var ccCondition = "IS_COMMUNITY_CENTER_COMPLETE";
                if (apShopItem.Condition.Contains(ccCondition))
                {
                    apShopItem.Condition = item.Condition.Replace(ccCondition, GameStateConditionProvider.CreateHasReceivedItemCondition(APItem.FOREST_MAGIC));
                }
                shopData.Items.Insert(i, apShopItem);
            }
        }
    }
}
