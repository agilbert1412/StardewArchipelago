using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Stardew;
using StardewValley;

namespace StardewArchipelago.Items
{
    public class ItemParser
    {
        private const string RESOURCE_PACK_PREFIX = "Resource Pack: ";
        
        private StardewItemManager _itemManager;
        private UnlockManager _unlockManager;
        private SpecialItemManager _specialItemManager;

        public ItemParser(StardewItemManager itemManager, UnlockManager unlockManager, SpecialItemManager specialItemManager)
        {
            _itemManager = itemManager;
            _unlockManager = unlockManager;
            _specialItemManager = specialItemManager;
        }

        public Item ProcessItem(string itemName, int numberReceived, int numberNewReceived)
        {
            var itemIsResourcePack = TryParseResourcePack(itemName, out var stardewItemName, out var resourcePackAmount);
            if (itemIsResourcePack)
            {
                return GetResourcePack(stardewItemName, resourcePackAmount * numberNewReceived);
            }

            if (_unlockManager.IsUnlock(itemName))
            {
                _unlockManager.PerformUnlock(itemName, numberReceived);
                return null;
            }

            if (_specialItemManager.IsUnlock(itemName))
            {
                _specialItemManager.PerformUnlock(itemName, numberNewReceived);
                return null;
            }

            if (_specialItemManager.IsItem(itemName))
            {
                return _specialItemManager.GetSpecialItem(itemName);
            }

            if (_itemManager.ItemExists(itemName))
            {
                return GetSingleItem(itemName);
            }

            throw new ArgumentException($"Could not process item {itemName}");
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

        private Item GetSingleItem(string stardewItemName)
        {
            var item = _itemManager.GetItemByName(stardewItemName);
            return item.PrepareForGivingToFarmer();
        }

        private Item GetResourcePack(string stardewItemName, int resourcePackAmount)
        {
            var player = Game1.player;

            if (stardewItemName == "Money")
            {
                GiveGoldToPlayer(player, resourcePackAmount);
                return null;
            }

            var item = _itemManager.GetItemByName(stardewItemName);
            return item.PrepareForGivingToFarmer(resourcePackAmount);
        }

        private void GiveGoldToPlayer(Farmer player, int amount)
        {
            player.addUnearnedMoney(amount);
        }
    }
}
