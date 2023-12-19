using System;
using System.Collections.Generic;
using System.Linq;
using StardewArchipelago.Archipelago.Gifting;
using StardewArchipelago.Extensions;
using StardewValley;
using StardewValley.Objects;
using StardewValley.Tools;
using Object = StardewValley.Object;

namespace StardewArchipelago.Items.Traps
{
    public class InventoryShuffler
    {
        private const double GIFTING_RATE = 0.20;

        private class ItemSlot
        {
            public IList<Item> Inventory { get; set; }
            public int SlotNumber { get; set; }

            public ItemSlot(IList<Item> inventory, int slotNumber)
            {
                Inventory = inventory;
                SlotNumber = slotNumber;
            }

            public void SetItem(Item item)
            {
                while (SlotNumber >= Inventory.Count)
                {
                    Inventory.Add(null);
                }
                Inventory[SlotNumber] = item;
            }
        }

        private const int CRAFTING_CATEGORY = -9;
        private const string CRAFTING_TYPE = "Crafting";
        private GiftSender _giftSender;

        public InventoryShuffler(GiftSender giftSender)
        {
            _giftSender = giftSender;
        }

        public void ShuffleInventories(ShuffleInventoryTarget targets)
        {
            if (targets == ShuffleInventoryTarget.None)
            {
                return;
            }

            var slotsToShuffle = new Dictionary<ItemSlot, Item>();

            AddItemSlotsFromPlayerInventory(slotsToShuffle, targets == ShuffleInventoryTarget.Hotbar);
            if (targets >= ShuffleInventoryTarget.InventoryAndChests)
            {
                AddItemSlotsFromChestsInEntireWorld(slotsToShuffle);
            }

            var random = new Random((int)(Game1.uniqueIDForThisGame + Game1.stats.DaysPlayed));

            if (targets >= ShuffleInventoryTarget.InventoryAndChestsAndFriends)
            {
                SendRandomGifts(slotsToShuffle, random, GIFTING_RATE);
            }

            var allSlots = slotsToShuffle.Keys.ToList();
            var allItems = slotsToShuffle.Values.ToList();
            var allItemsShuffled = allItems.Shuffle(random);

            for (var i = 0; i < allSlots.Count; i++)
            {
                allSlots[i].SetItem(allItemsShuffled[i]);
            }

            foreach (var chest in FindAllChests())
            {
                chest.clearNulls();
            }
        }

        private void SendRandomGifts(Dictionary<ItemSlot, Item> slotsToShuffle, Random random, double giftingRate)
        {
            var giftsToSend = new Dictionary<string, List<Object>>();
            var slotsToClear = new Dictionary<Object, ItemSlot>();

            foreach (var (itemSlot, item) in slotsToShuffle)
            {
                if (item is not Object objectToGift || random.NextDouble() > giftingRate)
                {
                    continue;
                }

                var validTargets = _giftSender.GetAllPlayersThatCanReceiveShuffledItems(objectToGift);
                if (validTargets == null || !validTargets.Any())
                {
                    continue;
                }

                var chosenTarget = validTargets[random.Next(validTargets.Count)];
                if (!giftsToSend.ContainsKey(chosenTarget))
                {
                    giftsToSend.Add(chosenTarget, new List<Object>());
                }

                giftsToSend[chosenTarget].Add(objectToGift);
                slotsToClear.Add(objectToGift, itemSlot);
            }

            var failedToSendGifts = _giftSender.SendShuffleGifts(giftsToSend);

            foreach (var (gift, slot) in slotsToClear)
            {
                if (failedToSendGifts.Contains(gift))
                {
                    continue;
                }

                slotsToShuffle.Remove(slot);
                slot.SetItem(null);
            }
        }

        private static void AddItemSlotsFromPlayerInventory(Dictionary<ItemSlot, Item> slotsToShuffle, bool hotbarOnly)
        {
            var player = Game1.player;
            var maxSlot = hotbarOnly ? 12 : player.MaxItems;
            for (var i = 0; i < maxSlot; i++)
            {
                Item item = null;
                if (player.Items.Count > i && player.Items[i] != null)
                {
                    item = player.Items[i];
                }

                if (item is FishingRod rod && (rod.isReeling || rod.isFishing || rod.pullingOutOfWater))
                {
                    continue;
                }

                var slot = new ItemSlot(player.Items, i);
                slotsToShuffle.Add(slot, item);
            }
        }

        private static void AddItemSlotsFromChestsInEntireWorld(Dictionary<ItemSlot, Item> slotsToShuffle)
        {
            foreach (var chest in FindAllChests())
            {
                AddItemSlotsFromChest(slotsToShuffle, chest);
            }
        }

        private static IEnumerable<Chest> FindAllChests()
        {
            var locations = Game1.locations.ToList();
            foreach (var building in Game1.getFarm().buildings)
            {
                if (building?.indoors.Value == null)
                {
                    continue;
                }
                locations.Add(building.indoors.Value);
            }

            foreach (var gameLocation in locations)
            {
                foreach (var (tile, gameObject) in gameLocation.Objects.Pairs)
                {
                    if (gameObject is not Chest { SpecialChestType: Chest.SpecialChestTypes.None, Category: CRAFTING_CATEGORY, Type: CRAFTING_TYPE } chest || chest.giftbox.Value)
                    {
                        continue;
                    }

                    yield return chest;
                }
            }
        }

        private static void AddItemSlotsFromChest(Dictionary<ItemSlot, Item> slotsToShuffle, Chest chest)
        {
            var capacity = chest.GetActualCapacity();
            for (var i = 0; i < capacity; i++)
            {
                Item item = null;
                if (chest.items.Count > i && chest.items[i] != null)
                {
                    item = chest.items[i];
                }

                var slot = new ItemSlot(chest.items, i);
                slotsToShuffle.Add(slot, item);
            }
        }
    }
}
