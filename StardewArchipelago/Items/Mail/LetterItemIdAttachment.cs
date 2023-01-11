using StardewArchipelago.Archipelago;
using StardewArchipelago.Stardew;

namespace StardewArchipelago.Items.Mail
{
    public class LetterItemIdAttachment : LetterAttachment
    {
        public int ItemId { get; private set; }
        public int AttachmentAmount { get; private set; }

        public LetterItemIdAttachment(ReceivedItem apItem, int itemId, int attachmentAmount = 1) : base(apItem)
        {
            ItemId = itemId;
            AttachmentAmount = attachmentAmount;
        }

        public override string GetEmbedString()
        {
            return $"%item {ItemId} {AttachmentAmount} %%";
        }

        public override void SendToPlayer(Mailman _mailman)
        {
            _mailman.SendArchipelagoMail(ArchipelagoItem.ItemName, ArchipelagoItem.PlayerName, ArchipelagoItem.LocationName, GetEmbedString());
        }
    }
}
