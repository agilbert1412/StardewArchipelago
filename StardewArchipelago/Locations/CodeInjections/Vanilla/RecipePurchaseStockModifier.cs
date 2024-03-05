using System;
using System.Collections.Generic;
using System.Linq;
using Archipelago.MultiClient.Net.Models;
using Force.DeepCloner;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants;
using StardewArchipelago.Constants.Locations;
using StardewArchipelago.Constants.Modded;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Shops;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using xTile.Dimensions;
using Object = StardewValley.Object;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public class RecipePurchaseStockModifier : ShopStockModifier
    {
        public RecipePurchaseStockModifier(IMonitor monitor, IModHelper helper, ArchipelagoClient archipelago) : base(monitor, helper, archipelago)
        {
            _monitor = monitor;
            _helper = helper;
            _archipelago = archipelago;
        }

        public override void OnShopStockRequested(object sender, AssetRequestedEventArgs e)
        {
            if (!_archipelago.SlotData.Chefsanity.HasFlag(Chefsanity.Purchases))
            {
                return;
            }

            if (!e.NameWithoutLocale.IsEquivalentTo("Data/Shops"))
            {
                return;
            }

            e.Edit(asset =>
                {
                    var shopsData = asset.AsDictionary<string, ShopData>().Data;

                    foreach (var (shopId, shopData) in shopsData)
                    {
                        ReplaceRecipesWithChefsanityChecks(shopId, shopData);
                    }
                },
                AssetEditPriority.Late
            );
        }

        private void ReplaceRecipesWithChefsanityChecks(string shopId, ShopData shopData)
        {
            string[] shopsWithRecipes = { "Saloon", "ResortBar", "IslandTrade", "VolcanoShop" };
            if (!shopsWithRecipes.Contains(shopId))
            {
                return;
            }

            var itemsData = DataLoader.Objects(Game1.content);
            for (var i = shopData.Items.Count - 1; i >= 0; i--)
            {
                var item = shopData.Items[i];
                var itemData = itemsData[item.ItemId];
                if (!item.IsRecipe || !CraftingRecipe.cookingRecipes.ContainsKey(itemData.Name))
                {
                    continue;
                }

                var location = $"{itemData.Name}{Suffix.CHEFSANITY}";
                var apShopItem = CreateArchipelagoLocation(item, location);
                shopData.Items.RemoveAt(i);
                shopData.Items.Insert(i, apShopItem);
            }
        }
    }
}
