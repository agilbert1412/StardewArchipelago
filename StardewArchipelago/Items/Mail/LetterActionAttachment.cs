using StardewArchipelago.Archipelago;

namespace StardewArchipelago.Items.Mail
{
    public class LetterActionAttachment : LetterAttachment
    {
        public string LetterOpenedAction { get; private set; }
        public string ActionParameter { get; private set; }

        public LetterActionAttachment(ReceivedItem apItem, string openedAction, string parameter = "") : base(apItem)
        {
            LetterOpenedAction = openedAction;
            ActionParameter = parameter;
        }

        public override void SendToPlayer(Mailman _mailman)
        {
            var key = GetMailKey();
            _mailman.SendArchipelagoMail(key, ArchipelagoItem.ItemName, ArchipelagoItem.PlayerName, ArchipelagoItem.LocationName, GetEmbedString());
        }

        public override string GetMailKey()
        {
            var key = $"AP|{ArchipelagoItem.ItemName}|{ArchipelagoItem.PlayerName}|{ArchipelagoItem.LocationName}|{LetterOpenedAction}|{ActionParameter}";
            var trimmedKey = key.Replace(" ", "_");
            return trimmedKey;
        }
    }
}
