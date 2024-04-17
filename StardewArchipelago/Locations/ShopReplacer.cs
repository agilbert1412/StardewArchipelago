﻿using System;
using System.Collections.Generic;
using System.Linq;
using Archipelago.MultiClient.Net.Models;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using Object = StardewValley.Object;

namespace StardewArchipelago.Locations
{
    public class ShopReplacer
    {
        private IMonitor _monitor;
        private IModHelper _modHelper;
        private ArchipelagoClient _archipelago;
        private LocationChecker _locationChecker;

        public ShopReplacer(IMonitor monitor, IModHelper modHelper, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _monitor = monitor;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        public void PlaceShopRecipeCheck(Dictionary<ISalable, ItemStockInformation> itemPriceAndStock, string apLocation, string recipeItemName, Hint[] myActiveHints, ItemStockInformation price, bool removeOriginal = true)
        {
            if (removeOriginal)
            {
                RemoveShopRecipe(itemPriceAndStock, recipeItemName);
            }

            AddArchipelagoCheckToStock(itemPriceAndStock, apLocation, myActiveHints, price);
        }

        private static void RemoveShopRecipe(Dictionary<ISalable, ItemStockInformation> itemPriceAndStock, string recipeItemName)
        {
            foreach (var itemOnSale in itemPriceAndStock.Keys.ToArray())
            {
                if (itemOnSale is not Object salableObject || !salableObject.IsRecipe || !salableObject.Name.Contains(recipeItemName))
                {
                    continue;
                }

                itemPriceAndStock.Remove(itemOnSale);
            }
        }

        public void ReplaceShopItem(Dictionary<ISalable, ItemStockInformation> itemPriceAndStock, string apLocation, Func<Object, bool> conditionToMeet, Hint[] myActiveHints)
        {
            foreach (var itemOnSale in itemPriceAndStock.Keys.ToArray())
            {
                if (itemOnSale is not Object salableObject || !conditionToMeet(salableObject))
                {
                    continue;
                }

                ReplaceShopItem(itemPriceAndStock, itemOnSale, apLocation, myActiveHints, salableObject);
                return;
            }
        }

        public void ReplaceShopItem(Dictionary<ISalable, ItemStockInformation> itemPriceAndStock, ISalable itemOnSale, string apLocation, Func<Object, bool> conditionToMeet, Hint[] myActiveHints)
        {
            if (itemOnSale is not Object salableObject || !conditionToMeet(salableObject))
            {
                return;
            }

            ReplaceShopItem(itemPriceAndStock, itemOnSale, apLocation, myActiveHints, salableObject);
        }

        private void ReplaceShopItem(Dictionary<ISalable, ItemStockInformation> itemPriceAndStock, ISalable itemOnSale, string apLocation, Hint[] myActiveHints, Object salableObject)
        {
            var apName = BigCraftable.ConvertToApName(salableObject);
            var shouldRemoveOriginal = apName == "Stardrop" || !_archipelago.HasReceivedItem(apName);

            ReplaceShopItem(itemPriceAndStock, itemOnSale, apLocation, shouldRemoveOriginal, myActiveHints);
        }

        public void ReplaceShopItem(Dictionary<ISalable, ItemStockInformation> itemPriceAndStock, ISalable itemOnSale, string apLocation, Func<Furniture, bool> conditionToMeet, Hint[] myActiveHints)
        {
            if (itemOnSale is not Furniture salableFurniture || !conditionToMeet(salableFurniture))
            {
                return;
            }

            ReplaceShopItem(itemPriceAndStock, itemOnSale, apLocation, true, myActiveHints);
        }

        public void ReplaceShopItem(Dictionary<ISalable, ItemStockInformation> itemPriceAndStock, ISalable itemOnSale, string apLocation, Func<Hat, bool> conditionToMeet, Hint[] myActiveHints)
        {
            if (itemOnSale is not Hat salableHat || !conditionToMeet(salableHat))
            {
                return;
            }

            ReplaceShopItem(itemPriceAndStock, itemOnSale, apLocation, true, myActiveHints);
        }

        public void ReplaceShopItem(Dictionary<ISalable, ItemStockInformation> itemPriceAndStock, ISalable itemOnSale, string apLocation, string itemName, Hint[] myActiveHints)
        {
            ReplaceShopItem(itemPriceAndStock, itemOnSale, apLocation, item => item.Name == itemName, myActiveHints);
        }

        public void ReplaceShopItem(Dictionary<ISalable, ItemStockInformation> itemPriceAndStock, ISalable itemOnSale, string apLocation, Func<Item, bool> conditionToMeet, Hint[] myActiveHints)
        {
            if (itemOnSale is not Item salableItem || !conditionToMeet(salableItem))
            {
                return;
            }

            ReplaceShopItem(itemPriceAndStock, itemOnSale, apLocation, true, myActiveHints);
        }

        private void ReplaceShopItem(Dictionary<ISalable, ItemStockInformation> itemPriceAndStock, ISalable itemOnSale, string apLocationName, bool removeOriginal, Hint[] myActiveHints)
        {
            var itemPrice = itemPriceAndStock[itemOnSale].Price;
            if (removeOriginal)
            {
                itemPriceAndStock.Remove(itemOnSale);
            }

            var itemStockInfo = new ItemStockInformation(itemPrice, 1);
            AddArchipelagoCheckToStock(itemPriceAndStock, apLocationName, myActiveHints, itemStockInfo);
        }

        private void AddArchipelagoCheckToStock(Dictionary<ISalable, ItemStockInformation> itemPriceAndStock, string apLocationName, Hint[] myActiveHints, ItemStockInformation itemStockInfo)
        {
            if (!_locationChecker.IsLocationMissing(apLocationName))
            {
                return;
            }

            if (itemPriceAndStock.Keys.Any(x => (x is PurchaseableArchipelagoLocation apLocation) && apLocation.LocationName.Equals(apLocationName)))
            {
                return;
            }

            var purchaseableLocation = new PurchaseableArchipelagoLocation(apLocationName, _monitor, _modHelper, _locationChecker, _archipelago, myActiveHints);
            itemPriceAndStock.Add(purchaseableLocation, itemStockInfo);
        }

        private bool IsRarecrow(Object item)
        {
            return item.IsScarecrow() &&
                   item.Name == "Rarecrow";
        }

        public bool IsRarecrow(Object item, int rarecrowNumber)
        {
            if (!IsRarecrow(item))
            {
                return false;
            }

            return rarecrowNumber switch
            {
                1 => item.ParentSheetIndex == 110,
                2 => item.ParentSheetIndex == 113,
                3 => item.ParentSheetIndex == 126,
                4 => item.ParentSheetIndex == 136,
                5 => item.ParentSheetIndex == 137,
                6 => item.ParentSheetIndex == 138,
                7 => item.ParentSheetIndex == 139,
                8 => item.ParentSheetIndex == 140,
                _ => false,
            };
        }
    }
}
