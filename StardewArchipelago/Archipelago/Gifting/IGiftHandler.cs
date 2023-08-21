using StardewArchipelago.Items.Mail;
using StardewArchipelago.Stardew;
using StardewModdingAPI;

namespace StardewArchipelago.Archipelago.Gifting
{
    public interface IGiftHandler
    {
        void Initialize(IMonitor monitor, ArchipelagoClient archipelago, StardewItemManager itemManager, Mailman mail);
        bool HandleGiftItemCommand(string message);
        void ReceiveAllGiftsTomorrow();
    }
}