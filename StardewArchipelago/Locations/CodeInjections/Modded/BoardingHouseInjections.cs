using System;
using System.Collections.Generic;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using Microsoft.Xna.Framework;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

namespace StardewArchipelago.Locations.CodeInjections.Modded
{
    public static class BoardingHouseInjections
    {
        private const string CHEST_1A = "Abandoned Treasure - Floor 1A";
        private const string CHEST_1B = "Abandoned Treasure - Floor 1B";
        private const string CHEST_2A = "Abandoned Treasure - Floor 2A";
        private const string CHEST_2B = "Abandoned Treasure - Floor 2B";
        private const string CHEST_3 = "Abandoned Treasure - Floor 3";
        private const string CHEST_4 = "Abandoned Treasure - Floor 4";
        private const string CHEST_5 = "Abandoned Treasure - Floor 5";
        private static readonly Dictionary<string, string> abandonedMines = new()
        {
            { "Custom_BoardingHouse_AbandonedMine1A", CHEST_1A }, { "Custom_BoardingHouse_AbandonedMine1B", CHEST_1B },
            { "Custom_BoardingHouse_AbandonedMine2A", CHEST_2A }, { "Custom_BoardingHouse_AbandonedMine2B", CHEST_2B },
            { "Custom_BoardingHouse_AbandonedMine3", CHEST_3 }, { "Custom_BoardingHouse_AbandonedMine4", CHEST_4 },
            { "Custom_BoardingHouse_AbandonedMine5", CHEST_5 },
        };

        private static readonly Dictionary<ShopIdentification, PricedItem[]> craftsanityRecipes = new()
        {
            {
                new ShopIdentification("Mine", "Dwarf"), new[]
                {
                    new PricedItem("Pterodactyl Skeleton L", 5000), new PricedItem("Pterodactyl Skeleton M", 5000),
                    new PricedItem("Pterodactyl Skeleton R", 5000), new PricedItem("T-Rex Skeleton L", 5000), new PricedItem("T-Rex Skeleton M", 5000),
                    new PricedItem("T-Rex Skeleton R", 5000), new PricedItem("Neanderthal Skeleton", 5000),
                }
            },
        };

        private static ILogger _logger;
        private static LocationChecker _locationChecker;
        private static StardewArchipelagoClient _archipelago;
        private static ShopMenu _lastShopMenuUpdated = null;

        public static void Initialize(ILogger logger, LocationChecker locationChecker, StardewArchipelagoClient archipelago)
        {
            _logger = logger;
            _locationChecker = locationChecker;
            _archipelago = archipelago;
        }

        // public override void update(GameTime time)
        public static void Update_ReplaceDwarfShopChecks_Postfix(ShopMenu __instance, GameTime time)
        {
            try
            {
                // We only run this once for each menu
                if (_lastShopMenuUpdated == __instance)
                {
                    return;
                }

                _lastShopMenuUpdated = __instance;
                ReplaceCraftsanityRecipes(__instance);

                __instance.forSale = __instance.itemPriceAndStock.Keys.ToList();
                return; //  run original logic
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(Update_ReplaceDwarfShopChecks_Postfix)}:\n{ex}");
                return; // run original logic
            }
        }


        private static void ReplaceCraftsanityRecipes(ShopMenu shopMenu)
        {
            if (_archipelago.SlotData.Craftsanity != Craftsanity.All)
            {
                return;
            }

            foreach (var (shopIdentification, recipes) in craftsanityRecipes)
            {
                if (!shopIdentification.IsCorrectShop(shopMenu))
                {
                    continue;
                }

                foreach (var recipe in recipes)
                {
                    throw new Exception($"{nameof(BoardingHouseInjections)}.{nameof(ReplaceCraftsanityRecipes)} attempted to use the now removed ShopReplacer. It needs to be updated for 1.6");
                    // _shopReplacer.PlaceShopRecipeCheck(shopMenu.itemPriceAndStock, $"{recipe.RecipeName} Recipe", recipe.RecipeName, myActiveHints, recipe.Price);
                }
            }
        }

        public static bool CheckForAction_TreasureChestLocation_Prefix(Chest __instance, Farmer who, bool justCheckingForActivity, ref bool __result)
        {
            try
            {
                var playerLocation = Game1.player.currentLocation.Name;
                if (justCheckingForActivity || !abandonedMines.Keys.Contains(playerLocation))
                {
                    return true; //run original logic
                }
                var currentChest = abandonedMines[playerLocation];
                if (__instance.Items.Count <= 0 || _locationChecker.IsLocationChecked(currentChest))
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }
                who.currentLocation.playSound("openChest");
                if (__instance.synchronized.Value)
                {
                    __instance.GetMutex().RequestLock(() => __instance.openChestEvent.Fire());
                }
                else
                {
                    __instance.performOpenChest();
                }
                var obj = __instance.Items[0];
                __instance.Items[0] = null;
                __instance.Items.RemoveAt(0);
                _locationChecker.AddCheckedLocation(currentChest);
                Game1.playSound("openChest");
                __result = true;
                return false; //don't run original logic (first chest is a check)
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CheckForAction_TreasureChestLocation_Prefix)}:\n{ex}");
                return true;
            }
        }
    }
}
