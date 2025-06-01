using KaitoKid.ArchipelagoUtilities.Net.Client;

namespace StardewArchipelago.Items.Mail
{
    public class LetterBuffAttachment : LetterAttachment
    {
        public string BuffName { get; private set; }

        protected override bool IsEmptyLetter => false;

        public LetterBuffAttachment(ReceivedItem apItem, string buffName) : base(apItem)
        {
            BuffName = buffName;
        }

        public override void SendToPlayer(Mailman mailman)
        {
            var key = GetMailKey();
            mailman.SendArchipelagoMail(key, ArchipelagoItem.ItemName, ArchipelagoItem.PlayerName, ArchipelagoItem.LocationName, GetEmbedString());
        }

        public override MailKey GetMailKey()
        {
            return new MailKey(ArchipelagoItem.ItemName, ArchipelagoItem.PlayerName, ArchipelagoItem.LocationName, LetterActionsKeys.Buff, BuffName, GetEmbedString(), ArchipelagoItem.UniqueId.ToString(), IsEmptyLetter);
        }
    }
}
