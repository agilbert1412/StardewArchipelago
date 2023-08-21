using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Archipelago.Gifting.Net;
using StardewArchipelago.Items.Mail;
using StardewArchipelago.Stardew;
using StardewModdingAPI;

namespace StardewArchipelago.Archipelago.Gifting
{
    public class GiftReceiver
    {
        private IMonitor _monitor;
        private ArchipelagoClient _archipelago;
        private IGiftingService _giftService;
        private StardewItemManager _itemManager;
        private Mailman _mail;
        private GiftProcessor _giftProcessor;

        public GiftReceiver(IMonitor monitor, ArchipelagoClient archipelago, IGiftingService giftService, StardewItemManager itemManager, Mailman mail)
        {
            _monitor = monitor;
            _archipelago = archipelago;
            _giftService = giftService;
            _itemManager = itemManager;
            _mail = mail;
            _giftProcessor = new GiftProcessor(monitor, archipelago, itemManager);
        }

        public void ReceiveAllGifts()
        {
            var gifts = _giftService.GetAllGiftsAndEmptyGiftbox();

            foreach (var (id, gift) in gifts)
            {
                ReceiveGiftTomorrow(gift);
            }
        }

        private void ReceiveGiftTomorrow(Gift gift)
        {
            if (!_giftProcessor.TryMakeStardewItem(gift, out var item, out var amount))
            {
                if (!gift.IsRefund)
                {
                    _giftService.RefundGift(gift);
                }
            }
            
            var mailKey = GetMailKey(gift);
            var embed = GetEmbed(item, amount);
            _mail.SendArchipelagoGiftMail(mailKey, gift.SenderName, embed);
        }

        private string GetEmbed(StardewItem item, int amount)
        {
            if (item == null || amount <= 0)
            {
                return "";
            }

            return $"%item object {item.Id} {amount} %%";
        }

        private string GetMailKey(Gift gift)
        {
            return $"APGift;{gift.ID}";
        }
    }
}
