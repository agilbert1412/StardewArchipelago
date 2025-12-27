using System;
using KaitoKid.Utilities.Interfaces;
using StardewArchipelago.Items.Mail;
using StardewArchipelago.Items.Traps;
using StardewArchipelago.Stardew;

namespace StardewArchipelago.Archipelago.Gifting
{
    public interface IGiftHandler : IDisposable
    {
        GiftSender Sender { get; }
        GiftReceiver Receiver { get; }
        void Initialize(ILogger logger, StardewArchipelagoClient archipelago, StardewItemManager itemManager, Mailman mail, GiftTrapManager giftTrapManager);
        bool HandleGiftItemCommand(string message);
        void ReceiveAllGiftsTomorrow();
        void ExportAllGifts(string filePath);
    }
}
