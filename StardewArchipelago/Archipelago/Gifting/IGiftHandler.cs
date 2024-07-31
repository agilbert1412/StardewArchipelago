using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Items.Mail;
using StardewArchipelago.Stardew;

namespace StardewArchipelago.Archipelago.Gifting
{
    public interface IGiftHandler
    {
        GiftSender Sender { get; }
        void Initialize(ILogger logger, ArchipelagoClient archipelago, StardewItemManager itemManager, Mailman mail);
        bool HandleGiftItemCommand(string message);
        void ReceiveAllGiftsTomorrow();
        void ExportAllGifts(string filePath);
    }
}
