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
    }
}
