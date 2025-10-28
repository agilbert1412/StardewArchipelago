using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Microsoft.Xna.Framework.Input;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants.Locations;
using StardewArchipelago.Stardew;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using KaitoKid.Utilities.Interfaces;
using Microsoft.Xna.Framework;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public static class CollectionsInjections
    {
        private const int SHIPPING_INDEX = 0;

        private static ILogger _logger;
        private static StardewArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;
        private static StardewItemManager _stardewItemManager;
        private static NightShippingBehaviors _nightShippingBehaviors;

        public static void Initialize(ILogger logger, StardewArchipelagoClient archipelago, LocationChecker locationChecker, StardewItemManager stardewItemManager, NightShippingBehaviors nightShippingBehaviors)
        {
            _logger = logger;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            _stardewItemManager = stardewItemManager;
            _nightShippingBehaviors = nightShippingBehaviors;
        }

        // public CollectionsPage(int x, int y, int width, int height)
        public static void CollectionsPageConstructor_AddAllShipsanityItems_Postfix(CollectionsPage __instance, int x, int y, int width, int height)
        {
            try
            {
                var shippingCollection = __instance.collections[SHIPPING_INDEX];
                var allShippables = _stardewItemManager.GetAllItemsMatching(x => x.canBeShipped());
                var allActiveShippables = allShippables.Where(x => _locationChecker.LocationExists(GetShipsanityLocationName(x))).ToArray();

                var allShippingComponents = shippingCollection.SelectMany(x => x.ToArray()).Select(x => x.name.Split(" ").First()).ToHashSet();

                foreach (var activeShippable in allActiveShippables)
                {
                    if (allShippingComponents.Contains(activeShippable.ItemId) || allShippingComponents.Contains(activeShippable.QualifiedItemId))
                    {
                        continue;
                    }

                    AddShippingComponent(__instance, shippingCollection, activeShippable, height);
                }

                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CollectionsPageConstructor_AddAllShipsanityItems_Postfix)}:\n{ex}");
                return;
            }
        }

        private static void AddShippingComponent(CollectionsPage collectionsPage, List<List<ClickableTextureComponent>> shippingCollection, Item shippableItem, int collectionsPageHeight)
        {
            var itemId = shippableItem.ItemId;
            // var drawShadow = Game1.player.basicShipped.ContainsKey(itemId);
            var drawShadow = _locationChecker.IsLocationChecked(GetShipsanityLocationName(shippableItem));
            var flag = false;
            var startXOffset = collectionsPage.xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder;
            var startYOffset = collectionsPage.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16;
            var itemsPerRow = 10;
            var itemIndexInPage = shippingCollection.Last().Count;
            var x1 = startXOffset + (itemIndexInPage % itemsPerRow) * 68;
            var y1 = startYOffset + (itemIndexInPage / itemsPerRow) * 68;
            if (y1 > collectionsPage.yPositionOnScreen + collectionsPageHeight - 128)
            {
                shippingCollection.Add(new List<ClickableTextureComponent>());
                itemIndexInPage = 0;
                x1 = startXOffset;
                y1 = startYOffset;
            }
            if (shippingCollection.Count == 0)
            {
                shippingCollection.Add(new List<ClickableTextureComponent>());
            }
            var lastShippingPage = shippingCollection.Last();
            var dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(itemId);
            var itemComponent = new ClickableTextureComponent($"{itemId} {drawShadow} {flag}", new Rectangle(x1, y1, 64, 64), null, "", dataOrErrorItem.GetTexture(), dataOrErrorItem.GetSourceRect(), 4f, drawShadow);
            itemComponent.myID = lastShippingPage.Count;
            itemComponent.rightNeighborID = (lastShippingPage.Count + 1) % itemsPerRow == 0 ? -1 : lastShippingPage.Count + 1;
            itemComponent.leftNeighborID = lastShippingPage.Count % itemsPerRow == 0 ? 7001 : lastShippingPage.Count - 1;
            itemComponent.downNeighborID = y1 + 68 > collectionsPage.yPositionOnScreen + collectionsPageHeight - 128 ? -7777 : lastShippingPage.Count + itemsPerRow;
            itemComponent.upNeighborID = lastShippingPage.Count < itemsPerRow ? 12347 : lastShippingPage.Count - itemsPerRow;
            itemComponent.fullyImmutable = true;
            lastShippingPage.Add(itemComponent);
        }

        private static string GetShipsanityLocationName(Item item)
        {
            return $"{NightShippingBehaviors.SHIPSANITY_PREFIX}{_nightShippingBehaviors.GetNameForShipsanity(item)}";
        }
    }
}
