using System.Collections.Generic;
using System.Linq;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Stardew;
using StardewValley;

namespace StardewArchipelago.Items
{
    internal class ItemManager
    {
        private ArchipelagoClient _archipelago;
        private ItemParser _itemParser;
        private Dictionary<long, int> _itemsAlreadyProcessed;

        public ItemManager(ArchipelagoClient archipelago, StardewItemManager itemManager, UnlockManager unlockManager, SpecialItemManager specialItemManager, Dictionary<long, int> itemsAlreadyProcessed)
        {
            _archipelago = archipelago;
            _itemParser = new ItemParser(itemManager, unlockManager, specialItemManager);
            _itemsAlreadyProcessed = itemsAlreadyProcessed.ToDictionary(x => x.Key, x => x.Value);
        }

        public void RegisterAllUnlocks()
        {
            var allReceivedItems = _archipelago.GetAllReceivedItems();
            foreach (var (itemId, numberReceived) in allReceivedItems)
            {
                var itemName = _archipelago.GetItemName(itemId);
                _itemParser.ProcessUnlockWithoutGivingNewItems(itemName, numberReceived);
            }
        }

        public void ReceiveAllNewItems()
        {
            var allReceivedItems = _archipelago.GetAllReceivedItems();
            var receivedItemsNotProcessedYet = allReceivedItems.Where(x => !_itemsAlreadyProcessed.ContainsKey(x.Key) || _itemsAlreadyProcessed[x.Key] < x.Value).ToArray();

            if (!receivedItemsNotProcessedYet.Any())
            {
                return;
            }

            var itemsToGiveFarmer = new List<Item>();

            foreach (var (idToReceive, numberReceived) in receivedItemsNotProcessedYet)
            {
                var itemToReceive = ReceiveNewItem(idToReceive, numberReceived);
                if (itemToReceive == null)
                {
                    continue;
                }

                itemsToGiveFarmer.Add(itemToReceive);
            }

            GiveItemsToFarmerByMenuIfNecessaryAndDropRemainderOnTheFloor(itemsToGiveFarmer);
        }

        private static void GiveItemsToFarmerByMenuIfNecessaryAndDropRemainderOnTheFloor(List<Item> itemsToGiveFarmer)
        {
            if (!itemsToGiveFarmer.Any())
            {
                return;
            }

            if (itemsToGiveFarmer.Count == 1)
            {
                Game1.player.addItemByMenuIfNecessaryElseHoldUp(itemsToGiveFarmer.First());
                return;
            }

            var itemsToDrop = new List<Item>();
            const int maxItemsToGive = 36;
            if (itemsToGiveFarmer.Count() > maxItemsToGive)
            {
                itemsToDrop.AddRange(itemsToGiveFarmer.Skip(maxItemsToGive));
                itemsToGiveFarmer = itemsToGiveFarmer.Take(maxItemsToGive).ToList();
            }

            Game1.player.addItemsByMenuIfNecessary(itemsToGiveFarmer);

            foreach (var itemToDrop in itemsToDrop)
            {
                if (itemToDrop == null)
                {
                    continue;
                }

                Game1.createItemDebris(itemToDrop, Game1.player.getStandingPosition(), Game1.player.FacingDirection);
            }
        }

        private Item ReceiveNewItem(long itemArchipelagoId, int numberReceived)
        {
            if (!_itemsAlreadyProcessed.ContainsKey(itemArchipelagoId))
            {
                _itemsAlreadyProcessed.Add(itemArchipelagoId, 0);
            }

            var numberNewReceived = numberReceived - _itemsAlreadyProcessed[itemArchipelagoId];
            var itemName = _archipelago.GetItemName(itemArchipelagoId);
            var itemToReceive = _itemParser.ProcessItem(itemName, numberReceived, numberNewReceived);

            _itemsAlreadyProcessed[itemArchipelagoId] = numberReceived;
            return itemToReceive;
        }

        public Dictionary<long, int> GetAllItemsAlreadyProcessed()
        {
            return _itemsAlreadyProcessed.ToDictionary(x => x.Key, x => x.Value);
        }
    }
}
