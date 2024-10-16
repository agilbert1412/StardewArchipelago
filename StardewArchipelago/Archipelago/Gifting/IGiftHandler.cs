﻿using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Items.Mail;
using StardewArchipelago.Stardew;

namespace StardewArchipelago.Archipelago.Gifting
{
    public interface IGiftHandler
    {
        GiftSender Sender { get; }
        void Initialize(ILogger logger, StardewArchipelagoClient archipelago, StardewItemManager itemManager, Mailman mail);
        bool HandleGiftItemCommand(string message);
        void ReceiveAllGiftsTomorrow();
        void ExportAllGifts(string filePath);
    }
}
