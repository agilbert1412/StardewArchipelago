using StardewArchipelago.Archipelago;

namespace StardewArchipelago.Items.Mail
{
    public class LetterMoneyAttachment : LetterAttachment
    {
        public int MoneyAmount { get; private set; }

        protected override bool IsEmptyLetter => false;

        public LetterMoneyAttachment(ReceivedItem apItem, int moneyAmount) : base(apItem)
        {
            MoneyAmount = moneyAmount;
        }

        public override string GetEmbedString()
        {
            return $"%item money {MoneyAmount} %%";
        }

        public override void SendToPlayer(Mailman mailman)
        {
            mailman.SendArchipelagoMail(GetMailKey(), ArchipelagoItem.ItemName, ArchipelagoItem.PlayerName, ArchipelagoItem.LocationName, GetEmbedString());
        }
    }
}
