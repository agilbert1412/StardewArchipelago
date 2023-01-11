using System;
using StardewArchipelago.Archipelago;

namespace StardewArchipelago.Items.Mail
{
    public class LetterCustomAttachment : LetterAttachment
    {
        public Action LetterOpenedAction { get; private set; }

        public LetterCustomAttachment(ReceivedItem apItem, Action openedAction) : base(apItem)
        {
            LetterOpenedAction = openedAction;
        }

        public override void SendToPlayer(Mailman _mailman)
        {
            _mailman.SendArchipelagoMail(ArchipelagoItem.ItemName, ArchipelagoItem.PlayerName, ArchipelagoItem.LocationName, GetEmbedString());
        }
    }
}
