using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewValley;

namespace StardewArchipelago
{
    internal class ItemManager
    {
        private const string RESOURCE_PACK_PREFIX = "Resource Pack: ";

        private ArchipelagoClient _archipelago;
        private StardewItemManager _itemManager;
        private UnlockManager _unlockManager;
        private List<long> _itemsAlreadyProcessed;

        public ItemManager(ArchipelagoClient archipelago, StardewItemManager itemManager, UnlockManager unlockManager, IEnumerable<long> itemsAlreadyProcessed)
        {
            _archipelago = archipelago;
            _itemManager = itemManager;
            _unlockManager = unlockManager;
            _itemsAlreadyProcessed = itemsAlreadyProcessed.ToList();
        }

        public void ReceiveAllNewItems()
        {
            var allReceivedItems = _archipelago.GetAllReceivedItems();
            var receivedItemsNotProcessedYet = allReceivedItems.Where(x => !_itemsAlreadyProcessed.Contains(x));

            foreach (var itemToReceive in receivedItemsNotProcessedYet)
            {
                ReceiveNewItem(itemToReceive);
            }
        }

        private void ReceiveNewItem(long itemArchipelagoId)
        {
            var itemName = _archipelago.GetItemName(itemArchipelagoId);
            var itemIsResourcePack = TryParseResourcePack(itemName, out var stardewItemName, out var resourcePackAmount);
            if (itemIsResourcePack)
            {
                GiveResourcePackToFarmer(stardewItemName, resourcePackAmount);
            }
            else
            {
                HandleArchipelagoUnlock(itemName);
            }
            _itemsAlreadyProcessed.Add(itemArchipelagoId);
        }

        private bool TryParseResourcePack(string apItemName, out string stardewItemName, out int amount)
        {
            stardewItemName = "";
            amount = 0;
            if (!apItemName.StartsWith(RESOURCE_PACK_PREFIX))
            {
                return false;
            }

            var apItemWithoutPrefix = apItemName.Substring(RESOURCE_PACK_PREFIX.Length);
            var parts = apItemWithoutPrefix.Split(" ");
            if (!int.TryParse(parts[0], out amount))
            {
                return false;
            }

            stardewItemName = apItemWithoutPrefix.Substring(apItemWithoutPrefix.IndexOf(" ", StringComparison.Ordinal) + 1);
            return true;
        }

        private void GiveResourcePackToFarmer(string stardewItemName, int resourcePackAmount)
        {
            var item = _itemManager.GetItemByName(stardewItemName);
            var player = Game1.player;
            var stardewItem = new StardewValley.Object(item.Id, resourcePackAmount);
            GiveResourcePackToPlayer(player, stardewItem);
            // SpawnResourcePackOnGround(player, stardewItem);
        }

        private void GiveResourcePackToPlayer(Farmer player, StardewValley.Object item)
        {
            player.addItemByMenuIfNecessary(item);
        }

        private void SpawnResourcePackOnGround(Farmer player, StardewValley.Object item)
        {
            var playerLocation = player.currentLocation;
            playerLocation.dropObject(item, player.GetDropLocation(), Game1.viewport, true, (Farmer)null);
        }

        private void HandleArchipelagoUnlock(string itemName)
        {
            _unlockManager.DoUnlock(itemName);
        }

        public IEnumerable<long> GetAllItemsAlreadyProcessed()
        {
            return _itemsAlreadyProcessed;
        }
    }
}
