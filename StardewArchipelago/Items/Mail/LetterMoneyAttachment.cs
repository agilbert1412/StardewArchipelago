using StardewArchipelago.Archipelago;

namespace StardewArchipelago.Items.Mail
{
    public class LetterMoneyAttachment : LetterAttachment
    {
        public int MoneyAmount { get; private set; }

        public LetterMoneyAttachment(ReceivedItem apItem, int moneyAmount) : base(apItem)
        {
            MoneyAmount = moneyAmount;
        }

        public override string GetEmbedString()
        {
            return $"%item money {MoneyAmount} %%";
        }

        public override void SendToPlayer(Mailman _mailman)
        {
            _mailman.SendArchipelagoMail(GetMailKey(), ArchipelagoItem.ItemName, ArchipelagoItem.PlayerName, ArchipelagoItem.LocationName, GetEmbedString());
        }
    }
}
