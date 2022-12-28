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
        private Dictionary<long, int> _itemsAlreadyProcessed;

        public ItemManager(ArchipelagoClient archipelago, StardewItemManager itemManager, UnlockManager unlockManager, Dictionary<long, int> itemsAlreadyProcessed)
        {
            _archipelago = archipelago;
            _itemManager = itemManager;
            _unlockManager = unlockManager;
            _itemsAlreadyProcessed = itemsAlreadyProcessed.ToDictionary(x => x.Key, x => x.Value);
        }

        public void ReceiveAllNewItems()
        {
            var allReceivedItems = _archipelago.GetAllReceivedItems();
            var receivedItemsNotProcessedYet = allReceivedItems.Where(x => !_itemsAlreadyProcessed.ContainsKey(x.Key) || _itemsAlreadyProcessed[x.Key] < x.Value);

            foreach (var (itemToReceive, numberReceived) in receivedItemsNotProcessedYet)
            {
                ReceiveNewItem(itemToReceive, numberReceived);
            }
        }

        private void ReceiveNewItem(long itemArchipelagoId, int numberReceived)
        {
            if (!_itemsAlreadyProcessed.ContainsKey(itemArchipelagoId))
            {
                _itemsAlreadyProcessed.Add(itemArchipelagoId, 0);
            }

            var numberNewReceived = numberReceived - _itemsAlreadyProcessed[itemArchipelagoId];
            var itemName = _archipelago.GetItemName(itemArchipelagoId);
            var itemIsResourcePack = TryParseResourcePack(itemName, out var stardewItemName, out var resourcePackAmount);
            if (itemIsResourcePack)
            {
                GiveResourcePackToFarmer(stardewItemName, resourcePackAmount * numberNewReceived);
            }
            else
            {
                HandleArchipelagoUnlock(itemName, numberReceived);
            }

            _itemsAlreadyProcessed[itemArchipelagoId] = numberReceived;
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
            var player = Game1.player;

            if (stardewItemName == "Gold")
            {
                GiveGoldToPlayer(player, resourcePackAmount);
                return;
            }

            var item = _itemManager.GetItemByName(stardewItemName);
            var stardewItem = new StardewValley.Object(item.Id, resourcePackAmount);
            GiveResourcePackToPlayer(player, stardewItem);
            // SpawnResourcePackOnGround(player, stardewItem);
        }

        private void GiveGoldToPlayer(Farmer player, int amount)
        {
            player.addUnearnedMoney(amount);
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

        private void HandleArchipelagoUnlock(string itemName, int numberReceived)
        {
            _unlockManager.DoUnlock(itemName, numberReceived);
        }

        public Dictionary<long, int> GetAllItemsAlreadyProcessed()
        {
            return _itemsAlreadyProcessed.ToDictionary(x => x.Key, x => x.Value);
        }
    }
}
