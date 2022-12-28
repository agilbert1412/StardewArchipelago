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

        public ItemParser(StardewItemManager itemManager, UnlockManager unlockManager)
        {
            _itemManager = itemManager;
            _unlockManager = unlockManager;
        }

        public Action GetActionToProcessItem(string itemName, int numberReceived, int numberNewReceived)
        {
            var itemIsResourcePack = TryParseResourcePack(itemName, out var stardewItemName, out var resourcePackAmount);
            if (itemIsResourcePack)
            {
                return () => GiveResourcePackToFarmer(stardewItemName, resourcePackAmount * numberNewReceived);
            }
            else
            {
                return HandleArchipelagoUnlock(itemName, numberReceived);
            }
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

            if (stardewItemName == "Money")
            {
                GiveGoldToPlayer(player, resourcePackAmount);
                return;
            }

            var item = _itemManager.GetItemByName(stardewItemName);
            var stardewItem = new StardewValley.Object(item.Id, resourcePackAmount);
            GiveResourcePackToPlayer(player, stardewItem);
        }

        private void GiveGoldToPlayer(Farmer player, int amount)
        {
            player.addUnearnedMoney(amount);
        }

        private void GiveResourcePackToPlayer(Farmer player, StardewValley.Object item)
        {
            player.addItemByMenuIfNecessary(item);
        }

        private Action HandleArchipelagoUnlock(string itemName, int numberReceived)
        {
            return _unlockManager.GetUnlockProcess(itemName, numberReceived);
        }
    }
}
