﻿using StardewArchipelago.Archipelago;

namespace StardewArchipelago.Items.Mail
{
    public class LetterItemIdAttachment : LetterAttachment
    {
        public int ItemId { get; private set; }
        public int AttachmentAmount { get; private set; }

        protected override bool IsEmptyLetter => false;

        public LetterItemIdAttachment(ReceivedItem apItem, int itemId, int attachmentAmount = 1) : base(apItem)
        {
            ItemId = itemId;
            AttachmentAmount = attachmentAmount;
        }

        public override string GetEmbedString()
        {
            return $"%item object {ItemId} {AttachmentAmount} %%";
        }

        public override void SendToPlayer(Mailman mailman)
        {
            mailman.SendArchipelagoMail(GetMailKey(), ArchipelagoItem.ItemName, ArchipelagoItem.PlayerName, ArchipelagoItem.LocationName, GetEmbedString());
        }
    }
}
