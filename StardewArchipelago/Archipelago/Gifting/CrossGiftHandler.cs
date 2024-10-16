﻿using System.Collections.Generic;
using System.IO;
using Archipelago.Gifting.Net.Service;
using Archipelago.Gifting.Net.Traits;
using Archipelago.Gifting.Net.Utilities.CloseTraitParser;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using StardewArchipelago.Items.Mail;
using StardewArchipelago.Stardew;
using StardewValley;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;

namespace StardewArchipelago.Archipelago.Gifting
{
    internal class CrossGiftHandler : IGiftHandler
    {
        private static readonly string[] _desiredTraits =
        {
            GiftFlag.Speed, GiftFlag.Wood, GiftFlag.Stone, GiftFlag.Consumable, GiftFlag.Food, GiftFlag.Drink,
            GiftFlag.Fish, GiftFlag.Heal, GiftFlag.Metal, GiftFlag.Seed,
        };

        private static ILogger _logger;
        private StardewItemManager _itemManager;
        private Mailman _mail;
        private StardewArchipelagoClient _archipelago;
        private IGiftingService _giftService;
        private GiftSender _giftSender;
        private GiftReceiver _giftReceiver;
        private ICloseTraitParser<string> _closeTraitParser;

        public GiftSender Sender => _giftSender;

        public CrossGiftHandler()
        {
        }

        public void Initialize(ILogger logger, StardewArchipelagoClient archipelago, StardewItemManager itemManager, Mailman mail)
        {
            if (!archipelago.SlotData.Gifting)
            {
                return;
            }

            _logger = logger;
            _itemManager = itemManager;
            _mail = mail;
            _archipelago = archipelago;
            _giftService = new GiftingService(archipelago.GetSession());
            _giftSender = new GiftSender(_logger, _archipelago, _itemManager, _giftService);

            _giftService.OpenGiftBox(true, _desiredTraits);
            RegisterAllAvailableGifts();

            _giftReceiver = new GiftReceiver(_logger, _archipelago, _giftService, _itemManager, _mail, _closeTraitParser);
        }

        public bool HandleGiftItemCommand(string message)
        {
            if (_archipelago == null || !_archipelago.SlotData.Gifting)
            {
                return false;
            }

            var giftPrefix = $"{ChatForwarder.COMMAND_PREFIX}gift";
            var trapPrefix = $"{ChatForwarder.COMMAND_PREFIX}trap";
            var giftPrefixWithSpace = $"{giftPrefix} ";
            var trapPrefixWithSpace = $"{trapPrefix} ";
            var isGift = message.StartsWith(giftPrefixWithSpace);
            var isTrap = message.StartsWith(trapPrefixWithSpace);
            if (!isGift && !isTrap)
            {
                if (message.StartsWith(giftPrefix) || message.StartsWith(trapPrefix))
                {
                    Game1.chatBox?.addMessage($"Usage: !!gift [slotName]", Color.Gold);
                    return true;
                }
                return false;
            }

            var receiverSlotName = isTrap ? message[trapPrefixWithSpace.Length..] : message[giftPrefixWithSpace.Length..];
#if RELEASE
            if (receiverSlotName == _archipelago.SlotData.SlotName)
            {
                Game1.chatBox?.addMessage($"You cannot send yourself a gift", Color.Gold);
                return true;
            }
#endif
            _giftSender.SendGift(receiverSlotName, isTrap);
            return true;
        }

        public void ReceiveAllGiftsTomorrow()
        {
            if (_archipelago == null || !_archipelago.SlotData.Gifting || !_archipelago.MakeSureConnected())
            {
                return;
            }

            _giftReceiver.ReceiveAllGifts();
        }

        public void ExportAllGifts(string filePath)
        {
            var items = GetAllGiftAndTraitsByName();
            var objectsAsJson = JsonConvert.SerializeObject(items);
            File.WriteAllText(filePath, objectsAsJson);
        }

        private void RegisterAllAvailableGifts()
        {
            _closeTraitParser = new BKTreeCloseTraitParser<string>();
            var items = GetAllGiftAndTraitsByName();
            foreach (var (item, traits) in items)
            {
                _closeTraitParser.RegisterAvailableGift(item, traits);
            }
        }

        private Dictionary<string, GiftTrait[]> GetAllGiftAndTraitsByName()
        {
            var allItems = _itemManager.GetAllItems();
            var items = new Dictionary<string, GiftTrait[]>();
            foreach (var item in allItems)
            {
                var stardewItem = item.PrepareForGivingToFarmer();
                if (stardewItem is not Object stardewObject)
                {
                    continue;
                }

                if (!_giftSender.GiftGenerator.TryCreateGiftItem(stardewObject, false, out var giftItem, out var traits, out _))
                {
                    continue;
                }

                if (items.ContainsKey(giftItem.Name))
                {
                    continue;
                }
                items.Add(giftItem.Name, traits);
            }
            return items;
        }
    }
}
