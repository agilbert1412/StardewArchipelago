using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Stardew;
using StardewValley;

namespace StardewArchipelago.Items
{
    internal class ItemManager
    {
        private const string RESOURCE_PACK_PREFIX = "Resource Pack: ";

        private ArchipelagoClient _archipelago;
        private ItemParser _itemParser;
        private Dictionary<long, int> _itemsAlreadyProcessed;

        public ItemManager(ArchipelagoClient archipelago, StardewItemManager itemManager, UnlockManager unlockManager, Dictionary<long, int> itemsAlreadyProcessed)
        {
            _archipelago = archipelago;
            _itemParser = new ItemParser(itemManager, unlockManager);
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
            var actionToProcessItem = _itemParser.GetActionToProcessItem(itemName, numberReceived, numberNewReceived);
            actionToProcessItem();

            _itemsAlreadyProcessed[itemArchipelagoId] = numberReceived;
        }

        public Dictionary<long, int> GetAllItemsAlreadyProcessed()
        {
            return _itemsAlreadyProcessed.ToDictionary(x => x.Key, x => x.Value);
        }
    }
}
