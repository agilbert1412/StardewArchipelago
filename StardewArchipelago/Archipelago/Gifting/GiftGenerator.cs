using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Archipelago.Gifting.Net;
using Microsoft.Xna.Framework;
using StardewArchipelago.Stardew;
using StardewValley;
using Object = StardewValley.Object;

namespace StardewArchipelago.Archipelago.Gifting
{
    public class GiftGenerator
    {
        private StardewItemManager _itemManager;

        public GiftGenerator(StardewItemManager itemManager)
        {
            _itemManager = itemManager;
        }

        public bool TryCreateGiftItem(Object giftObject, out GiftItem giftItem, out GiftTrait[] traits)
        {
            giftItem = null;
            traits = null;
            if (giftObject == null)
            {
                Game1.chatBox?.addMessage($"You must hold an item in your hand to gift it", Color.Gold);
                return false;
            }
            
            if (!_itemManager.ObjectExists(giftObject.Name) || giftObject.questItem.Value)
            {
                Game1.chatBox?.addMessage($"{giftObject.Name} cannot be gifted to other players", Color.Gold);
                return false;
            }

            giftItem = new GiftItem(giftObject.Name, giftObject.Stack, giftObject.salePrice() * BankHandler.EXCHANGE_RATE);
            traits = GenerateGiftTraits(giftObject);
            return true;
        }

        private GiftTrait[] GenerateGiftTraits(Object giftObject)
        {
            return Array.Empty<GiftTrait>();
        }
    }
}
